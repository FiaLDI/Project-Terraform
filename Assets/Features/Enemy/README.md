# Enemy Subsystem Guide

Этот документ описывает текущую enemy-подсистему после последних `P0/P1` правок: как собрать врага, какие кейсы уже поддерживаются, какие параметры на что влияют и что ещё остаётся долгом.

## Current state

Подсистема сейчас работает как hybrid:

- ECS отвечает за perception, LOS, aggro, targeting, AI state, move goal и despawn marking.
- MonoBehaviour-bridge связывает ECS с Unity physics, FishNet, animation, LOD и effect execution.
- `EnemyConfigSO` управляет stats, AI, combat и render-настройками.

Это уже рабочая config-driven система. Старый монолитный `EnemyAIJob` убран, и базовый brain теперь разложен на несколько ECS systems:

- `EnemyAISystem`: perception pre-pass, cooldown update, frame lock reset
- `EnemyPatrolStateSystem`
- `EnemyChaseStateSystem`
- `EnemyAttackStateSystem`
- `EnemyReturnStateSystem`

Это ещё не полный behavior graph, но менять и расширять поведение теперь заметно проще.

## Current priorities

### P0

- Прогнать Unity compile + Play Mode smoke test после правок в combat, targeting и animation flow.
- Проверить реальные боевые сценарии с 2+ игроками:
  - aggro от обычного урона;
  - aggro от hitscan;
  - aggro от projectile damage;
  - target switching под разной дистанцией и threat.
- Проверить контент-миграцию:
  - у врагов должен быть заполнен `combat.attackEffect`;
  - у ranged-врагов должен быть корректный `combat.attackType`;
  - при использовании override должен быть заполнен `render.animatorController`.
- Проверить LOS layer masks на префабах и конфигах, чтобы враги не стреляли "сквозь всё" и не теряли LOS из-за лишних слоёв.

### P1

- Перевести контент на новые distance-настройки:
  - `preferredCombatDistance`
  - `retreatDistance`
  - `reengageDistance`
  - `attackMoveGoalTolerance`
- Подобрать `EnemyNetworkSync` thresholds под реальные толпы врагов и observer counts.
- При необходимости добавить отдельный beam/laser flow:
  - `Aim`
  - `Charge`
  - `FireLoop`
- При необходимости дальше дробить brain на ещё более узкие слои:
  - отдельный perception memory layer
  - отдельный attack request layer
  - отдельный move goal layer
- Решить, нужен ли отдельный despawn fallback в ECS, если `EnemyDespawnBridge` отсутствует.

## What is supported right now

### Supported well

- Melee-враги с patrol -> chase -> attack -> return.
- Ranged-враги через `attackEffect`.
- Legacy enemy content без полной миграции:
  - если `combat.attackType = None`, runtime попытается определить `Melee` или `Ranged` автоматически;
  - если `combat.attackEffect` не заполнен, `EnemyAttackHandler` использует свой старый serialized effect как fallback.
- Aggro от реально нанесённого урона.
- Threat-based target selection с target stickiness.
- Vision range + vision cone + LOS.
- LOD-отключение части дорогой логики.
- Runtime-override animator controller из `EnemyRenderConfigSO`.
- Damage parity на runtime-уровне:
  - `attackDamage` прокидывается в damage-based effect execution.
  - Поддержаны `DealDamage`, `HitscanDamage`, `ChainDamage`, `SpawnProjectile`.

### Supported with caveats

- Laser / beam enemy:
  - поддерживается как hitscan или projectile-based effect;
  - не поддерживается как отдельная state machine с charge / sweep / track.
- Custom animation sets:
  - хорошо работают, если Animator использует стандартные параметры;
  - всё ещё завязаны на текущий visual bridge, а не на отдельную animation domain model.

## Practical cases

### Ranged enemy cases

- Short-range caster:
  - `attackType = Ranged`
  - средний `attackRange`
  - `attackDelay` как cast time
  - `attackEffect.type = DealDamage` или `HitscanDamage`
- Laser turret:
  - `attackType = Ranged`
  - большой `attackRange`
  - узкий `visionAngle`
  - `attackEffect.type = HitscanDamage`
- Projectile shooter:
  - `attackType = Ranged`
  - `attackEffect.type = SpawnProjectile`
  - корректный `ProjectileConfig`
- Sniper:
  - `attackType = Ranged`
  - длинный `attackRange`
  - узкий `visionAngle`
  - большой `attackDelay`
  - повышенный `preferredCombatDistance`
