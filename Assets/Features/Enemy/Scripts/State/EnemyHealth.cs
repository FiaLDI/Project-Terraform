using System.Collections.Generic;
using UnityEngine;

namespace Features.Enemy.UnityIntegration
{
    public sealed class EnemyHealth : MonoBehaviour
    {
        private readonly List<EnemyHealthBarUI> views = new();

        public void RegisterHealthView(EnemyHealthBarUI view)
        {
            if (!views.Contains(view))
                views.Add(view);
        }

        public void UnregisterHealthView(EnemyHealthBarUI view)
        {
            views.Remove(view);
        }

        // CLIENT ONLY
        public void SetHealthFromNetwork(float hp, float maxHp)
        {
            for (int i = 0; i < views.Count; i++)
                views[i].SetHealth(hp, maxHp);
        }
    }
}
