# Процедурный мир: текущий анализ

Этот документ фиксирует текущее состояние подсистемы процедурно сгенерированного мира в `Assets/World/Biomes` и её связь с enemy LOD из `Assets/Features/Enemy/Scripts/.../LOD`.

## Коротко

- Мир строится от сетевого `seed`: по сети передаются seed и флаг готовности мира, а terrain и procedural spawn локально пересчитываются из одинаковых правил.
- Базовая единица жизненного цикла мира — `Chunk`: он владеет terrain mesh, collider, procedural spawn и выгрузкой.
- Биом влияет не только на высоту terrain, но и на material, fog, water, atmosphere, environment, resources и enemies.
- Enemy LOD не генерирует мир, а оптимизирует уже заспавненных в этом мире врагов.

## Сквозной поток данных

1. Игрок вызывает `PlayerSessionNetwork.RequestWorldServerRpc(worldConfigId, questIds, chainIds)`.
2. Сервер сам генерирует seed, складывает его вместе с `worldConfigId` и стартовым quest payload в `ServerWorldSession`.
3. При старте world-сцены `WorldProvider` вызывает `ServerWorldSession.ConsumeBootstrap()` и публикует `Seed`, `WorldConfigId`, `HasBootstrap` и `IsWorldReady` как `SyncVar`.
4. `RuntimeWorldGenerator` на сервере и клиентах ждёт `WorldProvider`, читает `Seed` и `WorldConfigId`, резолвит нужный `WorldConfig`, записывает seed в `worldConfig.seed` и поднимает runtime мира.
5. Дальше строится `BiomeRuntimeDatabase`, создаётся `ChunkManager`, запускается потоковая загрузка чанков и procedural spawn.
6. После первой серверной генерации вызывается `WorldProvider.SetWorldReady()`.
7. `WorldReadyRuntime` ждёт этот флаг и только потом пускает дальше игровой flow.

Архитектурно это хорошая схема: по сети не реплицируется целая карта, а синхронизируется источник детерминизма и момент готовности мира.

## Ядро подсистемы

### 1. Конфиги мира и биомов

- `WorldConfig` задаёт `seed`, `chunkSize`, набор `BiomeLayer[]` и общие параметры мира.
- `BiomeConfig` хранит terrain-параметры, environment/resources/enemies, fog, water, weather и визуальные настройки биома.
- `WorldConfig.GetBiomeBlend()` вычисляет смешивание биомов по world position.
- `WorldConfig.GetHeight()` получает blended-высоту и дополнительно выравнивает безопасную стартовую площадку около `(0, 0)`.

Итог: мир здесь хранится не как готовая карта, а как набор функций и конфигов, которые можно воспроизводить из seed.

### 2. Генерация terrain

- `BiomeHeightUtility` переводит тип биома в конкретную функцию рельефа: hills, mountains, dunes, canyons, fractal mountains и т.д.
- `MeshDataGenerator` строит heightmap чанка и смешивает вклад нескольких биомов.
- `TerrainMeshGenerator` собирает `Mesh`.
- `Chunk.GenerateLOD()` подготавливает несколько LOD-мешей terrain.
- `ChunkMeshLOD` на клиенте переключает mesh terrain по дистанции до локального игрока.

Высота terrain считается по blend нескольких биомов, но material/spawn-правила чанка в основном опираются на доминирующий биом.

### 3. Стриминг чанков

- `ChunkManager` управляет созданием, очередью загрузки, обновлением и выгрузкой чанков.
- На клиенте мир стримится вокруг локального игрока.
- На сервере мир теперь стримится вокруг всех активных игроков, а не только вокруг хоста.
- Очередь `loadQueue` и защита от повторного enqueue не дают плодить дубликаты загрузки одного и того же чанка.
- При выходе чанка из рабочей области вызывается `Chunk.Unload()`, и runtime-данные чанка освобождаются вместе с ним.

Это важный уже доведённый участок: именно multi-player server streaming убрал проблему, когда удалённый от хоста клиент проваливался из-за того, что сервер не держал коллайдеры и сетевой runtime вокруг него.

### 4. Процедурный спавн содержимого

- `BiomeRuntimeDatabase` переводит `BiomeConfig` в компактные runtime rules для environment/resources/enemies/quests.
- `InstanceRegistry` связывает `prefabInstanceID` с prefab/mesh/material и теперь дополнительно хранит локальную матрицу render mesh внутри prefab и root scale.
- `Chunk.RunMegaSpawn()` запускает `MegaSpawnJob`.
- `MegaSpawnJob` сэмплирует вершины terrain, считает slope/normal и выдаёт `SpawnInstance`.
- `MegaSpawnScheduler` забирает результаты job и разводит их по runtime-путям спавна.

