# CoreGameplay Effects

Документ описывает текущую систему эффектов в `CoreGameplay/Effects` и идеи предметов, которые можно собрать на базе уже готовых эффектов.

## Где это используется

Эффекты подключаются к предметам через `Features/Items/Data/Item.cs`:

- `Item.actions[]` содержит действия предмета: `Primary`, `Secondary`, `Reload`, `Alt`;
- каждое действие хранит тайминги (`cooldown`, `tickInterval`, `windupTime`, `burstCount`, `burstInterval`) и массив `EffectDefinition[]`;
- экипировка берет активный предмет из `Features/Equipment`, создает runtime для действия и запускает эффекты на сервере;
- `equippedBuffs` и `upgrades` применяются при экипировке через `EquipmentItemBuffApplier`.

Текущие готовые предметы:

- `Drill` - добыча ресурсов через `MineNetworkResource`, небольшой урон и impact FX.
- `Scaner` - скан ресурсов через `ScanResourceEffect`.
- `Weapon`, `W 2`, `Laser` - стрельба через `SpawnProjectile`, impact FX и звук.

## Поток выполнения

1. Предмет вызывает `ItemRuntimeContext.StartUse`.
2. Runtime обновляет точку выстрела/наведения через `UpdateAim`.
3. На server tick выполняются эффекты из `ItemActionDefinition.effects`.
4. `EffectExecutor` резолвит цели через `TargetResolver`.
5. `EffectFactory` создает конкретный эффект и вызывает `Apply`.

Система сервер-авторитетная: `EffectExecutor` и большинство эффектов ничего не делают на клиенте.

## TargetMode

| Режим | Что делает | Типичные предметы |
| --- | --- | --- |
| `Self` | Целью становится сам источник, если он является `IBuffTarget` | аптечка, стимулятор, щит, временный бафф |
| `Area` | Ищет цели в радиусе через `Physics.OverlapSphere` | граната, импульс, аура, ремонтная зона |
| `Directional` | Делает raycast вперед на `radius` | дрель, луч, оружие, сканер направления |
| `Explicit` | Использует цели, переданные в контексте | projectile hit, спец-логика |

Дополнительные фильтры:

- `ownership`: `Any`, `SameOwner`, `DifferentOwner`;
- `coneAngle`: оставляет цели в конусе перед игроком;
- `selectClosest`: выбирает ближайшую цель.

## Готовые эффекты

| EffectType | Реализация | Что делает | Полезные поля |
| --- | --- | --- | --- |
| `DealDamage` | `DealDamageEffect` | Наносит урон найденным целям, учитывает combat stats источника, крит, множитель урона, сопротивления и penetration | `value`, `damageType`, `targetMode`, `radius`, `layerMask` |
| `HitscanDamage` | `HitscanDamageEffect` | Делает raycast и наносит урон первой цели | `value`, `radius`, `layerMask`, `damageType` |
| `HealInstant` | `HealInstantEffect` | Мгновенно лечит цели | `value`, `targetMode`, `radius`, `ownership` |
| `ApplyBuff` | `ApplyBuffEffect` | Вешает `BuffSO` на цели с lifetime `Duration` | `buff`, `targetMode`, `ownership` |
| `RemoveBuffSource` | `RemoveBuffSourceEffect` | Снимает все баффы от текущего source или конкретный `buffId` | `onlySpecificBuff`, `buffId` |
| `SpawnPrefab` | `SpawnPrefabEffect` | Спавнит prefab из `SpawnPrefabRegistry` по `prefabId` | `prefabId`, `lifetime`, `useSourcePosition` |
| `SpawnPrefabOnLayerEffect` | `SpawnPrefabOnLayerEffect` | Есть код эффекта спавна над целями, но он не подключен в `EffectFactory` и enum | `prefabId`, `lifetime`, `heightOffset` |
| `MineNetworkResource` | `MineNetworkResourceEffect` | Добывает `ResourceNodeNetwork` у найденных целей | `value`, `radius`, `layerMask` |
| `Continuous` | `ContinuousEffect` | Запускает повтор child effects с интервалом | `tickInterval`, `childEffects` |
| `StopContinuous` | `StopContinuousEffect` | Останавливает continuous effect для source | source из контекста |
| `Scan` | `ScanEffect` | Вызывает `IScannable.OnScanned(strength)` у целей | `value`, `targetMode`, `radius` |
| `ScanResourceEffect` | `ScanResourceEffect` | Ищет resource nodes в радиусе и спавнит маркер над ними | `prefabId`, `radius`, `layerMask`, `lifetime`, `heightOffset` |
| `SpawnProjectile` | `SpawnProjectileEffect` | Спавнит серверный projectile или делает server-side hitscan с клиентской визуализацией | `projectileConfig` |
| `SpawnImpact` | `SpawnImpactEffect` | Спавнит impact FX в точке попадания из `HitEffectContext` | `impactFxId` |
| `PlaySound` | `PlaySoundEffect` | Проигрывает звук через `ImpactFxDispatcher` | `soundConfig` |

