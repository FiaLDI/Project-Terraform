# Quests Status

Этот файл фиксирует текущее состояние ветки `Assets/Features/Quests` после последних правок.

## Коротко

Сейчас квестовая система работает как нормальный серверный контур:

1. `QuestAsset` и `QuestChainAsset` собираются в runtime-определения.
2. `PlayerQuestComponent` на сервере держит `QuestService` и `QuestChainService`.
3. События летят через `QuestEventBus`.
4. Состояние режется в `QuestNetState` и реплицируется через `SyncDictionary`.
5. Клиентские UI (`QuestUIRuntime`, `QuestDebugUI`) рисуют список квестов из сетевого состояния.

Отдельно важно: разделение на личные и глобальные квесты уже заложено в `QuestScope` и это нормальная часть текущей архитектуры, а не недоделка.

## Что уже исправлено

### 1. Исправлена опасная отписка в `QuestEventBus`

Файл: `Assets/Features/Quests/Scripts/Application/QuestEventBus.cs`

Раньше `Unsubscribe` сравнивал только `Method`, из-за чего отписка одного игрока могла снести подписки других объектов с тем же обработчиком. Сейчас хранится оригинальный delegate instance, и удаляется только он.

### 2. Исправлен reset lifecycle

Файлы:

- `Assets/Features/Quests/Scripts/UnityIntegration/PlayerQuestComponent.cs`
- `Assets/Features/Quests/Scripts/Application/QuestService.cs`
- `Assets/Features/Quests/Scripts/Application/QuestChainService.cs`

`ClearAll()` теперь чистит не только active quests, но и:

- `rewarded`
- `advancedChains`
- chain state
- сетевой словарь `Quests`

`QuestService.ResetQuest()` и `FailQuest()` теперь также корректно убирают квест из `_completed`.

### 3. Квесты переживают смерть и respawn

Файлы:

- `Assets/Features/Multiplayer/Scripts/Domain/PlayerSession.cs`
- `Assets/Features/Quests/Scripts/Domain/QuestPersistenceState.cs`
- `Assets/Features/Quests/Scripts/UnityIntegration/PlayerQuestComponent.cs`
- `Assets/Features/Quests/Scripts/Application/QuestService.cs`
- `Assets/Features/Quests/Scripts/Application/QuestChainService.cs`

Состояние квестов теперь сохраняется в `PlayerSession` и восстанавливается в новом `PlayerQuestComponent` после respawn:

- активные квесты
- прогресс по условиям
- `QuestState`
- chain progression
- уже выданные награды
- уже продвинутые шаги цепочек

### 4. По сети теперь идёт полноценный `QuestState`

Файлы:

- `Assets/Features/Quests/Scripts/Domain/QuestStateNet.cs`
- `Assets/Features/Quests/Scripts/UI/QuestUIRuntime.cs`
- `Assets/Features/Quests/Scripts/UI/Debug/QuestDebugItemUI.cs`

Раньше клиент видел только `completed / not completed`. Сейчас реплицируется `QuestState`, поэтому UI уже различает:

- `Active`
- `Completed`
- `Failed`

### 5. Починен `Clear All` в debug UI

Файлы:

- `Assets/Features/Quests/Scripts/UI/Debug/QuestDebugListUI.cs`
- `Assets/Features/Quests/Scripts/UI/QuestUIRuntime.cs`

UI теперь умеет:

- восстанавливать уже существующие записи при `Init()`
- корректно обрабатывать `SyncDictionaryOperation.Clear`

Из-за этого `Clear All` больше не должен оставлять на экране зависшие элементы после фактического сброса данных.

### 6. Завершение квеста теперь даёт XP, а не прямой level up

Файлы:

- `Assets/Features/Quests/Scripts/Domain/QuestModel.cs`
- `Assets/Features/Quests/Scripts/Data/QuestAsset.cs`
- `Assets/Features/Quests/Scripts/UnityIntegration/PlayerQuestComponent.cs`
- `Assets/Features/Progress/Scripts/Application/PlayerProgressService.cs`
- `Assets/Features/Progress/Scripts/Domain/PlayerProgressionRules.cs`
- `Assets/Features/Multiplayer/Scripts/Domain/PlayerSession.cs`
- `Assets/Features/Multiplayer/Scripts/UnityIntegration/Net/ConnectionObject.cs`
- `Assets/Features/Multiplayer/Scripts/UnityIntegration/Server/ServerLoginHandler.cs`

Что изменилось:

- у `QuestDefinition` появился `ExperienceReward`
- квест больше не повышает уровень напрямую
- завершение квеста выдаёт XP
- уровень растёт только когда XP достигает порога
- текущий `level/experience` теперь протягивается через login и хранится в `PlayerSession`
- при награде квестом обновляются и серверный `session`, и локальный player save