Разделение по типам сейчас такое:

- `EnvironmentInstanced` идёт в `ChunkedInstanceLODSystem` и хранится по чанкам.
- `ResourceGameObject`, `EnemyGameObject`, `QuestGameObject` создаются через `RuntimeSpawnerSystem`.
- `InstancedSpawnerSystem` остался как fallback/legacy-ветка совместимости, если chunked instancing в конкретной сцене не подключён.

### 5. Детерминизм `MegaSpawnJob`

`MegaSpawnJob` не использует «каждый раз новый случайный» random. Его раскладка детерминирована:

- базовый seed приходит из сетевого `WorldProvider`;
- `Chunk.RunMegaSpawn()` смешивает его с координатами чанка;
- внутри job псевдорандом дополнительно стабилизируется sampled vertex / локальными индексами.

Это значит, что при одинаковом `world.seed` и одинаковом наборе правил environment/resources/enemies должны раскладываться одинаково на сервере и клиентах.

## Что уже доведено

### 1. Серверный streaming больше не привязан к хосту

`RuntimeWorldGenerator` разделён на server-side и client-side поток обновления:

- сервер использует отдельный `ChunkManager` и обновляет чанки вокруг всех игроков;
- клиент использует свой локальный `ChunkManager` только вокруг локального player target;
- host-client путь больше не перетирает серверный manager клиентским.

Именно это исправило ситуацию, когда далеко от хоста клиент терял поддержку мира и проваливался.

### 2. `ChunkedInstanceLODSystem` реально завершён

Раньше ветка chunked instancing выглядела незавершённой. Сейчас она собрана в рабочий pipeline:

- environment-инстансы регистрируются по координатам чанка;
- они попадают в `ChunkRuntimeData`;
- `ChunkedInstanceLODSystem` каждый кадр рисует инстансы активных чанков;
- при выгрузке чанка его instanced environment тоже выгружается.

То есть environment теперь живёт в том же chunk lifecycle, что и остальной procedural runtime, а не отдельной висящей веткой.

### 3. Исправлена геометрия instanced environment

Для instanced environment теперь учитываются:

- реальная локальная матрица mesh renderer внутри prefab;
- root scale prefab;
- автоматическое создание instanced-копии материала, если исходный material был без `enableInstancing`.

Это убрало два реальных класса проблем:

- `DrawMeshInstanced` больше не падает на материалах без заранее включённого instancing;
- environment перестал чаще «висеть» над землёй из-за того, что раньше в instancing-матрицу не попадала настоящая внутренняя трансформация render mesh.

### 4. Исправлен порядок сетевого спавна ресурсов

Для `RuntimeSpawnerSystem` поправлен порядок выставления transform:

- финальные `position/rotation/scale` теперь вычисляются до сетевого `Spawn()`;
- ресурс сначала сажается на землю и получает итоговый масштаб;
- только после этого вызывается сетевой spawn.

Дополнительно введён safe fallback масштаба: если из job/конфига приходит `scale <= 0`, используется `1f`.

Это устранило баг, при котором хост мог не видеть руду, а клиент видел: сетевой объект уже рождался на клиенте, но сервер потом локально перетирал transform, включая нулевой scale из некорректного конфига.

### 5. Выбор `WorldConfig` и набор готовых миров

`WorldGeneratorUI` теперь передаёт только выбранный `WorldConfig` и стартовые quest ids. Сам seed генерируется на сервере, затем публикуется через `WorldProvider`, а `RuntimeWorldGenerator` на обеих сторонах поднимает один и тот же preset мира.

Сейчас в проекте заведены готовые world preset:

- `Dust Frontier` — базовая красная пустыня с пылью, водой в низинах и акцентом на минералы.
- `Crater Fields` — мир с кратерным рельефом, плохой обзорностью и более жёсткой атмосферой.
- `Crystal Hollows` — холодный кристаллический мир с каньонами, биолюминесцентным тоном и акцентом на кристаллы.
- `Toxic Mire` — болотный токсичный мир с водой, зелёным fog-профилем и более тяжёлой атмосферой.
- `Ruined Expanse` — сухой мир руин и плато, рассчитанный под более «древний» визуальный тон.

