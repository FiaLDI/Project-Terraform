using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using Features.Quests.Data;

namespace Features.Quests.Editor
{
    public class QuestChainGraphView : GraphView
    {
        private QuestChainGraphWindow editor;
        private readonly List<QuestNodeView> nodeViews = new();

        public QuestChainGraphView(QuestChainGraphWindow editor)
        {
            this.editor = editor;

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentZoomer());

            SetupZoom(0.1f, 4f);
        }

        // Build graph
        public void Populate(QuestChainAsset chain)
        {
            DeleteElements(graphElements.ToList());
            nodeViews.Clear();

            if (chain == null || chain.quests == null || chain.quests.Count == 0)
                return;

            float x = 0;

            foreach (var quest in chain.quests)
            {
                if (quest == null) continue;

                var node = new QuestNodeView(quest);
                AddElement(node);

                node.SetPosition(new Rect(x, 200, 200, 100));
                nodeViews.Add(node);

                x += 260;
            }

            for (int i = 0; i < nodeViews.Count - 1; i++)
            {
                var edge = nodeViews[i].output.ConnectTo(nodeViews[i + 1].input);
                AddElement(edge);
            }
        }

        // Save graph into asset
        public void ApplyToChain(QuestChainAsset chain)
        {
            var edges = this.edges.ToList();

            var first = nodeViews.FirstOrDefault(n =>
                !edges.Any(e => e.input.node == n)
            );

            if (first == null)
            {
                Debug.LogError("Chain must have exactly 1 starting node!");
                return;
            }

            chain.quests.Clear();

            QuestNodeView current = first;

            while (current != null)
            {
                chain.quests.Add(current.quest);

                var nextEdge = edges.FirstOrDefault(e => e.output.node == current);
                if (nextEdge == null)
                    break;

                current = nextEdge.input.node as QuestNodeView;
            }

            UnityEditor.EditorUtility.SetDirty(chain);
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }
}