- Kiting shooter:
  - `attackType = Ranged`
  - `preferredCombatDistance > 0`
  - `retreatDistance > 0`
  - `reengageDistance > 0`
  - работает уже сейчас, но без отдельной специализированной ranged-brain ветки

### Animation cases

- Базовый humanoid controller:
  - `Speed` float
  - `Attack` trigger
- Controller с явным moving state:
  - `Speed` float
  - `IsMoving` bool
  - `Attack` trigger
- Controller с разными melee/ranged attack clips:
  - `Attack` trigger
  - `AttackType` int
  - значения:
    - `0 = None`
    - `1 = Melee`
    - `2 = Ranged`
- Heavy monster:
  - можно использовать только idle / run / attack;
  - walk state необязателен.
- Charge-up ranged animation:
  - можно смоделировать через attack clip + `attackDelay`;
  - отдельных параметров `Aim`, `Charge`, `FireLoop` пока нет.

## Runtime flow

1. `EnemyConfigSO` назначается на prefab или приходит из spawner.
2. `EnemyEcsRuntimeBinder` создаёт ECS entity и применяет AI/combat/render-related runtime config.
3. `EnemyTargetingSystem` выбирает цель по threat + proximity + current target bias.
4. `EnemyLOSSystemECS` проверяет vision cone и LOS.
5. `EnemyAISystem` собирает perception context и обновляет cooldown/frame lock.
6. `EnemyPatrolStateSystem`, `EnemyChaseStateSystem`, `EnemyAttackStateSystem`, `EnemyReturnStateSystem` обновляют state и move goal.
7. `EnemyEcsMoveBridge` двигает Rigidbody по ECS steering-настройкам.
8. `EnemyVisualController` обновляет animation parameters и запускает attack playback.
9. `EnemyAttackHandler` исполняет `attackEffect`.
10. `DealDamageEffect` добавляет threat обратно в enemy aggro pipeline.

## How to assemble a new enemy

### 1. Create configs

Создай и свяжи между собой:

- `EnemyAIConfigSO`
- `EnemyCombatConfigSO`
- `EnemyRenderConfigSO`
- `EnemyStatsPresetSO`
- `EnemyConfigSO`

В `EnemyConfigSO` назначь:

- `ai`
- `combat`
- `render`
- `stats`

### 2. Prepare prefab

Минимальный набор компонентов:

- `NetworkObject`
- `Rigidbody`
- `EnemyEcsRuntimeBinder`
- `EnemyEcsMoveBridge`
- `EnemyAttackHandler`
- `EnemyVisualController`
- `EnemyActor`
- `EnemyStats`
- `StatsBuffTarget`
- `BuffSystem`
- `EnemyDistanceLODSystem`
- `EnemyLODView`
- `EnemyLogicLODAdapter`

Рекомендуемые компоненты:

- `EnemyNetworkSync`
- `EnemyDespawnBridge`
- `EnemyHealthBarUI`

### 3. Configure visuals

На LOD model prefab должен быть Animator.

Поддерживаемые animator parameters:

- `Speed` float: опционально
- `IsMoving` bool: опционально
- `Attack` trigger: рекомендуется
- `AttackType` int: опционально для melee/ranged attack branching

Если в `EnemyRenderConfigSO.animatorController` назначен controller, `EnemyVisualController` применит его runtime-ом при смене активной модели.

### 4. Configure combat

`EnemyCombatConfigSO`:

- `attackType`
  - `Melee` для ближнего боя
  - `Ranged` для дальнего
  - `None` допустим как legacy fallback, runtime попытается определить тип автоматически
- `attackRange`
  - максимальная дистанция удара
- `attackDamage`
  - runtime damage override для поддержанных effect paths
- `attackCooldown`
  - интервал между атаками
- `attackDelay`
  - задержка от attack trigger до реального effect execution
- `attackEffect`
  - effect, который реально исполняется
  - если он не заполнен, runtime попытается использовать старый serialized effect из `EnemyAttackHandler`
- `attackEnterOffset`
  - насколько раньше враг может войти в attack state
- `attackExitOffset`
  - насколько позже он выйдет из attack state
- `stopDistanceMultiplier`
  - базовая предпочтительная дистанция боя, если `preferredCombatDistance` не задан

### 5. Configure AI and behavior

`EnemyAIConfigSO`:

#### Aggro

- `aggroRadius`
  - дистанция поиска/удержания близких целей
