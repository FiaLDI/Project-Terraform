<html>
<body>
<!--StartFragment--><html><head></head><body><h1>Документация разработчика: Система квестов</h1>
<h2>Содержание</h2>
<ol>
<li>
<p>Обзор</p>
</li>
<li>
<p>Архитектура и структура проекта</p>
</li>
<li>
<p>Основные сущности</p>
</li>
<li>
<p>Поток выполнения квеста</p>
</li>
<li>
<p>Пользовательский интерфейс (QuestUI)</p>
</li>
<li>
<p>Система цепочек квестов (QuestChain)</p>
</li>
<li>
<p>Добавление новых типов квестов</p>
</li>
<li>
<p>Тестирование и настройка на сцене</p>
</li>
<li>
<p>Расширение системы</p>
</li>
<li>
<p>Приложение: структура префабов</p>
</li>
</ol>
<hr>
<h2> Обзор</h2>
<p>Система квестов предназначена для модульного создания, управления и отображения квестов в Unity.<br>
Каждый квест представлен в виде <code inline="">ScriptableObject</code> (<code inline="">QuestAsset</code>), который содержит данные, цели и поведение.<br>
Поведение описывается через наследников <code inline="">QuestBehaviour</code>.<br>
Квесты управляются <code inline="">QuestManager</code>, а UI отображается через <code inline="">QuestUI</code>.</p>
<hr>
<h2>Архитектура и структура проекта</h2>
<h3>Основные директории</h3>
<pre><code>Assets/
 ├── Prefabs/
 │   ├── Quests/
 │   │   └── Quest system.prefab
 │   └── UI/
 │       └── QuestTemplate.prefab
 ├── Quests/
 │   ├── ApproachPoint/
 │   ├── Interaction/
 │   ├── KillEnemies/
 │   └── Shared/
 └── Scripts/
     └── Quests/
         ├── Core/
         ├── UI/
         └── ...
</code></pre>
<h3>Компоненты системы</h3>

Компонент | Тип | Назначение
-- | -- | --
QuestAsset | ScriptableObject | Хранит данные и прогресс квеста
QuestBehaviour | Base class | Определяет поведение (условия выполнения)
QuestManager | Singleton MonoBehaviour | Управляет активными и завершёнными квестами
QuestPoint | MonoBehaviour | Сценовая цель, связанная с QuestAsset
QuestUI | MonoBehaviour | Отображает HUD, журнал и уведомления
QuestInputHandler | MonoBehaviour | Обрабатывает ввод игрока (открытие панели квестов)
QuestChain / QuestChainManager | ScriptableObject + MonoBehaviour | Организуют последовательные цепочки квестов

