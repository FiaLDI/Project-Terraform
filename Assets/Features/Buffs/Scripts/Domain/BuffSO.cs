using UnityEngine;
using System.Collections.Generic;

namespace Features.Buffs.Domain
{
    [CreateAssetMenu(menuName = "Game/Buff/Buff")]
    public sealed class BuffSO : ScriptableObject
    {
        // =========================
        // INFO (UI / META)
        // =========================

        [Header("Info")]
        public string buffId;
        public string displayName;

        [TextArea(2, 4)]
        public string description;

        public Sprite icon;
        public bool isDebuff;

        // =========================
        // EFFECTS (LOGIC)
        // =========================

        [Header("Effects")]
        [Tooltip("Набор эффектов, которые применяются этим бафом")]
        public List<BuffEffectSO> effects = new();

        // =========================
        // TIMING
        // =========================

        [Header("Timing")]
        [Min(0f)]
        public float duration = 5f;

        public bool isStackable;

        // =========================
        // DEBUG
        // =========================

        public override string ToString()
        {
            return $"{buffId} ({effects.Count} effects, {duration}s)";
        }
    }
}
