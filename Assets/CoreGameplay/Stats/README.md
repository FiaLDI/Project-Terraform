# CoreGameplay Stats

Этот документ фиксирует актуальное состояние системы `CoreGameplay/Stats` после правок по owner-предустановкам, reset-контрактам и пресетам.

## Общая модель

Система stats построена как фасад над typed-подсистемами, а не как один универсальный словарь:

- `Health`
- `Energy`
- `Combat`
- `Movement`
- `Mining`
- `Protect`

Центральная точка входа - `StatsFacade`. Он держит ссылки на конкретные stat-модули и маршрутизирует `TryAdd` / `TryMultiply` в первый подходящий `IStatModifierTarget`.

Это дает три важных свойства:

- бафы и эффекты работают через `StatKey`, а не через прямое знание структуры статов;
- каждая подсистема сама отвечает за свой набор ключей;
- серверная логика stats остается отделенной от клиентского UI/view-слоя.

## Структура слоев

### Domain

- `StatsFacade`
- `StatKey` / `StatKeys`
- `HealthStats`
- `EnergyStats`
- `CombatStats`
- `MovementStats`
- `MiningStats`
- `ProtectStats`

### Data

- `StatsProfileSO` - какие stat-блоки вообще есть у owner.
- `StatsPresetSO` - базовые значения player/class stats.
- `EnemyStatsPresetSO` - базовые значения enemy stats.
- `TurretPresetSO` - базовые значения turret stats.

### UnityIntegration

- `StatsOwnerBase` - создает stat-модули и собирает `StatsFacade`.
- `PlayerStats`, `EnemyStats`, `TurretStats` - накладывают базовые значения конкретного owner.
- `UnifiedStatsUpdateSystem` - серверный regen loop.
- `StatsNetSync` - HP/energy snapshot sync.
- `MovementStatsSync` - отдельный sync movement-параметров.

### Adapter

`StatsFacadeAdapter` и его дочерние adapters - это view-слой. Он не является источником истины, а только принимает уже подготовленные значения для UI/presentation.

## Поток данных

1. `StatsOwnerBase.OnStartServer()` вызывает `InitStats()`.
2. По `StatsProfileSO` создаются только нужные stat-подсистемы.
3. Собирается `StatsFacade`.
4. Owner-класс применяет базовые значения из соответствующего preset/config.
5. Бафы и эффекты модифицируют значения через `IStatsFacade.TryAdd` / `TryMultiply`.
6. `UnifiedStatsUpdateSystem` тикает regen на сервере.
7. `StatsNetSync` и `MovementStatsSync` передают клиенту нужное подмножество state.
8. Клиентские adapters прокидывают данные дальше в presentation/UI.

## Как stats связаны с бафами

Интеграция уже чистая и устойчивая:

- `AddStatEffectSO` вызывает `stats.TryAdd(new StatKey(statId), value)`;
- `MultiplyStatEffectSO` вызывает `stats.TryMultiply(new StatKey(statId), multiplier)`;
- `BuffExecutor` не знает внутреннюю реализацию `HealthStats`, `CombatStats` и т.д.;
- stat-модуль сам решает, поддерживает он конкретный `StatKey` или нет.

Это одна из сильнейших сторон текущей архитектуры.

## Источники базовых значений

После правок базовые значения теперь опираются на presets заметно последовательнее:

- `PlayerStats` сначала применяет `defaultPreset`, а затем class preset, если он пришел позже.
- `EnemyStats` берет значения из `EnemyConfigSO.stats`, а при отсутствии конфига использует безопасный fallback без обращения к `null`.
- `TurretStats` теперь читает combat-параметры из `TurretPresetSO.combat`, а не держит их только в коде.

Следствие:

- owner-классы все еще управляют lifecycle stat-модулей;
- но сами численные базовые значения теперь заметно ближе к data-driven подходу;
- hardcoded числа остаются только как fallback, а не как основной путь конфигурации.

## Что было исправлено

### 1. Починен `PlayerStats.ApplyPreset()`

Раньше combat preset передавал аргументы в `CombatStats.ApplyBase()` со сдвигом, из-за чего `range/spread/aimSpread` попадали не в те поля.

Сейчас combat-параметры передаются по именам и в правильном порядке.

### 2. Починен fallback в `EnemyStats`

Раньше в fallback-ветке можно было дойти до обращения к `config.stats` даже в сценарии, где `config.stats == null`.

Сейчас:

- preset-ветка и fallback-ветка явно разделены;
- `ApplyPreset()` использует именно переданный `preset`, а не внешний `config.stats`.

### 3. `MovementStats.Reset()` теперь полный

Теперь reset действительно очищает:

- base
- add
- mult

включая `gravity` и `jumpHeight`.

### 4. `ProtectStats.Reset()` теперь полный

Теперь reset обнуляет не только add/mult, но и base resistance-поля.

### 5. Owner-базовые статы стали более data-driven

Ключевые изменения:

- у `PlayerStats` есть `defaultPreset`;
- `Player.prefab` теперь ссылается на общий stats preset;
- у `TurretPresetSO` появился реальный `combat` block, который используется в runtime;
- turret preset asset обновлен под новый формат.

## Что важно помнить про сеть

Сетевой слой по-прежнему синхронизирует stat-модули не одинаково, и это считается нормальным текущим контрактом системы.

### `StatsNetSync`

Синхронизирует:

- `health`
- `maxHealth`
- `energy`
- `maxEnergy`

Особенности:

- работает по интервалу `syncInterval`;
- использует `threshold` для фильтрации мелких изменений;
- на клиенте делает плавное приближение текущих значений;
- использует `StatApplyGuard` для аккуратного переапплая max-значений.

### `MovementStatsSync`

Синхронизирует:

- walk
- sprint
- crouch
- rotation
- gravity
- jumpHeight

Но он намеренно работает отдельно и вручную через `SendSnapshot()`, а не как такой же периодический поток, как HP/energy.

Это не считается дефектом системы, а текущим архитектурным решением.

## Текущее состояние системы

На текущий момент `Stats` выглядят как достаточно зрелая серверная подсистема:

- есть typed domain-модели;
- есть единый фасад для модификаторов;
- есть понятное разделение owner/domain/network/view;
- бафы уже интегрируются со stats без жесткой связности.

После последних правок главные явные проблемы были убраны:

- preset application стал корректнее;
- reset-контракты выровнены;
- базовые owner values стали ближе к asset-driven конфигурации.

Дальше систему уже можно развивать не как “спасение от багов”, а как нормальную платформу для новых механик.
