# Документация

## Назначение

**`QuestAssetCreator`** — это утилитарный редакторский скрипт (`Editor Utility`), который добавляет пункт в меню Unity **Assets → Create → Quests → New Quest Asset**.
Он используется для **быстрого создания нового квеста** (`QuestAsset`) прямо из редактора без необходимости вручную создавать ScriptableObject.

---

## Расположение

**Путь:**

```
Assets/Scripts/Quests/Editor/QuestAssetCreator.cs
```

> Скрипт должен находиться в папке **Editor**, чтобы корректно компилироваться только в редакторе Unity (и не попадать в сборку билда).

---

## Зависимости

* **Namespace:** `Quests`
* **Используемые классы:**

  * [`QuestAsset`](../Core/QuestAsset.cs) — создаваемый ScriptableObject
  * `UnityEditor.AssetDatabase` — API для управления ассетами
  * `EditorUtility` и `Selection` — инструменты выделения и обновления UI Project Window
  * `System.Guid` — для генерации уникального ID

---

## Поведение

При выборе **Assets → Create → Quests → New Quest Asset**:

1. Создаётся новый экземпляр `QuestAsset` (`ScriptableObject.CreateInstance<QuestAsset>()`).
2. Генерируется уникальный идентификатор (`questID = Guid.NewGuid().ToString()`).
3. Присваивается имя по умолчанию `questName = "New Quest"`.
4. Если папка `Assets/Quests` не существует — она автоматически создаётся.
5. Создаётся новый `.asset` файл с уникальным именем (`NewQuest.asset`, `NewQuest 1.asset` и т. д.).
6. Ассет сохраняется и выделяется в **Project Window**.
7. В консоль выводится сообщение об успешном создании:

   ```
   QuestAsset создан: Assets/Quests/NewQuest.asset (ID: xxxxx)
   ```

---

## Пример использования

### Шаг 1: Открыть меню

В Unity открой:

```
Assets → Create → Quests → New Quest Asset
```

### Шаг 2: Новый ассет появится

Файл появится в папке:

```
Assets/Quests/NewQuest.asset
```

### Шаг 3: Настройка квеста

Выдели ассет и заполни поля в инспекторе:

* `Quest ID` — уже сгенерирован автоматически
* `Quest Name` — название квеста
* `Description` — описание
* `Behaviour` — тип поведения (например, `ApproachPointQuestBehaviour`)

---

## Взаимосвязь с другими компонентами

| Компонент        | Роль                         | Связь                                    |
| ---------------- | ---------------------------- | ---------------------------------------- |
| **QuestAsset**   | Хранит данные квеста         | Создаётся этим скриптом                  |
| **QuestManager** | Управляет активными квестами | Позже будет использовать созданный ассет |
| **QuestChain**   | Содержит список квестов      | Может включать созданный `QuestAsset`    |
| **QuestUI**      | Отображает квесты игроку     | Получает данные из `QuestAsset`          |

---

## Результат

После выполнения команды создаётся объект:

```
QuestAsset
 ├── questID: "f2b5d210-2db0-49ad-8fd7-93e45395f5e1"
 ├── questName: "New Quest"
 ├── description: ""
 ├── behaviour: null
 ├── currentProgress: 0
 ├── targetProgress: 0
```

---
