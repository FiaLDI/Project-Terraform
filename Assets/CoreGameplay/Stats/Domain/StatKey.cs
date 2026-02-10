namespace Features.Stats.Domain
{
    [System.Serializable]
    public readonly struct StatKey
    {
        public readonly string Id;

        public StatKey(string id)
        {
            Id = id;
        }

        public override string ToString() => Id;

        public override int GetHashCode() => Id.GetHashCode();
        public override bool Equals(object obj) =>
            obj is StatKey other && other.Id == Id;

        public static bool operator ==(StatKey a, StatKey b) => a.Id == b.Id;
        public static bool operator !=(StatKey a, StatKey b) => a.Id != b.Id;
    }
}
