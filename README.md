АРХИТЕКТУРНЫЙ МАНИФЕСТ ПРОЕКТА ORBIS GAME

(Версия 1.0)

Документ задаёт основные законы проекта, которые определяют структуру, зависимости и принципы разработки всех игровых систем.

Он служит:

фундаментом для рефакторинга
ориентиром для новых фич
правилом для написания чистого и поддерживаемого кода
шаблоном для будущих разработчиков

1. ФИЛОСОФИЯ

Игра строится как набор независимых модулей (features), каждый из которых изолирован, тестируем, заменяем и не зависит от деталей других подсистем.

Gameplay-функции оформляются как микросервисы внутри Unity.

Код должен быть:

предсказуемым
чистым
минимально связанным
расширяемым
безопасным для рефакторинга

2. ГЛАВНЫЙ ПРИНЦИП — ЧИСТАЯ ФИЧЕВАЯ АРХИТЕКТУРА

Каждая игровая система — это самостоятельная Feature:

Features/Inventory
Features/Items
Features/Weapons
Features/Crafting
Features/Combat
Features/Equipment
Features/Loot
Features/Abilities
Features/Passives
Features/Enemies

Фича имеет:

/Domain
/Application
/UnityIntegration
/UI
/Data (ScriptableObjects)

3. ЧЕТЫРЕ СЛОЯ ЛОГИКИ

Каждый модуль обязан содержать максимум 4 независимых слоя:

3.1 Domain (чистая логика + модели + интерфейсы)

Содержит:

модели данных (Item, WeaponStats, ArmorModel, HealthModel)
интерфейсы (IDamageable, IInventoryService, ICraftingService)
структуры (HitInfo, Recipe, InventorySlot)

❗ Запрещено:

MonoBehaviour
UnityEngine API
Instantiate
Transform

3.2 Application (чистые сервисы)

Содержит:
InventoryService
CombatService
CraftingService
WeaponService
DamageCalculator
UpgradeService

Сервисы:

не обращаются к UI
не работают напрямую с MonoBehaviour
не знают о Unity
могут быть протестированы в консольном C#

3.3 UnityIntegration (адаптеры)

Эти классы:

соединяют чистые сервисы с Unity
хранят MonoBehaviour
получают инпут
отключают/включают окна
вызывают сервисы, но НЕ содержат их логику
Это слой glue-кода.

3.4 UI (View слой)

Всё UI лежит отдельно:
Slot UI
Inventory UI
Equipment UI
Crafting UI
HealthBars
Damage Popups

UI никогда не содержит логики данных.

4. РЕШЁТКА ЗАВИСИМОСТЕЙ

Система зависимостей имеет фиксированную форму:

Items → Inventory → Crafting → Weapons → Combat → Enemies
              ↑
        Equipment
              ↑
          Loot System

5. ПРАВИЛО ИНТЕРФЕЙСОВ

Все модули взаимодействуют только через интерфейсы:

Инвентарь:
IInventoryService
IInventoryConsumer

Бой:
IDamageable
ICombatService

Оружие:
IWeaponBehaviour
IAmmoProvider

Крафт:
ICraftingService

Никаких прямых привязок:
Crafting → InventoryManager
Weapon → PlayerHealthComponent
Enemy → WeaponController
Combat → Inventory

6. ПРЕДМЕТЫ (Items) НЕ ИМЕЮТ ЛОГИКИ

Item (ScriptableObject) хранит только:

базовые параметры
тип предмета
статы
апгрейды
метаданные
world prefab

Но Item не наносит урон, не стреляет, не крафтит и не действует.

Все действия — через сервисы.

7. ИНВЕНТАРЬ — ЭТО ДАННЫЕ, А НЕ ЛОГИКА

InventoryModel содержит только:
массивы слотов
состояние хотбара
выбранный слот
InventoryService:
добавляет предметы
удаляет
ищет
сортирует
фильтрует
InventoryUIController — только UI.

8. КРАФТ — НЕ ЗНАЕТ О СЛОТАХ

CraftingService работает по контракту:

HasItemCount(item)
ConsumeItem(item)
AddItem(item)

Он не смотрит в слоты, массивы, хотбар, предметы.

9. ОРУЖИЕ — НЕ Item

Оружие — отдельная подсистема.
Item просто указывает на WeaponConfig.
Органы стрельбы, отдача, разброс — в WeaponService.
WeaponController — Unity адаптер, который:
слушает input
играет анимации
отправляет хит в CombatService

10. COMBAT — ЦАРЬ ЛОГИКИ УРОНА

CombatService решает:
сколько урона нанести
куда нанести
как считать резисты
броню
крит
headshot
bleeding
DoT

Combat НЕ знает:

об Inventory
об Items
об UI

11. HealthComponent — только адаптер

HealthComponent не считает урон.

Он только:

хранит HealthModel
принимает ApplyDamage
вызывает Die()

12. SCRIPTABLEOBJECT — ЭТО ДАННЫЕ, НЕ ЛОГИКА

Запрещено:

писать heavy-логику в SO
обращаться к MonoBehaviour внутри SO
вызывать Instantiate из SO

SO — конфигурация.

13. UI — МОГУТ СМЕНЯТЬСЯ, НЕ МЕНЯЯ КОД ЛОГИКИ

UI полностью заменяем без влияния на:

combat
inventory
crafting
items

14. СИСТЕМЫ ДОЛЖНЫ БЫТЬ ВОЗМОЖНО ТЕСТИРОВАТЬ

То есть:
почти весь код должен быть вне MonoBehaviour
большинство классов не должны иметь зависимости от UnityEngine
сервисы должны быть testable

15. ВСЕ ФИЧИ ДОЛЖНЫ БЫТЬ МОДУЛЬНЫМИ И ЗАМЕНЯЕМЫМИ

Можно заменить:
InventoryService → server authoritative inventory
CraftingService → MMO crafting
CombatService → RUST-style combat
WeaponSystem → hitscan/bullet physics
LootSystem → dynamic tables/weighted drop

Ничего не ломается.