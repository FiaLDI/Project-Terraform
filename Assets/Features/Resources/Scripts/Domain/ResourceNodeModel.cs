namespace Features.Resources.Domain
{
    public class ResourceNodeModel : IResourceNode
    {
        public float MaxHp { get; }
        public float CurrentHp { get; private set; }
        public bool IsDepleted => CurrentHp <= 0;

        public ResourceNodeModel(float maxHp)
        {
            MaxHp = maxHp <= 0 ? 1 : maxHp;
            CurrentHp = MaxHp;
        }

        public bool Mine(float amount) => ApplyDamage(amount);

        public bool ApplyDamage(float amount)
        {
            if (IsDepleted || amount <= 0f)
                return false;

            CurrentHp -= amount;
            if (CurrentHp < 0) CurrentHp = 0;

            return IsDepleted;
        }

        public float GetProgress()
        {
            return 1f - (CurrentHp / MaxHp);
        }
    }
}