Сейчас порог уровня считается общей формулой из `PlayerProgressionRules`.

### 7. В HUD добавлен маленький UI прогресса уровня

Файл: `Assets/Features/Quests/Scripts/UI/QuestUIRuntime.cs`

`QuestUIRuntime` теперь сам создаёт небольшую runtime-плашку с:

- текущим `LVL`
- текущим `XP / required XP`
- полосой прогресса до следующего уровня

Плашка создаётся кодом, так что не требует отдельной ручной сборки prefab только ради первого запуска.

## Текущее поведение

### Сервер

- `PlayerQuestComponent.OnStartServer()` поднимает сервисы и либо восстанавливает квесты из `PlayerSession`, либо выдаёт стартовые.
- Все изменения квестов складываются в `pendingUpdates`, затем попадают в `Quests`.
- При завершении квеста сервер выдаёт item rewards и XP.
- XP обновляет текущий `PlayerSession`, чтобы respawn не откатывал уровень.
- `PlayerStats.SetLevel()` теперь вызывается после пересчёта уровня от XP.

### Клиент

- `QuestUIRuntime` слушает `Quests.OnChange` и рисует HUD/journal.
- `QuestDebugUI` работает поверх того же сетевого словаря.
- `QuestUIRuntime` также слушает `PlayerProgressService.ActiveCharacterChanged` и обновляет маленький XP HUD.
- Локальный `PlayerProgressService` сохраняет новый `level/experience` после квестовой награды.

## Оставшиеся проблемы и странности

### 1. Возможен race в `HaveItemCondition`

Файл: `Assets/Features/Quests/Scripts/UnityIntegration/Conditions/HaveItemCondition.cs`

Если инвентарь ещё не готов в момент старта условия, стартовая проверка может не засчитать уже имеющийся предмет. Тогда прогресс обновится только после следующего изменения инвентаря.

Симптом:

- игрок уже имеет нужный предмет
- квест стартует
- условие остаётся на `0/N`, пока инвентарь снова не изменится

### 2. Кэши баз данных не инвалидируются

Файлы:

- `Assets/Features/Quests/Scripts/Data/QuestDatabaseAsset.cs`
- `Assets/Features/Quests/Scripts/Data/QuestChainDatabaseAsset.cs`

Кэш определений строится лениво и потом не пересобирается. В runtime это терпимо, но в editor/dev-цикле можно получить старые данные после изменения `QuestAsset` или `QuestChainAsset`.

### 3. Слои и namespace местами расходятся

Пример:

- `Assets/Features/Quests/Scripts/Application/QuestService.cs`

Файл лежит в `Application`, но объявлен в `Features.Quests.Domain`. Runtime это не ломает, но навигацию по коду и границы слоёв делает менее очевидными.

### 4. XP reward сейчас задаётся просто и может потребовать балансировки

Файл: `Assets/Features/Quests/Scripts/Data/QuestAsset.cs`

Если у квеста явно не задан `experienceReward`, сейчас используется fallback:

- `50 XP * количество условий`

Это удобно для существующих ассетов, чтобы они сразу начали давать XP без ручной правки каждого файла, но потом это почти наверняка захочется перебалансировать руками по конкретным квестам.

## Что проверить руками в Unity

1. Завершить квест и убедиться, что растёт XP, а уровень меняется только после достижения порога.
2. Проверить, что после level up и последующего respawn уровень не откатывается назад.
3. Проверить маленький XP HUD в обычном игровом UI.
4. В debug UI выдать несколько квестов, нажать `Clear All`, закрыть и открыть окно заново.
5. В мультиплеере проверить, что respawn одного игрока не ломает квесты другого.
6. Проверить отображение `Failed` на обычном UI и в debug UI.

## Что я бы правил следующим

1. Закрыть race в `HaveItemCondition` через повторный initial sync или inventory-ready hook.
2. Сделать явную editor-валидацию или массовую настройку `experienceReward` для реальных quest assets.
3. Привести namespace и папки к одному слою ответственности.
4. Если понадобится, вынести формулу XP/level из кода в отдельный progression config asset.

## Итог

Сейчас ветка `Features/Quests` уже в рабочем состоянии и стала заметно цельнее:

- lifecycle стал чище
- reset стал честнее
- respawn больше не должен сбрасывать квесты
- квесты перешли с прямого level up на XP progression
- появился маленький UI прогресса уровня
- UI стал синхроннее с реальным сетевым состоянием

Остаточные проблемы сейчас уже не в базовом survival системы, а в качестве данных, editor/dev-поведения и балансировке XP-наград.
