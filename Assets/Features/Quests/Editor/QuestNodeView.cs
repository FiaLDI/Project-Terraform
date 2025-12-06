using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Features.Quests.Data;

namespace Features.Quests.Editor
{
    public class QuestNodeView : Node
    {
        public QuestAsset quest;
        public Port input;
        public Port output;

        public QuestNodeView(QuestAsset quest)
        {
            this.quest = quest;

            title = quest ? quest.questName : "NULL QUEST";

            input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            input.portName = "In";
            inputContainer.Add(input);

            output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            output.portName = "Out";
            outputContainer.Add(output);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
