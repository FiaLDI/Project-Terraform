namespace Features.Quests.Domain.Behaviours
{
    public sealed class StandOnPointQuestBehaviour : IQuestBehaviour
    {
        public string PointId { get; }
        public float RequiredTime { get; }

        private float _timer;
        private bool _playerOnPoint;

        public StandOnPointQuestBehaviour(string pointId, float requiredTime)
        {
            PointId = pointId;
            RequiredTime = requiredTime;
        }

        public void OnStart(QuestRuntime quest)
        {
            _timer = 0f;
            _playerOnPoint = false;
            quest.SetTarget(1);
            quest.SetProgress(0);
            quest.SetState(QuestState.Active);
        }

        public void OnEvent(QuestRuntime quest, IQuestEvent e)
        {
            if (quest.State != QuestState.Active)
                return;

            switch (e)
            {
                case PointReachedEvent pre when pre.PointId == PointId:
                    _playerOnPoint = true;
                    break;

                case TickEvent tick when _playerOnPoint:
                    _timer += tick.DeltaTime;
                    if (_timer >= RequiredTime)
                    {
                        quest.SetProgress(1);
                        quest.SetState(QuestState.Completed);
                    }
                    break;

                case PointLeftEvent ple when ple.PointId == PointId:
                    _playerOnPoint = false;
                    _timer = 0f;
                    break;
            }
        }

        public void OnReset(QuestRuntime quest)
        {
            _timer = 0f;
            _playerOnPoint = false;
            quest.SetProgress(0);
            quest.SetTarget(1);
            quest.SetState(QuestState.Inactive);
        }
    }

    // Дополнительное событие ухода с точки
    public sealed class PointLeftEvent : IQuestEvent
    {
        public string PointId { get; }
        public PointLeftEvent(string pointId) => PointId = pointId;
    }
}
