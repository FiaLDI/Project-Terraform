using Features.Effects.Domain;

namespace Features.Effects.Application
{
    public sealed class SpawnPrefabEffect : IEffect
    {
        private readonly string _prefabId;
        private readonly float _lifetime;
        private readonly bool _useSourcePosition;
        private readonly SpawnPositionMode _spawnPositionMode;
        private readonly float _forwardDistance;
        private readonly UnityEngine.LayerMask _surfaceMask;
        private readonly float _heightOffset;

        public SpawnPrefabEffect(
            string prefabId,
            float lifetime,
            bool useSourcePosition,
            SpawnPositionMode spawnPositionMode,
            float forwardDistance,
            UnityEngine.LayerMask surfaceMask,
            float heightOffset)
        {
            _prefabId = prefabId;
            _lifetime = lifetime;
            _useSourcePosition = useSourcePosition;
            _spawnPositionMode = spawnPositionMode;
            _forwardDistance = forwardDistance;
            _surfaceMask = surfaceMask;
            _heightOffset = heightOffset;
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
                _spawnPositionMode,
                _forwardDistance,
                _surfaceMask,
                _heightOffset,
                context
            );
        }
    }
}