<!--StartFragment--><h2 data-start="142" data-end="165"> Основные сущности</h2>
<p data-start="167" data-end="387">Квестовая система построена по модульному принципу и состоит из набора взаимосвязанных компонентов.<br data-start="266" data-end="269">
Каждый компонент отвечает за отдельный уровень логики — от данных и поведения до сцены и пользовательского интерфейса.</p>
<hr data-start="389" data-end="392">
<h3 data-start="394" data-end="415"><strong data-start="398" data-end="415">1. QuestAsset</strong></h3>
<p data-start="416" data-end="502"><strong data-start="416" data-end="424">Тип:</strong> <code data-start="425" data-end="443">ScriptableObject</code><br data-start="443" data-end="446">
<strong data-start="446" data-end="461">Назначение:</strong> хранит данные одного конкретного квеста.</p>
<p data-start="504" data-end="513"><strong data-start="504" data-end="513">Поля:</strong></p>
<ul data-start="514" data-end="861">
<li data-start="514" data-end="562">
<p data-start="516" data-end="562"><code data-start="516" data-end="525">questID</code> — уникальный идентификатор квеста.</p>
</li>
<li data-start="563" data-end="598">
<p data-start="565" data-end="598"><code data-start="565" data-end="576">questName</code> — отображаемое имя.</p>
</li>
<li data-start="599" data-end="635">
<p data-start="601" data-end="635"><code data-start="601" data-end="614">description</code> — описание для UI.</p>
</li>
<li data-start="636" data-end="742">
<p data-start="638" data-end="742"><code data-start="638" data-end="655">currentProgress</code> / <code data-start="658" data-end="674">targetProgress</code> — текущее и целевое количество целей (обновляются автоматически).</p>
</li>
<li data-start="743" data-end="861">
<p data-start="745" data-end="861"><code data-start="745" data-end="756">behaviour</code> — экземпляр (<code data-start="770" data-end="790">SerializeReference</code>) класса-наследника <code data-start="810" data-end="826">QuestBehaviour</code>, определяющий механику выполнения.</p>
</li>
</ul>
<p data-start="863" data-end="874"><strong data-start="863" data-end="874">Методы:</strong></p>
<ul data-start="875" data-end="1223">
<li data-start="875" data-end="919">
<p data-start="877" data-end="919"><code data-start="877" data-end="894">ResetProgress()</code> — сбрасывает прогресс.</p>
</li>
<li data-start="920" data-end="987">
<p data-start="922" data-end="987"><code data-start="922" data-end="940">RegisterTarget()</code> — добавляет новую цель (<code data-start="965" data-end="983">targetProgress++</code>).</p>
</li>
<li data-start="988" data-end="1063">
<p data-start="990" data-end="1063"><code data-start="990" data-end="1009">TargetCompleted()</code> — фиксирует выполненную цель (<code data-start="1040" data-end="1059">currentProgress++</code>).</p>
</li>
<li data-start="1064" data-end="1143">
<p data-start="1066" data-end="1143"><code data-start="1066" data-end="1079">IsCompleted</code> — проверяет завершение (<code data-start="1104" data-end="1139">currentProgress &gt;= targetProgress</code>).</p>
</li>
<li data-start="1144" data-end="1223">
<p data-start="1146" data-end="1223"><code data-start="1146" data-end="1168">NotifyQuestUpdated()</code> — уведомляет подписчиков об изменениях (например, UI).</p>
</li>
</ul>
<p data-start="1225" data-end="1363"><strong data-start="1225" data-end="1234">Роль:</strong><br data-start="1234" data-end="1237">
<code data-start="1237" data-end="1249">QuestAsset</code> — это <strong data-start="1256" data-end="1276">хранилище данных</strong> о квесте.<br data-start="1286" data-end="1289">
В нём нет логики выполнения, только состояние и уведомления об изменениях.</p>
<hr data-start="1365" data-end="1368">
<h3 data-start="1370" data-end="1395"><strong data-start="1374" data-end="1395">2. QuestBehaviour</strong></h3>
<p data-start="1396" data-end="1495"><strong data-start="1396" data-end="1404">Тип:</strong> <code data-start="1405" data-end="1421">abstract class</code><br data-start="1421" data-end="1424">
<strong data-start="1424" data-end="1439">Назначение:</strong> базовый класс, определяющий интерфейс поведения квеста.</p>
<p data-start="1497" data-end="1517"><strong data-start="1497" data-end="1517">Ключевые методы:</strong></p>
<ul data-start="1518" data-end="1836">
<li data-start="1518" data-end="1569">
<p data-start="1520" data-end="1569"><code data-start="1520" data-end="1550">StartQuest(QuestAsset quest)</code> — инициализация.</p>
</li>
<li data-start="1570" data-end="1648">
<p data-start="1572" data-end="1648"><code data-start="1572" data-end="1622">UpdateProgress(QuestAsset quest, int amount = 1)</code> — обновление прогресса.</p>
</li>
<li data-start="1649" data-end="1707">
<p data-start="1651" data-end="1707"><code data-start="1651" data-end="1684">CompleteQuest(QuestAsset quest)</code> — завершение квеста.</p>
</li>
<li data-start="1708" data-end="1761">
<p data-start="1710" data-end="1761"><code data-start="1710" data-end="1740">ResetQuest(QuestAsset quest)</code> — сброс состояния.</p>
</li>
<li data-start="1762" data-end="1836">
<p data-start="1764" data-end="1836"><code data-start="1764" data-end="1773">Clone()</code> — клонирует экземпляр для индивидуальных целей (<code data-start="1822" data-end="1834">QuestPoint</code>).</p>
</li>
</ul>
<p data-start="1838" data-end="1853"><strong data-start="1838" data-end="1853">Наследники:</strong></p>
<div class="_tableContainer_1rjym_1"><div tabindex="-1" class="group _tableWrapper_1rjym_13 flex w-fit flex-col-reverse">
Вот таблица с наследниками и их описанием:

| Класс | Описание |
|-------|-----------|
| `ApproachPointQuestBehaviour` | Игрок должен подойти к точке |
| `StandOnPointQuestBehaviour` | Игрок должен простоять на точке N секунд |
| `InteractQuestBehaviour` | Игрок должен взаимодействовать (нажать E) |
| `KillEnemiesQuestBehaviour` | Игрок должен убить N врагов с тегом "Enemy" |