Архитектурно это важно, потому что мир теперь выбирается как полноценный runtime preset, а не только как другой `seed` поверх одного и того же набора биомов.

## Визуальный runtime биомов

После генерации terrain биом продолжает управлять окружением:

- `BiomeFog` смешивает fog-параметры активных биомов вокруг игрока.
- `BiomeAtmosphereController` переключает skybox и погодные prefab-эффекты.
- `AdvancedWaterPlane` двигает water plane за игроком и подбирает water material/уровень воды по текущему биому.

Поэтому биом здесь — не просто функция высоты, а контейнер и геометрии, и атмосферы, и spawn-логики.

## Как сюда подключён Enemy LOD

Enemy LOD находится рядом с генерацией мира, но не внутри неё:

- `EnemyDistanceLODSystem` переключает `LOD0/LOD1/LOD2` по дистанции.
- `EnemyInstancingController` может уводить далёких врагов в GPU instancing.
- `EnemyLogicLODAdapter` ограничивает дорогую дальнюю логику.
- `EnemyVisualController` перевязывает animator на активную LOD-модель.

С точки зрения архитектуры это downstream-слой оптимизации для врагов, которых сначала должен породить biome/world runtime.

## Что архитектурно уже выглядит сильным

- Сетевой seed-поток короткий и понятный.
- Terrain и procedural spawn воспроизводятся из общих правил, а не вручную реплицируются.
- Есть хорошее разделение на Data / Application / UnityIntegration.
- Чанк уже является реальной единицей жизненного цикла: load, spawn, render, unload.
- Instanced environment теперь встроен в chunk lifecycle, а не живёт отдельно.

## Что ещё выглядит неполностью состыкованным

### 1. Quest/chain payload в `ServerWorldSession`

После разделения bootstrap-части и payload `WorldProvider.OnStartServer()` больше не съедает quest/chain списки вместе с seed, но сам подход всё ещё остаётся stateful и зависит от корректного жизненного цикла `ServerWorldSession` между world-запросом и фактической раздачей стартовых квестов.

### 2. Enemy performance/population уже встроены в runtime, но ещё требуют балансировки

- `EnemyPerformanceManager.LodScale` и `EnemyCountScale` подключены к реальным расчётам дальности и популяции.
- `BiomeEnemySpawner` теперь учитывает global/per-player/per-biome лимиты, `enemyRespawnDelay`, вес и условия spawn entry, а также может работать с runtime-выбранным `WorldConfig`.
- `EnemyBiomeCounter`, `EnemyAutoUnregister`, `EnemyLogicLODAdapter` и ECS despawn bridge уже работают в одном контуре, поэтому проблема была уже не в отсутствии интеграции, а в живой настройке частот, лимитов и дистанций despawn.
- Отдельный практический риск здесь теперь не архитектурный, а геймдизайнерский: spawn cadence и лимиты нужно докрутить в Play Mode под реальную нагрузку.

### 3. `WorldSession` пока не является источником истины для world pipeline

`Assets/Features/Game/Domain/WorldSession.cs` хранит `WorldVersion` и `Seed`, но фактический рабочий pipeline мира сейчас строится через `ServerWorldSession` + `WorldProvider` + `WorldConfig.seed`.

### 4. Data model местами богаче, чем текущее runtime-применение

В `BiomeConfig` и `BiomeRuntimeDatabase` уже сериализуется больше ограничений и правил, чем реально доходит до полного применения в `MegaSpawnJob` и downstream runtime. Это не ломает мир, но важно помнить: модель данных уже шире, чем фактическое поведение части spawn-пайплайна.

## Итог

Подсистема процедурного мира уже собрана как рабочий каркас:

- seed синхронизируется;
- terrain строится детерминированно;
- сервер и клиент стримят чанки по своим ролям;
- instanced environment встроен в chunk lifecycle;
- world preset выбирается из UI и синхронно поднимается на сервере и клиентах;
- биомы управляют и геометрией, и визуалом, и spawn-правилами;
- enemy LOD работает как downstream-оптимизация поверх мира.

Самая зрелая часть системы сейчас — это связка `seed -> WorldProvider -> RuntimeWorldGenerator -> ChunkManager -> Chunk -> MegaSpawn`.

Главные текущие зоны риска уже не в самой генерации рельефа, а в интеграции жизненного цикла и соседних подсистем:

- владение session payload для quests/chains;
- балансировка enemy population/performance в Play Mode;
- разрыв между богатой data model биомов и тем, что полностью применяется в runtime.
