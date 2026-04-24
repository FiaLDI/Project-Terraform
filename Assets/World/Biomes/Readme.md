# World/Biomes README

Актуальное описание подсистемы процедурного мира, биомов, safe-zone, ресурсов, мобов, маяка выхода, checkpoint spawnpoint и загрузочного экрана.

## Коротко

`World/Biomes` отвечает за:

- выбор биома по world position;
- расчет высоты terrain;
- генерацию mesh/collider чанков;
- runtime spawn объектов мира;
- запрет спавна в стартовой safe-zone;
- постановку специального exit beacon для возврата в Hub;
- постановку checkpoint trigger для персонального spawnpoint игрока.

Сейчас важная идея такая: terrain, fallback height, spawn logic и safe-zone должны опираться на один источник правды - `WorldConfig`.

## Основные файлы

- `Data/WorldConfig.cs` - seed, биомы, высота, safe-zone, loading background.
- `Utility/BiomeHeightUtility.cs` - расчет высоты биома с учетом seed.
- `Application/MeshDataGenerator.cs` - mesh/collider чанка.
- `Application/Chunk.cs` - подготовка spawn job для чанка.
- `UnityIntegration/MegaSpawnJob.cs` - основной spawn job объектов/ресурсов/мобов/квестовых объектов.
- `Application/RuntimeSpawnerSystem.cs` - применение spawn point результата в runtime.
- `UnityIntegration/Spawn/BiomeEnemySpawner.cs` - динамический спавн мобов около игроков.
- `Application/WorldPlacementService.cs` - поиск walkable/reachable точки для специальных объектов.
- `UnityIntegration/WorldExitBeacon.cs` - trigger возврата всех игроков в Hub.
- `UnityIntegration/WorldCheckpointTrigger.cs` - trigger персонального spawnpoint игрока.
- `UnityIntegration/Net/RuntimeWorldGenerator.cs` - orchestration runtime world generation, exit beacon и checkpoint.

## Seed

`worldConfig.seed` теперь участвует не только в `MegaSpawnJob`, но и в:

- выборе биома через `WorldConfig.GetBiomeAtWorldPos`;
- расчете высоты через `BiomeHeightUtility.GetHeight`;
- blend высот между биомами.

Следствие: смена seed должна менять не только раскладку объектов, но и сам мир: terrain, biome distribution и spawn layout.

## Height Pipeline

Высоту нужно брать через `WorldConfig.GetHeight(...)`, если коду нужна фактическая высота мира.

Текущий путь:

1. `WorldConfig.GetHeight(float2 worldPos)`.
2. Выбор/смешивание биомов с учетом seed.
3. `BiomeHeightUtility.GetHeight(..., seed)`.
4. Применение safe-zone flatten/blend.

`MeshDataGenerator` строит mesh/collider через тот же `WorldConfig.GetHeight(...)`. Это важно: визуальный terrain, физический terrain и fallback height теперь не должны расходиться концептуально.

## Safe-Zone

Safe-zone централизована в `WorldConfig`.

Поля:

- `safeSpawnFlatRadius` - полностью плоская зона без спавна ресурсов/мобов/объектов.
- `safeSpawnBlendRadius` - внешний радиус плавного перехода terrain от плоской зоны к обычному рельефу.
- `safeSpawnHeightOffset` - высота стартовой платформы над базовой высотой центра.

Правило:

- внутри `safeSpawnFlatRadius` нельзя спавнить ресурсы, мобов, окружение и квестовые объекты;
- между `safeSpawnFlatRadius` и `safeSpawnBlendRadius` terrain плавно возвращается к обычной высоте;
- `safeSpawnBlendRadius` должен быть больше или равен `safeSpawnFlatRadius`.

Чтобы уменьшить safe-zone, настрой в `WorldConfig`:

- `safeSpawnFlatRadius`, например `25`;
- `safeSpawnBlendRadius`, например `40`.

Не стоит делать blend меньше flat radius.

## Spawn Ресурсов И Руды

Ресурсы теперь рассматриваются как статичные world resource, а не как физический drop item.