В enum также есть `DealDamageHitscan` и `MeleeDamage`, но сейчас они не обработаны в `EffectFactory`.

## ProjectileConfig

`ProjectileConfig` задает поведение для `SpawnProjectile`:

- `useServerProjectile` - реальный сетевой projectile или instant hitscan;
- `projectilePrefab` - серверный projectile;
- `clientProjectilePrefab` - локальная FPS-визуализация;
- `visualType` - `Projectile`, `Trail`, `Laser`;
- `speed`, `lifetime`, `useGravity`;
- `damage`, `damageType`, `hitMask`, `destroyOnHit`, `hitEffect`.

Это уже закрывает пули, трассеры, лазеры, медленные снаряды, минометные дуги и энергетические заряды.

## Готовые device behaviours

| Behaviour | Что дает |
| --- | --- |
| `TurretBehaviour` | Автономная турель, ищет врагов в радиусе и наносит урон с учетом своих stats |
| `ShieldGridBehaviour` | Триггер-зона, отталкивает врагов rigidbody-силой |
| `RepairDroneBehaviour` | Дрон, который следует за владельцем |
| `OverloadPulseBehaviour` | Визуальный импульс, может следовать за владельцем |
| `DamageZone` | Триггер-зона периодического урона |

Эти behaviours удобнее всего использовать через `SpawnPrefab`.

## Идеи предметов на текущих эффектах

### Можно собрать почти без нового кода

| Предмет | Категория | Эффекты | Идея настройки |
| --- | --- | --- | --- |
| Mining Laser | Tool | `Continuous` -> `MineNetworkResource` + `SpawnImpact` | Дальняя добыча лучом, меньше tick damage чем у дрели, но безопаснее |
| Pulse Drill | Tool / Melee | `MineNetworkResource` + `DealDamage` в конусе | Дрель, которая одновременно добывает и отпугивает врагов рядом с жилой |
| Med Injector | Tool | `HealInstant` self | Быстрое лечение с большим cooldown |
| Repair Beacon | Throwable / Tool | `SpawnPrefab` prefab с зоной или дроном | Ставит ремонтный маяк на время |
| Stim Pack | Tool | `ApplyBuff` self | Временный бафф скорости, fire rate или regen через существующие `BuffSO` |
| Cleanser | Tool | `RemoveBuffSource` self | Снимает негативные эффекты, если они помечены source/id |
| Shock Baton | Melee | `DealDamage` area/cone + `ApplyBuff` | Ближний электрический удар с замедлением/станом через buff |
| Scatter Blaster | Weapon | несколько `SpawnProjectile` в одном действии | Дробовик или веер энергетических зарядов |
| Railgun | Weapon | `HitscanDamage` + `SpawnImpact` + `PlaySound` | Мощный точный выстрел с windup |
| Beam Rifle | Weapon | `Continuous` -> `HitscanDamage` / `DealDamage` | Луч с уроном по удержанию кнопки |
| Grenade | Throwable | `SpawnProjectile` с gravity + impact prefab | Метательный снаряд, взрыв через prefab/DamageZone |
| Scan Beacon | Scanner / Throwable | `SpawnPrefab` + `ScanResourceEffect` | Ставит маяк, который подсвечивает ресурсы вокруг |
| Enemy Scanner | Scanner | `Scan` или `ApplyBuff` area | Вешает reveal/debuff на врагов в радиусе |
| Overload Emitter | Tool | `SpawnPrefab` pulse + `DealDamage`/`ApplyBuff` area | Импульс вокруг игрока, урон/замедление врагов |
| Shield Projector | Tool | `SpawnPrefab` shield grid | Временная зона, отталкивающая врагов |
| Auto Turret Kit | Throwable / Tool | `SpawnPrefab` turret | Ставит временную турель |
| Acid Sprayer | Weapon | `Continuous` -> `DealDamage` cone, `damageType: Acid` | Конусный распылитель ближней дистанции |
| Cryo Projector | Weapon | `ApplyBuff` cone + `DealDamage` Frost | Замедляет и наносит холодный урон |
| Fire Lance | Weapon | `Continuous` -> `DealDamage` cone, `damageType: Fire` | Огнемет без отдельного кода, если визуал сделать prefab/VFX |
| Resource Ping | Scanner | `ScanResourceEffect` | Дешевый сканер с малым радиусом и коротким cooldown |

