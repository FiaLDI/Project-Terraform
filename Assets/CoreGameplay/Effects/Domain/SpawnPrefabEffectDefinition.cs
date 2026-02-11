namespace Features.Effects.Domain
{
    public enum SpawnOwnership
    {
        None,
        SameAsSource
    }

    public struct SpawnPrefabParams
    {
        public string PrefabId;
        public float Lifetime;
        public SpawnOwnership Ownership;
        public bool UseSourcePosition;
    }
}