Причина: дроп предметов использует другой prefab, а руда/ресурс в мире должна быть статичной целью для добычи киркой.

Текущее поведение:

- spawn ресурсов идет через server-authoritative путь;
- `RuntimeSpawnerSystem` для `SpawnKind.ResourceGameObject` вызывает настройку статичного ресурса;
- `WorldItemNetwork` получил режим `staticWorldSpawn`;
- Rigidbody у world resource заморожен: kinematic, без gravity, freeze constraints;
- collider остается, потому что по нему работает удар киркой.

Практический смысл: ресурс не должен падать, просыпаться физикой, по-разному оказываться на host/client или съезжать с позиции.

## Почему Руда Раньше Могла Быть Не На Земле

Основные причины были такие:

- spawn point мог рассчитываться отдельно от итогового terrain mesh/collider;
- safe-zone height раньше была не единым путем для всех потребителей;
- у resource prefab был Rigidbody, который мог менять позицию после spawn;
- host/client могли по-разному получить физическое состояние объекта.

Сейчас это закрывается так:

- terrain mesh/collider и fallback height используют `WorldConfig.GetHeight`;
- resource Rigidbody переводится в статичный режим;
- safe-zone проверяется в spawn job;
- ресурсы не должны физически досимулироваться в разные стороны на разных машинах.

## Spawn В Safe-Zone

`MegaSpawnJob` получает из `Chunk`:

- центр safe-zone;
- flat radius;
- blend radius.

Если base spawn point попадает в flat safe-zone, spawn отменяется. Для resource clusters дополнительно проверяются точки внутри cluster, чтобы отдельная руда не появилась внутри стартовой зоны.

`BiomeEnemySpawner` тоже проверяет safe-zone:

- не пытается спавнить, если игрок находится внутри flat safe-zone;
- отбрасывает candidate hit point внутри flat safe-zone.

Итог: мобы и ресурсы не должны появляться в стартовой safe-zone.

## Exit Beacon

Exit beacon нужен для выхода из procedural world обратно в Hub.

Настройка находится в `RuntimeWorldGenerator`:

- `spawnExitBeacon` - включить/выключить spawn beacon;
- `exitBeaconPrefab` - prefab маяка, который можно собрать и настроить вручную;
- `exitBeaconMinDistanceFromSpawn`;
- `exitBeaconMaxDistanceFromSpawn`;
- `exitBeaconTriggerRadius`;
- `exitBeaconPlacementAttempts`.

Beacon ставится через `WorldPlacementService`:

- вне flat safe-zone;
- на walkable terrain;
- с проверкой достижимости от spawn area;
- не внутри chunk/LOD контейнера, чтобы не пропадать вместе с LOD;
- parent в Play Mode: `RuntimeWorldGenerator / WorldExitBeacon`.

Если `exitBeaconPrefab` не назначен, создается fallback primitive, но для production лучше назначать свой prefab.

Важно: beacon prefab сейчас не обязан быть network prefab. Генератор создает копии локально, а server-side копия получает trigger logic.

## Как Собрать Prefab Exit Beacon

Минимальный prefab:

1. Создай пустой root object, например `PF_ExitBeacon`.
2. Добавь видимую модель/эффект/свет как child.
3. Не добавляй `NetworkObject`, если не переводишь систему на сетевой spawn prefab.
4. Collider на root можно не добавлять вручную: `RuntimeWorldGenerator` добавит server trigger, если его нет.
5. Если добавляешь collider сам, сделай его trigger или убедись, что trigger radius задается генератором.
6. Назначь prefab в `RuntimeWorldGenerator.exitBeaconPrefab`.
7. Настрой `exitBeaconTriggerRadius`.

Когда все игроки находятся внутри trigger radius, `WorldExitBeacon` вызывает возврат всех игроков в Hub через `SceneTransitionService.ReturnAllPlayersToHub()`.

## Checkpoint Spawnpoint

Checkpoint нужен, чтобы персональный spawnpoint игрока привязывался при прохождении через объект.

Текущий путь:

- `RuntimeWorldGenerator` может создать checkpoint route marker;
- на server-side объект добавляется `WorldCheckpointTrigger`;
- когда игрок проходит trigger, вызывается `PlayerSpawnRegistry.SetPlayerSpawnPoint(...)`;
- `SpawnService` при respawn сначала пытается взять персональный spawnpoint игрока;
- `PlayerSessionNetwork.RequestReturnToSpawnServerRpc` тоже использует owner client id.

Spawnpoint очищается при scene cleanup / возврате в Hub.

## Loading Screen

Загрузочный UI сделан как ручной UI-компонент, который можно поставить на свой Canvas и оставить через DontDestroyOnLoad.

Файл:

- `UI/LoadingScreenService.cs`

Поля:

- `root`;
- `dontDestroyOnLoad`;
- `hideDelay`;
- `backgroundImage`;
- `hubBackground`;
- `fallbackBackground`;
- `title`;
- `subtitle`.

Фон Hub:

- можно назначить прямо в `LoadingScreenService.hubBackground`;
- или через `ServerBootstrap.hubLoadingBackground`.

Фон процедурного мира:

- назначается в `WorldConfig.loadingBackground`;
- `WorldGeneratorUI` регистрирует backgrounds из `worldSelectionCatalog`;
- при генерации вызывает `LoadingScreenService.ShowWorld(selectedEntry.worldConfig, ...)`.

Задержка скрытия:

- `LoadingScreenService.hideDelay`;
- по умолчанию сейчас `1` секунда, чтобы переход `Hub <-> World` не исчезал мгновенно.

## Quest: First Blood

Для убийств был исправлен источник kill credit.

`EnemyStats` теперь пытается достать реального владельца урона из:

- `StatsBuffTarget.OwnerSource`;
- `ItemRuntimeSource.OwnerSource`;
- `RuntimeBuffSource.Owner`;
- `Component`;
- `NetworkPlayer` root.

Это нужно, чтобы personal quest вроде `First Blood` засчитывался игроку, даже если kill source пришел не напрямую от player component.

Важно: если `First Blood` находится внутри quest chain не первым шагом, он не будет выполняться до активации этого шага chain.

## Диагностика Если Beacon Не Видно

Проверь:

1. `RuntimeWorldGenerator.spawnExitBeacon = true`.
2. `RuntimeWorldGenerator.exitBeaconPrefab` назначен или разрешен fallback.
3. В Play Mode ищи объект под `RuntimeWorldGenerator / WorldExitBeacon`.
4. В Console должен быть log вида `[WorldGen] Exit beacon placed at ...`.
5. `exitBeaconMinDistanceFromSpawn` и `exitBeaconMaxDistanceFromSpawn` не слишком большие для доступной области мира.
6. `exitBeaconPlacementAttempts` достаточно большой, например `64`.
7. Safe-zone не перекрывает всю область поиска.

## Диагностика Если Руда Опять Ведет Себя Странно

Проверь:

1. У world resource prefab нет активной динамической физики.
2. Collider есть и доступен кирке.
3. Для drop item используется отдельный prefab.
4. Resource prefab проходит через `SpawnKind.ResourceGameObject`.
5. На объекте есть `WorldItemNetwork` или он корректно конфигурируется через `RuntimeSpawnerSystem`.
6. Resource не является child объекта, который отключается LOD/chunk cleanup.

## Checklist Setup

Перед тестом procedural world:

- В `WorldConfig` настроить `seed`, биомы, `safeSpawnFlatRadius`, `safeSpawnBlendRadius`.
- В `WorldConfig.loadingBackground` назначить картинку мира.
- В `RuntimeWorldGenerator` назначить `exitBeaconPrefab`.
- Проверить `exitBeaconTriggerRadius`.
- Проверить `spawnExitBeacon`.
- Проверить checkpoint settings, если нужен персональный spawnpoint.
- На scene/persistent Canvas поставить `LoadingScreenService`.
- В `LoadingScreenService` назначить `root`, `backgroundImage`, `title`, `subtitle`, `hubBackground`.
- У resource prefab оставить collider, но убрать динамическую физику.