**Роль:** Инкапсулирует конкретную логику выполнения квеста, вызываемую менеджером или сценой.

<hr data-start="2294" data-end="2297">
<h3 data-start="2299" data-end="2320"><strong data-start="2303" data-end="2320">3. QuestPoint</strong></h3>
<p data-start="2321" data-end="2408"><strong data-start="2321" data-end="2329">Тип:</strong> <code data-start="2330" data-end="2345">MonoBehaviour</code><br data-start="2345" data-end="2348">
<strong data-start="2348" data-end="2363">Назначение:</strong> сценавая цель квеста, размещаемая на уровне.</p>
<p data-start="2410" data-end="2426"><strong data-start="2410" data-end="2426">Особенности:</strong></p>
<ul data-start="2427" data-end="2729">
<li data-start="2427" data-end="2475">
<p data-start="2429" data-end="2475">Хранит ссылку <code data-start="2443" data-end="2456">linkedQuest</code> на <code data-start="2460" data-end="2472">QuestAsset</code>.</p>
</li>
<li data-start="2476" data-end="2583">
<p data-start="2478" data-end="2583">При старте регистрирует себя в квесте (<code data-start="2517" data-end="2535">RegisterTarget()</code>) и клонирует поведение (<code data-start="2560" data-end="2579">behaviour.Clone()</code>).</p>
</li>
<li data-start="2584" data-end="2662">
<p data-start="2586" data-end="2662">Отслеживает игрока (<code data-start="2606" data-end="2622">OnTriggerEnter</code>, <code data-start="2624" data-end="2634">Update()</code>) для проверки выполнения.</p>
</li>
<li data-start="2663" data-end="2729">
<p data-start="2665" data-end="2729">При выполнении цели уведомляет <code data-start="2696" data-end="2710">QuestManager</code> и уничтожает себя.</p>
</li>
</ul>
<p data-start="2731" data-end="2853"><strong data-start="2731" data-end="2740">Роль:</strong><br data-start="2740" data-end="2743">
<code data-start="2743" data-end="2755">QuestPoint</code> связывает <strong data-start="2766" data-end="2779">мир сцены</strong> с <strong data-start="2782" data-end="2800">логикой квеста</strong> и отвечает за проверку локальных условий выполнения.</p>
<hr data-start="2855" data-end="2858">
<h3 data-start="2860" data-end="2883"><strong data-start="2864" data-end="2883">4. QuestManager</strong></h3>
<p data-start="2884" data-end="2993"><strong data-start="2884" data-end="2892">Тип:</strong> <code data-start="2893" data-end="2920">Singleton (MonoBehaviour)</code><br data-start="2920" data-end="2923">
<strong data-start="2923" data-end="2938">Назначение:</strong> центральный контроллер активных и завершённых квестов.</p>
<p data-start="2995" data-end="3016"><strong data-start="2995" data-end="3016">Основные функции:</strong></p>
<ul data-start="3017" data-end="3325">
<li data-start="3017" data-end="3096">
<p data-start="3019" data-end="3096"><code data-start="3019" data-end="3039">StartSceneQuests()</code> — ищет все <code data-start="3051" data-end="3063">QuestPoint</code> и активирует связанные квесты.</p>
</li>
<li data-start="3097" data-end="3171">
<p data-start="3099" data-end="3171"><code data-start="3099" data-end="3123">StartQuest(QuestAsset)</code> — запускает квест и добавляет его в активные.</p>
</li>
<li data-start="3172" data-end="3262">
<p data-start="3174" data-end="3262"><code data-start="3174" data-end="3207">UpdateQuestProgress(QuestAsset)</code> — обновляет состояние и завершает при необходимости.</p>
</li>
<li data-start="3263" data-end="3325">
<p data-start="3265" data-end="3325"><code data-start="3265" data-end="3292">CompleteQuest(QuestAsset)</code> — переносит квест в завершённые.</p>
</li>
</ul>
<p data-start="3327" data-end="3473"><strong data-start="3327" data-end="3336">Роль:</strong><br data-start="3336" data-end="3339">
Главный <strong data-start="3347" data-end="3378">координатор системы квестов</strong>, обеспечивающий взаимодействие между <code data-start="3416" data-end="3428">QuestAsset</code>, <code data-start="3430" data-end="3446">QuestBehaviour</code>, <code data-start="3448" data-end="3460">QuestPoint</code> и <code data-start="3463" data-end="3472">QuestUI</code>.</p>
<hr data-start="3475" data-end="3478">
<h3 data-start="3480" data-end="3501"><strong data-start="3484" data-end="3501">5. QuestChain</strong></h3>
<p data-start="3502" data-end="3614"><strong data-start="3502" data-end="3510">Тип:</strong> <code data-start="3511" data-end="3529">ScriptableObject</code><br data-start="3529" data-end="3532">
<strong data-start="3532" data-end="3547">Назначение:</strong> задаёт последовательность квестов, которые выполняются по порядку.</p>
<p data-start="3616" data-end="3634"><strong data-start="3616" data-end="3634">Поля и методы:</strong></p>
<ul data-start="3635" data-end="3847">
<li data-start="3635" data-end="3672">
<p data-start="3637" data-end="3672"><code data-start="3637" data-end="3652">questsInOrder</code> — список квестов.</p>
</li>
<li data-start="3673" data-end="3717">
<p data-start="3675" data-end="3717"><code data-start="3675" data-end="3689">StartChain()</code> — запускает первый квест.</p>
</li>
<li data-start="3718" data-end="3794">
<p data-start="3720" data-end="3794"><code data-start="3720" data-end="3739">MoveToNextQuest()</code> — активирует следующий после завершения предыдущего.</p>
</li>
<li data-start="3795" data-end="3847">
<p data-start="3797" data-end="3847"><code data-start="3797" data-end="3811">ResetChain()</code> — сбрасывает прогресс всей цепочки.</p>
</li>
</ul>
<p data-start="3849" data-end="3932"><strong data-start="3849" data-end="3858">Роль:</strong><br data-start="3858" data-end="3861">
Обеспечивает <strong data-start="3874" data-end="3897">сценарную структуру</strong> — например, серию сюжетных миссий.</p>
<hr data-start="3934" data-end="3937">
<h3 data-start="3939" data-end="3967"><strong data-start="3943" data-end="3967">6. QuestChainManager</strong></h3>
<p data-start="3968" data-end="4072"><strong data-start="3968" data-end="3976">Тип:</strong> <code data-start="3977" data-end="3992">MonoBehaviour</code><br data-start="3992" data-end="3995">
<strong data-start="3995" data-end="4010">Назначение:</strong> управляет всеми цепочками квестов и автоматизирует их запуск.</p>
<p data-start="4074" data-end="4090"><strong data-start="4074" data-end="4090">Особенности:</strong></p>
<ul data-start="4091" data-end="4307">
<li data-start="4091" data-end="4167">
<p data-start="4093" data-end="4167">Хранит список <code data-start="4107" data-end="4120">questChains</code> и карту <code data-start="4129" data-end="4146">questToChainMap</code> (квест → цепочка).</p>
</li>
<li data-start="4168" data-end="4227">
<p data-start="4170" data-end="4227">Может автоматически запускать цепочки при старте сцены.</p>
</li>
<li data-start="4228" data-end="4307">
<p data-start="4230" data-end="4307">При завершении квеста вызывает <code data-start="4261" data-end="4280">MoveToNextQuest()</code> у соответствующей цепочки.</p>
</li>
</ul>
<p data-start="4309" data-end="4405"><strong data-start="4309" data-end="4318">Роль:</strong><br data-start="4318" data-end="4321">
Отвечает за <strong data-start="4333" data-end="4360">переходы между квестами</strong> и за контроль целостности квестовых цепочек.</p>
<hr data-start="4407" data-end="4410">
<h3 data-start="4412" data-end="4430"><strong data-start="4416" data-end="4430">7. QuestUI</strong></h3>
<p data-start="4431" data-end="4505"><strong data-start="4431" data-end="4439">Тип:</strong> <code data-start="4440" data-end="4455">MonoBehaviour</code><br data-start="4455" data-end="4458">
<strong data-start="4458" data-end="4473">Назначение:</strong> визуализация квестов на экране.</p>
<p data-start="4507" data-end="4522"><strong data-start="4507" data-end="4522">Компоненты:</strong></p>
<ul data-start="4523" data-end="4781">
<li data-start="4523" data-end="4611">
<p data-start="4525" data-end="4611"><strong data-start="4525" data-end="4539">HUD-панель</strong> (<code data-start="4541" data-end="4553">questPanel</code>) — краткий список активных квестов (до <code data-start="4593" data-end="4607">maxHudQuests</code>).</p>
</li>
<li data-start="4612" data-end="4690">
<p data-start="4614" data-end="4690"><strong data-start="4614" data-end="4632">Журнал квестов</strong> (<code data-start="4634" data-end="4650">allQuestsPanel</code>) — все активные и завершённые квесты.</p>
</li>
<li data-start="4691" data-end="4781">
<p data-start="4693" data-end="4781"><strong data-start="4693" data-end="4708">Уведомления</strong> (<code data-start="4710" data-end="4729">notificationPanel</code>) — всплывающие сообщения при добавлении/завершении.</p>
</li>
</ul>
<p data-start="4783" data-end="4803"><strong data-start="4783" data-end="4803">Основные методы:</strong></p>
<ul data-start="4804" data-end="5086">
<li data-start="4804" data-end="4848">
<p data-start="4806" data-end="4848"><code data-start="4806" data-end="4828">AddQuest(QuestAsset)</code> — добавляет в UI.</p>
</li>
<li data-start="4849" data-end="4909">
<p data-start="4851" data-end="4909"><code data-start="4851" data-end="4876">UpdateQuest(QuestAsset)</code> — обновляет прогресс и статус.</p>
</li>
<li data-start="4910" data-end="4967">
<p data-start="4912" data-end="4967"><code data-start="4912" data-end="4937">RemoveQuest(QuestAsset)</code> — удаляет после завершения.</p>
</li>
<li data-start="4968" data-end="5029">
<p data-start="4970" data-end="5029"><code data-start="4970" data-end="5001">ShowQuestNotification(string)</code> — показывает уведомление.</p>
</li>
<li data-start="5030" data-end="5086">
<p data-start="5032" data-end="5086"><code data-start="5032" data-end="5056">ToggleAllQuestsPanel()</code> — открывает/закрывает журнал.</p>
</li>
</ul>
<p data-start="5088" data-end="5188"><strong data-start="5088" data-end="5097">Роль:</strong><br data-start="5097" data-end="5100">
Обеспечивает <strong data-start="5113" data-end="5142">интерактивное отображение</strong> состояния квестов и взаимодействие с игроком.</p>
<hr data-start="5190" data-end="5193">
<h3 data-start="5195" data-end="5223"><strong data-start="5199" data-end="5223">8. QuestInputHandler</strong></h3>
<p data-start="5224" data-end="5314"><strong data-start="5224" data-end="5232">Тип:</strong> <code data-start="5233" data-end="5248">MonoBehaviour</code><br data-start="5248" data-end="5251">
<strong data-start="5251" data-end="5266">Назначение:</strong> обрабатывает ввод пользователя для работы с UI.</p>
<p data-start="5316" data-end="5331"><strong data-start="5316" data-end="5331">Функционал:</strong></p>
<ul data-start="5332" data-end="5466">
<li data-start="5332" data-end="5409">
<p data-start="5334" data-end="5409">Использует <code data-start="5345" data-end="5382">InputSystem_Actions.UI.ToggleQuests</code> (например, клавиша <code data-start="5402" data-end="5405">J</code>).</p>
</li>
<li data-start="5410" data-end="5466">
<p data-start="5412" data-end="5466">При нажатии вызывает <code data-start="5433" data-end="5465">QuestUI.ToggleAllQuestsPanel()</code>.</p>
</li>
</ul>
<p data-start="5468" data-end="5562"><strong data-start="5468" data-end="5477">Роль:</strong><br data-start="5477" data-end="5480">
Предоставляет удобный интерфейс управления квестовым журналом через систему ввода.</p>
<!--EndFragment-->

