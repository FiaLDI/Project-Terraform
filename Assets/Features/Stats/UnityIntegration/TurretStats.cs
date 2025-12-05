using UnityEngine;
using Features.Stats.Application;
using Features.Stats.Domain;
using Features.Stats.Adapter;


namespace Features.Stats.UnityIntegration
{
    public class TurretStats : MonoBehaviour
    {
        public TurretPresetSO preset;

        public IStatsFacade Facade { get; private set; }
        public StatsFacadeAdapter Adapter { get; private set; }

        // BUFF STORAGE
        public float DamageBonusAdd = 0f;
        public float DamageBonusMult = 1f;

        public float FireRateMultiplier = 1f;

        public float RotationBonusMult = 1f;

        public float MaxHpBonus = 0f;

        private void Awake()
        {
            Facade = new StatsFacade();
            Adapter = gameObject.AddComponent<StatsFacadeAdapter>();
            Adapter.Init(Facade);

            Facade.Combat.ApplyBase(preset.baseDamageMultiplier);

            Facade.Health.ApplyBase(preset.baseHp);
            Facade.Health.ApplyRegenBase(preset.baseRegen);

            Facade.Movement.ApplyBase(
                preset.rotationSpeed, 0, 0, 0
            );
        }

        public float FinalDamage =>
            (Facade.Combat.DamageMultiplier + DamageBonusAdd) * DamageBonusMult;

        public float FinalRotationSpeed =>
            Facade.Movement.BaseSpeed * RotationBonusMult;

        public float FinalFireRate => FireRateMultiplier;

        public void RecalculateHP()
        {
            Facade.Health.ApplyBase(preset.baseHp + MaxHpBonus);
        }

        
            public float Debug_MaxHP => Facade.Health.MaxHp;
            public float Debug_HP => Facade.Health.CurrentHp;

            public float Debug_DamageMultiplier => Facade.Combat.DamageMultiplier;
            public float Debug_RotationSpeed => Facade.Movement.BaseSpeed;
    }
}