using Features.Combat.Domain;
using System.Collections.Generic;

namespace Features.Combat.Application
{
    public class DoTEffectService
    {
        private readonly List<(IDamageable target, DoTEffectData data, float timer)> effects = new();

        public void ApplyDot(IDamageable target, DoTEffectData data)
        {
            effects.Add((target, data, data.duration));
        }

        public void Tick(float dt)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                var (target, data, timer) = effects[i];

                target.ApplyDamage(data.damagePerSecond * dt, data.type, default);

                timer -= dt;
                if (timer <= 0)
                    effects.RemoveAt(i);
                else
                    effects[i] = (target, data, timer);
            }
        }
    }
}
