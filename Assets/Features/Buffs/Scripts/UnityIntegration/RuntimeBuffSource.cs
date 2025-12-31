
// Assets/Features/Buffs/Scripts/Domain/RuntimeBuffSource.cs
namespace Features.Buffs.Domain
{
    /// <summary>
    /// Runtime-источник баффа (устройство, способность, эффект).
    /// </summary>
    public sealed class RuntimeBuffSource : IBuffSource
    {
        public object Owner { get; }

        public RuntimeBuffSource(object owner)
        {
            Owner = owner;
        }
    }
}