## Поток выполнения квеста

```
[QuestAsset]
   ↓
[QuestPoint] → регистрирует цель → сообщает в QuestManager
   ↓
[QuestManager] → активирует квест → вызывает UI
   ↓
Игрок выполняет условие (через QuestBehaviour)
   ↓
QuestBehaviour сообщает о выполнении → QuestAsset.TargetCompleted()
   ↓
QuestManager.UpdateQuestProgress() → QuestUI.UpdateQuest()
   ↓
Квест завершён → перенос в completedQuests
```

---

## Пользовательский интерфейс (QuestUI)

### Основные элементы:

| Элемент                             | Назначение                                          |
| ----------------------------------- | --------------------------------------------------- |
| `questPanel`                        | HUD панель активных квестов                         |
| `allQuestsPanel`                    | окно всех квестов (активных и завершённых)          |
| `notificationPanel`                 | всплывающие уведомления о новых/завершённых квестах |
| `questTemplate`, `allQuestTemplate` | префабы для отображения записей квестов             |

### Основные методы:

* `AddQuest(QuestAsset)` — добавить квест в UI
* `UpdateQuest(QuestAsset)` — обновить прогресс
* `RemoveQuest(QuestAsset)` — убрать квест
* `ToggleAllQuestsPanel()` — открыть/закрыть журнал
* `ShowQuestNotification(string)` — показать уведомление

