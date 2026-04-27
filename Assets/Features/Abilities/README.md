# Features Abilities

Этот документ фиксирует текущее состояние системы `Features/Abilities`, её поток данных и важные ограничения, найденные при разборе.

## Общая модель

Система abilities построена как server-authoritative модуль:

- клиент только инициирует каст;
- сервер проверяет готовность, стоимость и cooldown;
- сервер строит `AbilityContext`;
- сервер исполняет эффекты способности;
- клиент получает только нужный runtime-state для UI: cooldown и channel state.

Это хорошая базовая схема для gameplay-логики, потому что источник истины остаётся на сервере.

## Структура слоёв

### Domain

- `AbilitySO` - статическое описание способности.
- `AbilityContext` - runtime-контекст каста.
- `AbilityCastType` - тип каста (`Instant` / `Channel`).

### Application

- `AbilityService` - основная логика проверки каста, стоимости, cooldown и channel lifecycle.

### UnityIntegration

- `AbilityCasterNetAdapter` - входная сетeвая точка от клиента к серверу.
- `AbilityCaster` - серверный runtime-узел игрока, который держит список способностей, cooldown sync и вызов исполнения.
- `AbilityTickSystem` - общий server tick для всех `AbilityCaster`.
- `AbilityExecutor` - исполняет ability effects на сервере.

### UI

- `ClientAbilityView` - клиентское представление доступных способностей.
- `AbilityHUD` - HUD энергии и channel progress.
- `AbilitySlotUI` - отдельный слот способности, иконка, cooldown, tooltip.

## Поток каста

1. `AbilityInputHandler` ловит нажатие слота.
2. `AbilityCasterNetAdapter.Cast()` отправляет `ServerRpc`.
3. `AbilityCaster.TryCastWithContext()` выбирает способность по индексу.
4. `AbilityService.TryCast()` проверяет:
   - готовность;
   - отсутствие active channel;
   - cooldown;
   - наличие energy;
   - успешное списание cost.
5. `AbilityService` строит `AbilityContext`.
6. Для `Instant`-каста способность сразу переходит к исполнению.
7. Для `Channel`-каста сервис запускает канал и завершает его позже по server tick.
8. `AbilityCaster` собирает runtime effects, применяет ability modifiers из passives и вызывает `EffectExecutor`.

## Как считается стоимость

Способность хранит базовую стоимость в `AbilitySO.energyCost`.

Финальная цена считается в `AbilityService` так:

`finalCost = ability.energyCost * _energy.CostMultiplier`

То есть abilities сейчас используют `energy`, а не отдельную `mana`-систему.

## Проверка `costModifier` на минус mana cost

### Что работает

Уменьшение стоимости работает корректно через `energy.cost.mult`, если модификатор задаётся как multiplier меньше `1`.

Пример:

- `0.8` = способность стоит 80% от базовой цены;
- `0.5` = способность стоит 50% от базовой цены.

Это работает потому что:

- `MultiplyStatEffectSO` вызывает `stats.TryMultiply(...)`;
- `StatsFacade` передаёт модификатор в `EnergyStats`;
- `EnergyStats.TryMultiply()` поддерживает ключ `energy.cost.mult`;
- `AbilityService` реально использует `CostMultiplier` при списании energy.

Старый эффект `EnergyCostMultipillerEffect.asset` с `multiplier: 0.5` должен уменьшать стоимость корректно.

### Что не работает

`AddStatEffectSO` для `energy.cost.mult` сейчас не влияет на стоимость abilities.

Причина:

- `AddStatEffectSO` вызывает `stats.TryAdd(...)`;
- `EnergyStats.TryAdd()` не обрабатывает `StatKeys.EnergyCostMult`;
- значит additive-эффект на `energy.cost.mult` фактически игнорируется.

Следствие:

- `EnergyCostMultAddEffect.asset` сейчас является no-op для реального mana/energy cost;
- делать "минус манакост" через additive-эффект в текущем коде нельзя;
- делать "минус манакост" нужно только через multiply-эффект.

### Ограничение по нижней границе

`EnergyStats` зажимает `CostMultiplier` в диапазон `0.1 .. 10`.

Следствие:

- стоимость нельзя опустить ниже 10% от базовой;
- бесплатный каст через штатный multiplier-path сейчас невозможен.

## Интеграция с passives

Пассивки не меняют сам `AbilitySO`.

Вместо этого `AbilityCaster`:

- берёт базовый список `ability.effects`;
- строит runtime-копии;
- прогоняет через cached `AbilityModifierSO` из `PassiveSystem`;
- исполняет уже модифицированный runtime-набор эффектов.

Это хорошее решение, потому что asset-данные способности остаются стабильными, а runtime-модификации применяются только на сервере.

## Сетевое состояние

На клиент сейчас синхронизируются:

- cooldown по слотам через `SyncList<float> Cooldowns`;
- `NetIsChanneling`;
- `NetChannelSlot`;
- `NetChannelRemaining`.

Этого достаточно для текущего HUD, но это именно presentation-state, а не полная ability-domain синхронизация.

## Найденные ограничения и риски

### 1. `Channel`-способности, вероятно, исполняют эффекты дважды

Сейчас `AbilityCaster.TryCastWithContext()` вызывает `ExecuteWithModifiers()` сразу после успешного `TryCast()`.

При этом для `Channel`-каста после завершения канала вызывается `OnChannelFinished()`, который снова вызывает `ExecuteWithModifiers()`.

Если ability имеет `castType = Channel`, её gameplay-эффекты, скорее всего, исполняются:

- один раз в момент старта;
- второй раз в момент завершения канала.

Это выглядит как баг и требует отдельной правки.

### 2. `ability.cooldown` как stat key пока не подключён к runtime abilities

В `StatKeys` есть ключ `ability.cooldown`, и под него уже существуют buff effects.

Но в runtime abilities cooldown всё ещё берётся напрямую из `AbilitySO.cooldown`, а не из stats facade или отдельного cooldown stat-модуля.

Следствие:

- buff/debuff на `ability.cooldown` сейчас сам по себе не меняет реальный cooldown способности.

### 3. Валидация стоимости завязана только на `EnergyStats`

Сейчас способность умеет работать только через `IEnergyStats`.

Если позже появятся разные типы ресурсов, придётся расширять контракт abilities, а не только менять data.

## Текущее состояние

На текущий момент ability-система выглядит рабочей и достаточно чисто разделённой:

- input отделён от server execution;
- ability execution идёт только на сервере;
- UI получает только нужный sync-state;
- cost multiplier реально влияет на energy cost;
- passives могут модифицировать эффекты без мутации исходного `AbilitySO`.

Но важно помнить:

- минус mana cost сейчас корректно работает только через `MultiplyStatEffectSO` для `energy.cost.mult`;
- additive-модификатор на `energy.cost.mult` не работает;
- `Channel`-касты требуют отдельной проверки/фикса на двойное исполнение;
- `ability.cooldown` как stat key пока не подключён к runtime cooldown pipeline.
