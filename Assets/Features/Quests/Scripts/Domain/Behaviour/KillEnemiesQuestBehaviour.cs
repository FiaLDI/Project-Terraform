namespace Features.Quests.Domain.Behaviours
{
    public sealed class KillEnemiesQuestBehaviour : IQuestBehaviour
    {
        public string EnemyTag { get; }
        public int RequiredKills { get; }

        private int _currentKills;

        public KillEnemiesQuestBehaviour(string enemyTag, int requiredKills)
        {
            EnemyTag = enemyTag;
            RequiredKills = requiredKills;
        }

        public void OnStart(QuestRuntime quest)
        {
            _currentKills = 0;
            quest.SetTarget(RequiredKills);
            quest.SetProgress(0);
            quest.SetState(QuestState.Active);
        }

        public void OnEvent(QuestRuntime quest, IQuestEvent e)
        {
            if (quest.State != QuestState.Active)
                return;

            if (e is EnemyKilledEvent ek && ek.EnemyTag == EnemyTag)
            {
                _currentKills++;
                quest.SetProgress(_currentKills);

                if (_currentKills >= RequiredKills)
                {
                    quest.SetState(QuestState.Completed);
                }
            }
        }

        public void OnReset(QuestRuntime quest)
        {
            _currentKills = 0;
            quest.SetProgress(0);
            quest.SetState(QuestState.Inactive);
        }
    }
}