> HUD ограничен параметром `maxHudQuests` (по умолчанию 5).

---

### QuestInputHandler

Отвечает за ввод:

* Подписан на Input System `UI/ToggleQuests`.
* При нажатии (например, клавиша **J**) вызывает `questUI.ToggleAllQuestsPanel()`.

---

## 🔗 Система цепочек квестов (QuestChain)

###  QuestChain

ScriptableObject, определяющий последовательность квестов.

**Поля:**

* `chainName`
* `List<QuestAsset> questsInOrder`

**Методы:**

* `StartChain()` — запускает первый квест.
* `MoveToNextQuest()` — переходит к следующему.
* `ResetChain()` — сбрасывает прогресс.

---

###  QuestChainManager

Компонент, управляющий всеми цепочками.

**Функции:**

* Автозапуск цепочек при старте сцены (`autoStartChainsOnStart`).
* Карта `questToChainMap` связывает каждый квест с его цепочкой.
* `OnQuestCompleted(QuestAsset)` вызывает переход к следующему квесту.

---

##  Добавление новых типов квестов

Пример: **CollectItemsBehaviour**

```csharp
[System.Serializable]
public class CollectItemsBehaviour : QuestBehaviour
{
    public int requiredCount = 5;
    private int collected;

    public override void StartQuest(QuestAsset quest) => collected = 0;

    public override void UpdateProgress(QuestAsset quest, int amount = 1)
    {
        collected += amount;
        if (collected >= requiredCount)
            CompleteQuest(quest);
    }

    public override void CompleteQuest(QuestAsset quest)
    {
        Debug.Log($"Квест {quest.questName} выполнен: {collected}/{requiredCount}");
    }

    public override bool IsCompleted => collected >= requiredCount;
    public override int CurrentProgress => collected;
    public override int TargetProgress => requiredCount;

    public override QuestBehaviour Clone() => (CollectItemsBehaviour)MemberwiseClone();
}
```

