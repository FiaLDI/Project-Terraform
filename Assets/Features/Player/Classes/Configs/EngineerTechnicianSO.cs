using System.Collections.Generic;
using UnityEngine;
using Features.Abilities.Domain;
using Features.Passives.Domain;

[CreateAssetMenu(menuName = "Game/Class/EngineerTechnician")]
public class EngineerTechnicianSO : PlayerClass
{
    [Header("Энергия")]
    public float baseEnergy = 100f;
    public float regen = 8f;

    [Header("Активные способности (1–5)")]
    public List<AbilitySO> activeAbilities = new List<AbilitySO>(5);

    [Header("Пассивные бонусы")]
    public List<PassiveSO> passiveBonuses = new();
}
