using Features.Effects.Domain;

namespace Features.Effects.Application
{
    public sealed class SpawnPrefabEffect : IEffect
    {
        private readonly string _prefabId;
        private readonly float _lifetime;
        private readonly bool _useSourcePosition;

        public SpawnPrefabEffect(
            string prefabId,
            float lifetime,
            bool useSourcePosition)
        {
            _prefabId = prefabId;
            _lifetime = lifetime;
            _useSourcePosition = useSourcePosition;
        }

        public void Apply(EffectContext context)
        {
            UnityEngine.Debug.Log("TRY SPAWN");

            if (SpawnService.Instance == null)
            {
                UnityEngine.Debug.LogError("SpawnService.Instance NULL");
                return;
            }
            
            SpawnService.Instance.Spawn(
                _prefabId,
                _lifetime,
                _useSourcePosition,
                context
            );
        }
    }
}
