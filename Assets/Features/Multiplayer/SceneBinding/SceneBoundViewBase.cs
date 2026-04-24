using System;
using UnityEngine;

namespace Features.Multiplayer.SceneBinding
{
    public abstract class SceneBoundViewBase : MonoBehaviour, ISceneBoundView
    {
        [Header("Scene Bound Identity")]
        [SerializeField] private string boundType;
        [SerializeField] private string boundId;

        public string BoundType => boundType;
        public string BoundId => boundId;
        public string BoundKey => SceneBoundKey.Make(boundType, boundId);
        public GameObject GameObject => gameObject;

        protected virtual string DefaultBoundType => "world";

        protected virtual void OnEnable()
        {
            SceneBoundRegistry.RegisterView(this);
        }

        protected virtual void OnDisable()
        {
            SceneBoundRegistry.UnregisterView(this);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(boundType))
                boundType = DefaultBoundType;

            if (string.IsNullOrWhiteSpace(boundId))
                boundId = Guid.NewGuid().ToString("N");
        }
#endif
    }
}
