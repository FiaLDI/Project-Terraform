README — Enemy System (LOD + Instancing + Config + Canvas + Health)
✅ 1. EnemyConfigSO — главный конфиг врага

Каждый тип врага имеет один ScriptableObject:

Enemies/
 └─ ZombieConfig.asset


EnemyConfigSO содержит:

Обязательные поля
Поле	Описание
enemyId	Уникальный ID врага (используется в квестах, телеметрии)
displayName	Имя врага
prefab	БАЗОВЫЙ префаб врага (тот, что в сцену или пул)
worldCanvasPrefab	Префаб HP-бара (Canvas + EnemyHealthBarUI)
baseMaxHealth	Базовое здоровье врага
LOD

В конфиге больше НЕ хранятся Renderer-ы — они берутся автоматически из структуры префаба.

Поле используется только для дистанций:

Поле	Описание
lod0Distance	До какой дистанции включён Model_LOD0
lod1Distance	До какой дистанции включён Model_LOD1
lod2Distance	После этого включён Model_LOD2
Canvas
Поле	Описание
canvasHideDistance	На какой дистанции прятать HP-bar
Instancing
Поле	Описание
useGPUInstancing	Разрешено ли GPU-instancing
instancingDistance	На какой дистанции включать instancing
disableAnimatorInInstancing	Отключать Animator в instancing
makeRigidbodyKinematicInInstancing	Делать Rigidbody kinematic
✅ 2. Структура префаба врага

ПРЕФАБ ДОЛЖЕН БЫТЬ СТРОГО ТАКИМ:

Enemy_Zombie (root)
 ├─ Model
 │    ├─ Model_LOD0
 │    ├─ Model_LOD1
 │    └─ Model_LOD2
 ├─ Anchor               ← необязательно (точка для HP-бара)
 └─ (автоматически добавляемые компоненты:)
      EnemyLODController
      EnemyHealth
      EnemyInstanceTracker

Требования

✔ Должен существовать объект Model
✔ Внутри должны быть дочерние объекты:

Model_LOD0
Model_LOD1
Model_LOD2


LOD1 и LOD2 — опциональны, но LOD0 должен быть обязательно.

✔ Anchor — произвольная точка для HP-бара (если отсутствует, бар будет над головой авто-расчётом)

✅ 3. Компоненты на префабе
Обязательные (ставятся автоматически редактором или вручную)
Компонент	За что отвечает
EnemyHealth	HP врага, урон, смерть, события
EnemyLODController	Автоматическое переключение LOD + Instancing
EnemyInstanceTracker	Регистрация врага в EnemyWorldManager + BiomeCounter

Дополнительные (опциональные):

Animator (если есть анимации)

Rigidbody (для физики)

Collider (обычно CapsuleCollider)

✅ 4. Что делает EnemyLODController
Автоматически ищет рендеры:
Model/Model_LOD0 → lod0
Model/Model_LOD1 → lod1
Model/Model_LOD2 → lod2

LOD-включение:

LOD0 — ближний

LOD1 — средний

LOD2 — дальний

Очень далеко — GPU instancing

Canvas:

Если config.worldCanvasPrefab не null:

Canvas создаётся автоматически

EnemyHealthBarUI получает:

Target = EnemyHealth

HeadAnchor = Anchor (если есть)