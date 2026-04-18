using Features.Items.Data;

namespace Features.Resources.Domain
{
    [System.Serializable]
    public struct DropEntry
    {
        public Item item;
        public int min;
        public int max;
        public float chance;
    }
}
