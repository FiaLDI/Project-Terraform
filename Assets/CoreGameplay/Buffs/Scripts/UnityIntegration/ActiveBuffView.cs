namespace Features.Buffs.Client
{
    public readonly struct ActiveBuffView
    {
        public readonly string buffId;

        public ActiveBuffView(string buffId)
        {
            this.buffId = buffId;
        }
    }
}
