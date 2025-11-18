using UnityEngine;

[RequireComponent(typeof(PlayerEnergy))]
public class ClassManager : MonoBehaviour
{
    [SerializeField] private EngineerTechnicianSO technicianConfig;

    public EngineerTechnicianSO CurrentClass => technicianConfig;
    public AbilitySO[] ActiveAbilities { get; private set; } = new AbilitySO[5];

    private PlayerEnergy _energy;

    private void Awake()
    {
        _energy = GetComponent<PlayerEnergy>();

        if (technicianConfig != null)
        {
            _energy.SetMaxEnergy(technicianConfig.baseEnergy, true);
            _energy.SetRegen(technicianConfig.regen);

            for (int i = 0; i < ActiveAbilities.Length; i++)
            {
                ActiveAbilities[i] =
                    (i < technicianConfig.activeAbilities.Count)
                    ? technicianConfig.activeAbilities[i]
                    : null;
            }
        }
    }
}
