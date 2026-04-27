namespace Features.Buffs.Client
{
    public readonly struct ActiveBuffView
    {
        public readonly string buffId;
        public readonly int stacks;

        public ActiveBuffView(string buffId, int stacks)
        {
            this.buffId = buffId;
            this.stacks = stacks;
        }
    }
}