- `loseAggroRadius`
  - максимальная дистанция, после которой цель теряется
- `threatDecayPerSecond`
  - скорость затухания накопленного threat
- `targetSwitchThreshold`
  - насколько новая цель должна быть лучше текущей, чтобы переключиться
- `currentTargetBias`
  - бонус удержания текущей цели
- `aggroConfirmTime`
  - сколько времени нужно видеть цель до входа в chase

#### Vision

- `visionAngle`
  - угол обзора
- `visionRange`
  - дальность зрения
- `requireLineOfSight`
  - нужен ли LOS

#### Movement

- `moveSpeed`
  - линейная скорость движения
- `rotationSpeed`
  - скорость поворота

#### Steering weights

- `seekWeight`
  - притяжение к цели
- `avoidWeight`
  - сила обхода препятствий
- `separationWeight`
  - сила разлёта от соседей
- `orbitWeight`
  - сила бокового орбитирования

#### Steering distances

- `avoidDistance`
  - дистанция фронтальной проверки препятствия
- `sideAvoidDistance`
  - дистанция боковых raycast-проверок
- `separationRadius`
  - радиус соседей для separation

#### Movement feel

- `orbitStrength`
  - насколько сильно враг стремится смещаться вбок
- `directionSmoothing`
  - насколько быстро сглаживается направление движения

#### Brain

- `lostSightGraceTime`
  - сколько враг терпит потерю LOS в attack state
- `attackMoveGoalTolerance`
  - допустимое отклонение от preferred combat distance
- `returnReachDistance`
  - радиус, в котором враг считает возврат завершённым
- `preferredCombatDistance`
  - желаемая дистанция удержания вокруг цели
- `retreatDistance`
  - если цель слишком близко, враг пытается отступить
- `reengageDistance`
  - дистанция, на которой враг возвращается из chase к attack

#### Toggles

- `enableSeparation`
- `enableAvoidance`
- `enableOrbit`

### 6. Configure LOS mask

`EnemyEcsRuntimeBinder.obstacleMask` лучше задавать явно.

Если он не задан, runtime сейчас использует `Physics.DefaultRaycastLayers` как безопасный fallback. Это лучше, чем `~0`, но всё равно не заменяет явную настройку project layers.

Рекомендуемая практика:

- Environment / Walls / Obstacles включены
- Player hitboxes выключены
- Enemy hitboxes выключены, если они не должны блокировать LOS

## Damage behavior

Сейчас runtime делает так:

- `attackEffect` определяет тип атаки.
- `attackDamage` прокидывается поверх effect definition в runtime.

Что уже переопределяется:

- `DealDamage.value`
- `HitscanDamage.value`
- `ChainDamage.value`
- `SpawnProjectile.projectileConfig.damage`

Это значит, что `attackDamage` уже реально влияет на итоговый урон, но всё равно стоит проверить старые контент-ассеты, если они раньше полагались на damage внутри самого effect asset.

## Recommended recipes

### Melee bruiser

- `attackType = Melee`
- `attackRange = 2.2`
- `preferredCombatDistance = 1.2`
- `retreatDistance = 0`
- `reengageDistance = 1.8`
- `attackEffect.type = DealDamage`

### Laser sentinel

- `attackType = Ranged`
- `attackRange = 14`
- `attackDelay = 0.6`
- `visionAngle = 40`
- `preferredCombatDistance = 10`
- `retreatDistance = 6`
- `reengageDistance = 12`
- `attackEffect.type = HitscanDamage`

### Projectile scarab

- `attackType = Ranged`
- `attackRange = 10`
- `attackDelay = 0.35`
- `preferredCombatDistance = 7`
- `retreatDistance = 4`
- `reengageDistance = 8`
- `attackEffect.type = SpawnProjectile`

## Desert monster ideas

- Sand Scorpion
- Crystal Basilisk
- Dune Hunter
- Sun Scarab
- Obsidian Golem
- Dust Shaman
- Heat Spider
- Ash Vulture
- Sand Ray
- Glass Burrower

## Still open after this pass

- Brain уже разбит по state systems, но это ещё не полностью event-driven graph.
- Нет отдельного beam charge / track / sweep state machine.
- `EnemyDespawnCleanupSystem` всё ещё можно усилить fallback-логикой.
- `EnemyNetworkSync` уже легче, но требует реального профилирования.
- Namespace-структура врагов по проекту всё ещё плавает.
