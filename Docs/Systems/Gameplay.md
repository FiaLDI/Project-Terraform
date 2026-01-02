# 📘 Gameplay Systems Architecture

**Classes · Abilities · Passives · Buffs · NetCode**

## 1. Цели архитектуры

**Основные требования:**

* ☑ Сервер — единственный источник истины (authoritative)
* ☑ Клиент — только ввод + визуал
* ☑ Domain-логика не зависит от сети
* ☑ UI не знает о сервере
* ☑ Возможность late-join
* ☑ Отсутствие double-spawn / double-apply
* ☑ Минимум SyncVar, максимум событий

---

## 2. Общая схема (высокоуровневая)

```
Client Input
   ↓
NetAdapter (ServerRpc)
   ↓
Server Domain Logic
   ↓
State Change
   ↓
ObserversRpc / SyncVar
   ↓
Client View
   ↓
UI
```

---

## 3. GamePhase (инициализация)

### GamePhase — это **pipeline инициализации**, а не gameplay-система.

```
None
 ↓
StatsReady
 ↓
ClassReady
 ↓
AbilitiesReady
 ↓
BuffsReady
```

### Правила:

* ❌ Клиент не управляет фазами
* ✅ Все подписки делаются **до** фаз
* ✅ Фазы гарантируют порядок инициализации
* ❌ Никакой логики внутри UI

---

## 4. Class System

### Роли

| Компонент               | Роль                                |
| ----------------------- | ----------------------------------- |
| `PlayerClassController` | Domain-логика класса                |
| `PlayerStateNetAdapter` | Серверная точка входа               |
| `PlayerClassConfigSO`   | Данные (abilities, passives, buffs) |

### Поток

```
Server:
ApplyClass(classId)
  → Apply stats preset
  → Apply passives
  → Send abilities to clients
  → Advance GamePhase
```

### Ключевые правила

* ❌ Класс нельзя применять с клиента
* ❌ Класс нельзя применять дважды
* ✅ Класс применяется **один раз за жизнь игрока**
* ✅ Все данные берутся из SO

---

## 5. Ability System

### Архитектура

| Слой        | Компонент                         |
| ----------- | --------------------------------- |
| Domain      | `AbilityCaster`, `AbilityService` |
| Network     | `AbilityCasterNetAdapter`         |
| Client View | `ClientAbilityView`               |
| UI          | `AbilityHUD`, `AbilitySlotUI`     |

---

### Поток кастования

```
[Client]
Input
 ↓
AbilityInputHandler
 ↓
AbilityCasterNetAdapter.Cast(index)
 ↓ (ServerRpc)
[Server]
AbilityCaster.TryCastWithContext()
 ↓
AbilityService.Execute()
 ↓
Cooldown / Energy / Effects
```

### Синхронизация способностей

```
Server:
PlayerStateNetAdapter
  → RpcApplyAbilities(abilityIds)

Client:
RpcApplyAbilities
  → ClientAbilityView.SetAbilities()
  → AbilityHUD.RebindAbilities()
```

### Критические правила

* ❌ UI НЕ читает AbilityCaster напрямую
* ❌ Клиент НЕ исполняет Ability
* ❌ ClientAbilityView НИКОГДА не генерирует данные
* ✅ ClientAbilityView — только snapshot с сервера
* ✅ AbilityCaster работает **только на сервере**

---

## 6. Passive System

### Назначение

Пассивы — это **модификаторы**, которые:

* подписываются на события
* не имеют таймера
* живут, пока жив источник

### Архитектура

| Компонент       | Роль              |
| --------------- | ----------------- |
| `PassiveSystem` | Runtime контейнер |
| `PassiveSO`     | Описание пассива  |
| `StatModifier`  | Влияние на статы  |

### Поток

```
Class Apply
 ↓
PassiveSystem.Add(passive)
 ↓
Passive subscribes to events
 ↓
Stat modifiers applied
```

### Правила

* ❌ Пассивы не синхронизируются напрямую
* ❌ Пассивы не имеют UI
* ✅ Их эффект виден через статы / бафы

---

## 7. Buff System

### Назначение

Buff = **временный или условный эффект**, часто визуализируемый.

---

### Архитектура

| Слой        | Компонент                            |
| ----------- | ------------------------------------ |
| Domain      | `BuffSystem`, `BuffInstance`         |
| Network     | `NetworkBuffSystem` (SyncList / RPC) |
| Client View | `ClientBuffView`                     |
| UI          | `BuffHUD`, `BuffIconUI`              |

---

### Поток применения бафа

```
Server:
BuffSystem.AddBuff()
  → create BuffInstance
  → apply stat modifiers
  → add buffId to SyncList
```

### Синхронизация

```
Client:
SyncList<string> buffIds
 ↓
ClientBuffView rebuild snapshot
 ↓
BuffHUD rebuild icons
```

### UI

* UI **никогда** не работает с BuffSystem напрямую
* UI работает через `ClientBuffView`
* Tooltip ищет BuffInstance **только по buffId**

---

## 8. Combat / Damage (пример: EnemyHealth)

### Принцип

**Только сервер считает урон.**

```
Client hit
 ↓
ServerRpc TakeDamage
 ↓
EnemyHealth.ApplyDamageServer()
 ↓
SyncVar<float> CurrentHealth
 ↓
Client EnemyHealth.SetHealthFromNetwork()
 ↓
UI update
```

### EnemyHealth — domain only:

* ❌ нет ServerRpc
* ❌ нет Destroy / Despawn
* ❌ нет SyncVar
* ✅ только цифры и события

---

## 9. NetCode правила (FishNet)

### Разрешено

* `ServerRpc` — только вход с клиента
* `ObserversRpc` — передача snapshot
* `SyncVar` — простое состояние (HP, IDs)
* `SyncList<string>` — идентификаторы

### Запрещено

* ❌ SyncVar на сложные объекты
* ❌ Логика в OnChange(asServer)
* ❌ UI, читающий SyncVar напрямую
* ❌ Domain-логика внутри NetworkBehaviour

---

## 10. Anti-patterns (что больше НЕ делаем)

* ❌ AbilityCaster на клиенте
* ❌ Бафы через UI
* ❌ Клиент решает, можно ли кастовать
* ❌ Destroy вместо Despawn
* ❌ Повторная инициализация при late-join
* ❌ Данные из UI → Gameplay

---

## 11. Инварианты системы (золотые правила)

1. **Server owns gameplay**
2. **Client owns input**
3. **UI owns visuals**
4. **SO owns data**
5. **Domain ≠ Network**
6. **Network ≠ UI**

---

## 12. Результат

✔ Предсказуемая и масштабируемая архитектура
✔ Отсутствие race conditions
✔ Корректный late-join
✔ Лёгкое расширение (новые классы / бафы / абилки)
✔ Чистое разделение ответственности
