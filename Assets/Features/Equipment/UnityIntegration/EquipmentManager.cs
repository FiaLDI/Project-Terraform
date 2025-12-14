using Features.Equipment.Domain;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using UnityEngine;
using Features.Inventory;

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

            EquipRightHand(model.rightHand.item);

            bool isTwoHanded =
                model.rightHand.item?.itemDefinition?.isTwoHanded == true;

            if (isTwoHanded)
                ClearLeftHand();
            else
                EquipLeftHand(model.leftHand.item);

            usage?.OnHandsUpdated(leftHandUsable, rightHandUsable, isTwoHanded);
        }

        // ======================================================
        // RIGHT HAND
        // ======================================================

        private void EquipRightHand(ItemInstance inst)
        {
            ClearRightHand();
            if (inst == null)
                return;

            InstantiateRuntimeItem(
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

            InstantiateRuntimeItem(
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
        // INSTANTIATION
        // ======================================================

        private void InstantiateRuntimeItem(
            ItemInstance inst,
            Transform parent,
            out GameObject obj,
            out IUsable usable)
        {
            obj = null;
            usable = null;

            // Guards
            if (inst == null || inst.itemDefinition == null || parent == null)
                return;

            var prefab = inst.itemDefinition.worldPrefab;
            if (prefab == null)
            {
                Debug.LogError($"[EquipmentManager] worldPrefab NULL for {inst.itemDefinition.name}");
                return;
            }

            obj = Instantiate(prefab, parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            if (obj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            var holder = obj.GetComponent<ItemRuntimeHolder>()
                        ?? obj.AddComponent<ItemRuntimeHolder>();
            holder.SetInstance(inst);

            if (obj.TryGetComponent<IItemModeSwitch>(out var mode))
            {
                mode.SetEquippedMode();
            }

            usable = obj.GetComponent<IUsable>();
            if (usable == null)
            {
                Debug.LogWarning($"[EquipmentManager] No IUsable on {obj.name}");
                return;
            }

            usable.Initialize(playerCamera);
        }


        // ======================================================
        // PUBLIC API
        // ======================================================

        public IUsable GetRightHandUsable() => rightHandUsable;
        public IUsable GetLeftHandUsable()  => leftHandUsable;
    }
}
