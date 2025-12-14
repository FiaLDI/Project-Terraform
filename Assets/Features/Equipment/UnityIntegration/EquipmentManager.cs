using Features.Equipment.Domain;
using Features.Items.Domain;
using UnityEngine;
using Features.Inventory;
using Features.Items.UnityIntegration;
using Features.Weapons.UnityIntegration;

namespace Features.Equipment.UnityIntegration
{
    public class EquipmentManager : MonoBehaviour
    {
        [Header("Hands")]
        [SerializeField] private Transform rightHandTransform;
        [SerializeField] private Transform leftHandTransform;

        [Header("Camera")]
        [SerializeField] private UnityEngine.Camera playerCamera;

        private GameObject currentRightHandObject;
        private GameObject currentLeftHandObject;

        private IUsable rightHandUsable;
        private IUsable leftHandUsable;

        private PlayerUsageController usage;
        private IInventoryContext inventory;

        // ======================================================
        // INIT
        // ======================================================

        public void Init(IInventoryContext inventory)
        {
            this.inventory = inventory;
            inventory.Service.OnChanged += EquipFromInventory;
        }

        private void Awake()
        {
            if (playerCamera == null)
                playerCamera = UnityEngine.Camera.main;

            usage = GetComponent<PlayerUsageController>();
        }

        private void OnDestroy()
        {
            if (inventory != null)
                inventory.Service.OnChanged -= EquipFromInventory;
        }

        // ======================================================
        // INVENTORY → EQUIPMENT
        // ======================================================

        private void EquipFromInventory()
        {
            if (inventory == null)
                return;

            var model = inventory.Model;

            Debug.Log($"[EquipFromInventory] rightHand.item = {model.rightHand.item}");

            EquipRightHand(model.rightHand.item);

            bool isTwoHanded =
                model.rightHand.item?.itemDefinition?.isTwoHanded == true;

            if (isTwoHanded)
                ClearLeftHand();
            else
                EquipLeftHand(model.leftHand.item);

            usage?.OnHandsUpdated(leftHandUsable, rightHandUsable, isTwoHanded);

            EquipmentEvents.OnHandsUpdated?.Invoke(
                leftHandUsable,
                rightHandUsable,
                isTwoHanded
            );
        }

        // ======================================================
        // RIGHT HAND
        // ======================================================

        private void EquipRightHand(ItemInstance inst)
        {
            ClearRightHand();
            if (inst == null)
                return;

            InstantiateEquippedItem(
                inst,
                rightHandTransform,
                out currentRightHandObject,
                out rightHandUsable
            );
        }

        private void ClearRightHand()
        {
            if (currentRightHandObject != null)
                Destroy(currentRightHandObject);

            currentRightHandObject = null;
            rightHandUsable = null;
        }

        // ======================================================
        // LEFT HAND
        // ======================================================

        private void EquipLeftHand(ItemInstance inst)
        {
            ClearLeftHand();
            if (inst == null)
                return;

            InstantiateEquippedItem(
                inst,
                leftHandTransform,
                out currentLeftHandObject,
                out leftHandUsable
            );
        }

        private void ClearLeftHand()
        {
            if (currentLeftHandObject != null)
                Destroy(currentLeftHandObject);

            currentLeftHandObject = null;
            leftHandUsable = null;
        }

        // ======================================================
        // INSTANTIATION (EQUIPPED ONLY)
        // ======================================================

        private void InstantiateEquippedItem(
            ItemInstance inst,
            Transform parent,
            out GameObject obj,
            out IUsable usable)
        {
            obj = null;
            usable = null;

            if (inst == null || inst.itemDefinition == null || parent == null)
                return;

            var prefab = inst.itemDefinition.equippedPrefab;
            if (prefab == null)
            {
                Debug.LogError($"[EquipmentManager] equippedPrefab NULL for {inst.itemDefinition.name}");
                return;
            }

            obj = Instantiate(prefab, parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            var holder = obj.GetComponent<ItemRuntimeHolder>() ?? obj.AddComponent<ItemRuntimeHolder>();
            holder.SetInstance(inst);

            usable = obj.GetComponent<IUsable>();

            var weapon = obj.GetComponent<WeaponController>();
            if (weapon != null)
            {
                weapon.Setup(inst);
                weapon.Init(inventory);
                weapon.Initialize(playerCamera);
            }

            // 🔹 Usable (обёртка)
            usable.Initialize(playerCamera);

        }


        // ======================================================
        // PUBLIC API
        // ======================================================

        public IUsable GetRightHandUsable() => rightHandUsable;
        public IUsable GetLeftHandUsable()  => leftHandUsable;
    }
}
