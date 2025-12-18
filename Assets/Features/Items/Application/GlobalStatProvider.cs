namespace Features.Items.Application
{
    public class DefaultGlobalStatProvider : IGlobalStatProvider
    {
        public float Apply(ItemStatType stat, float baseValue)
        {
            return baseValue; // no modifiers
        }
    }
}
