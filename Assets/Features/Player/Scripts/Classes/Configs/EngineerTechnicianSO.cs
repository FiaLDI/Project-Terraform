using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Class/EngineerTechnician")]
public class EngineerTechnicianSO : ScriptableObject
{
    [Header("Энергия")]
    public float baseEnergy = 100f;
    public float regen = 8f;

    [Header("Активные способности (слоты 1–5)")]
    public List<AbilitySO> activeAbilities = new List<AbilitySO>(5);

    [Header("Пассивные бонусы")]
    public List<ScriptableObject> passiveBonuses;
}