---

##  Тестирование и настройка на сцене

### Требуется на сцене:

* `QuestManager`
* `QuestUI`
* `QuestInputHandler`
* Игрок с `tag = "Player"`

### Для каждой квестовой точки:

1. Добавь объект с `QuestPoint`.
2. Привяжи `linkedQuest` (ScriptableObject).
3. Добавь `SphereCollider` (`isTrigger = true`).
4. Убедись, что `QuestManager` и `QuestUI` корректно связаны.

### Управление:

* Квесты автоматически активируются при старте сцены (`StartSceneQuests`).
* Панель квестов открывается клавишей (по умолчанию **J**).

---

##  Расширение системы

| Возможность                | Описание                                                            |
| -------------------------- | ------------------------------------------------------------------- |
| **Мультицели**             | массив `QuestBehaviour[]` внутри `QuestAsset`                       |
| **Цепочки**                | `QuestChain` и `QuestChainManager` для сюжетных последовательностей |
| **Награды**                | добавить поле `reward` в `QuestAsset`                               |
| **Сохранения**             | сериализация данных в JSON или `PlayerPrefs`                        |
| **UI улучшения**           | добавить фильтры (активные, завершённые, сюжетные)                  |
| **Интеграция с диалогами** | триггеры активации квестов через диалоговую систему                 |

---

##  Приложение: структура префабов

### Prefab: `Quest system`

```
Quest system
 ├── QuestManager
 └── QuestUIManager
      └── Canvas
          ├── Quest panel
          │   └── Quest
          │       └── Content
          │           └── QuestTemplate (Text, Image)
          ├── AllQuestsPanel
          │   ├── ActiveQuestsParent (Scroll View → ContentActive → QuestTemplate)
          │   ├── CompletedQuestsParent (Scroll View → ContentCompleted → QuestTemplate)
          └── NotificationPanel (NotificationText)
```

### Prefab: `QuestTemplate`

```
QuestTemplate
 ├── Text (TMP)
 └── Image (иконка или индикатор)
```

---