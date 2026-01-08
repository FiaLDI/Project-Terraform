using System;
using System.Collections.Generic;
using UnityEngine;
using Features.Abilities.Domain;

namespace Features.Abilities.Client
{
    public sealed class ClientAbilityView : MonoBehaviour
    {
        public IReadOnlyList<AbilitySO> Active => active;
        public event Action AbilitiesChanged;

        private readonly List<AbilitySO> active = new();

        public void Bind()
        {
            // ❌ НИЧЕГО НЕ ДЕЛАЕМ
            // данные придут ТОЛЬКО через SetAbilities
        }

        public void SetAbilities(AbilitySO[] abilities)
        {
            active.Clear();

            if (abilities != null)
                active.AddRange(abilities);

            Debug.Log($"[ClientAbilityView] SetAbilities count={active.Count}", this);

            AbilitiesChanged?.Invoke(); // 🔥 ЕДИНСТВЕННЫЙ ТРИГГЕР
        }
    }

}