### Хорошо ложится на систему, но нужна небольшая доработка

| Предмет | Что добавить |
| --- | --- |
| Trap Mine | Подключить prefab с trigger logic или расширить `SpawnPrefab` установкой rotation/позиции по hit point |
| Aura Totem | Prefab с `AreaBuffEmitter` или отдельный reusable zone prefab |
| Buff Grenade | Projectile impact должен уметь выполнять child effects по area в точке взрыва |
| Chain Lightning Gun | Новый effect для прыжка между несколькими ближайшими целями |
| Pull/Black Hole Device | Area effect с force к центру, аналогично `ShieldGridBehaviour`, но с обратным направлением |
| Deployable Damage Zone | Уже есть `DamageZone`, но надо проверить prefab/network setup и registry id |
| Target Marker Gun | Effect, который помечает цель buff'ом и усиливает входящий урон от всех источников |
| Ammo/Charge System | Сейчас `reloadTime` есть в data, но runtime не обрабатывает `Reloading`; для оружия с магазином нужна доработка runtime |

## Быстрые комбинации эффектов

### Оружие с ударом и звуком

```text
Primary:
  SpawnProjectile(projectileConfig)
  SpawnImpact(impactFxId)
  PlaySound(soundConfig)
```

### Лечение игрока

```text
Primary:
  HealInstant(targetMode: Self, value: 25)
```

### Аура урона вокруг игрока

```text
Primary:
  Continuous(tickInterval: 0.5)
    child: DealDamage(targetMode: Area, ownership: DifferentOwner, radius: 4)

Alt / Stop:
  StopContinuous
```

### Скан ресурсов

```text
Primary:
  ScanResourceEffect(targetMode: Self, radius: 100, prefabId: scan-item, lifetime: 3, heightOffset: 1.5)
```

### Временный deployable

```text
Primary:
  SpawnPrefab(prefabId: turret/shield/drone, lifetime: 20, useSourcePosition: false)
```

## Что стоит поправить в системе

- Подключить `SpawnPrefabOnLayerEffect` в `EffectType` и `EffectFactory`, если нужен спавн маркеров/эффектов над целями.
- Убрать или реализовать неиспользуемые enum-значения `DealDamageHitscan` и `MeleeDamage`.
- В `EffectDefinition` есть `duration` и `coneDistance`, но сейчас они почти не участвуют в логике.
- `MineNetworkResourceEffect` хранит `_range` и `_mask`, но фактически работает по уже найденным targets.
- Для `ContinuousEffectRuntime` стоит подумать о копировании контекста, если continuous должен следить за актуальным aim/position.
- `ItemActionDefinition.reloadTime` и состояние `Reloading` пока не используются в `ItemRuntimeContext`.
