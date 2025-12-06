namespace Features.Quests.Domain.Behaviours
{
    public sealed class InteractPointQuestBehaviour : IQuestBehaviour
    {
        public string PointId { get; }

        public InteractPointQuestBehaviour(string pointId)
        {
            PointId = pointId;
        }

        public void OnStart(QuestRuntime quest)
        {
            quest.SetTarget(1);
            quest.SetProgress(0);
            quest.SetState(QuestState.Active);
        }

        public void OnEvent(QuestRuntime quest, IQuestEvent e)
        {
            if (quest.State != QuestState.Active)
                return;

            if (e is InteractionEvent ie && ie.PointId == PointId)
            {
                quest.SetProgress(1);
                quest.SetState(QuestState.Completed);
            }
        }

        public void OnReset(QuestRuntime quest)
        {
            quest.SetProgress(0);
            quest.SetTarget(1);
            quest.SetState(QuestState.Inactive);
        }
    }
}
