namespace Features.Items.Application
{
    public interface IGlobalStatProvider
    {
        float Apply(ItemStatType stat, float baseValue);
    }
}
