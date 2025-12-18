using UnityEngine;
using TMPro;
using Features.Equipment.Domain;
using Features.Equipment.UnityIntegration;

public class AmmoCounterUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text ammoText;

    private IAmmoProvider ammoProvider;

    private void Awake()
    {
        root.SetActive(false);
    }

    private void OnEnable()
    {
        EquipmentEvents.OnHandsUpdated += HandleHandsUpdated;
    }

    private void OnDisable()
    {
        EquipmentEvents.OnHandsUpdated -= HandleHandsUpdated;
    }

    private void Update()
    {
        //Debug.Log($"AmmoUI: provider={(ammoProvider != null)}");

        if (ammoProvider == null)
            return;

        ammoText.text = $"{ammoProvider.CurrentAmmo} / {ammoProvider.MaxAmmo}";
    }

    private void HandleHandsUpdated(
        IUsable left,
        IUsable right,
        bool twoHanded)
    {
        Debug.Log($"[AmmoUI] HandsUpdated called. Right = {right?.GetType().Name}");

        if (right is UsableGun gun)
        {
            Debug.Log("[AmmoUI] Right hand IS UsableGun");

            if (gun.AmmoProvider != null)
            {
                Debug.Log("[AmmoUI] AmmoProvider OK");
                ammoProvider = gun.AmmoProvider;
                root.SetActive(true);
                return;
            }
            else
            {
                Debug.LogWarning("[AmmoUI] AmmoProvider IS NULL");
            }
        }

        ammoProvider = null;
        root.SetActive(false);
    }

}
