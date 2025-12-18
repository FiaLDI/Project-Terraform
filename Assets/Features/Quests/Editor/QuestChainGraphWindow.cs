using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Features.Quests.Data;

namespace Features.Quests.Editor
{
    public class QuestChainGraphWindow : EditorWindow
    {
        private QuestChainGraphView graphView;
        private ObjectField chainField;
        private QuestChainAsset currentChain;

        [MenuItem("Orbis/Quest Chain Editor")]
        public static void Open()
        {
            var window = GetWindow<QuestChainGraphWindow>();
            window.titleContent = new GUIContent("Quest Chain Editor");
            window.minSize = new Vector2(800, 500);
        }

        private void OnEnable()
        {
            ConstructGraphView();
            ConstructToolbar();
        }

        private void ConstructGraphView()
        {
            graphView = new QuestChainGraphView(this)
            {
                name = "Quest Chain GraphView"
            };

            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void ConstructToolbar()
        {
            var toolbar = new Toolbar();

            chainField = new ObjectField("Chain Asset:");
            chainField.objectType = typeof(QuestChainAsset);
            chainField.RegisterValueChangedCallback(evt =>
            {
                currentChain = evt.newValue as QuestChainAsset;
                graphView.Populate(currentChain);
            });

            var btnSave = new Button(() =>
            {
                if (currentChain != null)
                {
                    graphView.ApplyToChain(currentChain);
                    Debug.Log("✔ Chain saved");
                }
                else Debug.LogWarning("Select a chain first!");
            })
            { text = "Save" };

            toolbar.Add(chainField);
            toolbar.Add(btnSave);

            rootVisualElement.Add(toolbar);
        }
    }
}
