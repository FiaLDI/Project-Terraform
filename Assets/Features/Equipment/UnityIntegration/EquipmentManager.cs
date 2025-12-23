using UnityEngine;
using FishNet.Object;
using Features.Camera.UnityIntegration;
using Features.Equipment.Domain;
using Features.Inventory;
using Features.Inventory.UnityIntegration;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Player.UnityIntegration;
using Features.Weapons.UnityIntegration;

namespace Features.Equipment.UnityIntegration
{
    /// <summary>
    /// Спавнит/удаляет предметы в руках на основе InventoryModel (left/right hand slots).
    /// Работает на всех клиентах (чтобы другие видели экип), но камеру/Initialize(IUsable) даёт только owner.
    /// Также напрямую обновляет локальный ввод (PlayerUsageController) и сетевой адаптер (PlayerUsageNetAdapter)
    /// на этом же player root.
    /// </summary>
    public sealed class EquipmentManager : MonoBehaviour
    {
        [Header("Hands")]
        [SerializeField] private Transform rightHandTransform;
        [SerializeField] private Transform leftHandTransform;

        [Header("Camera (optional override)")]
        [SerializeField] private UnityEngine.Camera playerCamera;

        private PlayerAnimationController anim;
        private PlayerUsageController usageLocal;
        private PlayerUsageNetAdapter usageNet;

        private GameObject currentRightHandObject;
        private GameObject currentLeftHandObject;

        private IUsable rightHandUsable;
        private IUsable leftHandUsable;

        private IInventoryContext inventory;
        private InventoryManager invManager;

        private bool initialized;

        // ======================================================
        // UNITY
        // ======================================================

        private void Awake()
        {
            anim = GetComponent<PlayerAnimationController>();
            usageLocal = GetComponent<PlayerUsageController>();
            usageNet = GetComponent<PlayerUsageNetAdapter>();
        }

        private void OnDestroy()
        {
            UnsubscribeInventory();
        }

        // ======================================================
        // INIT
        // ======================================================

        public void Init(IInventoryContext inventory)
        {
            if (inventory == null)
                return;

            // если уже инициализированы — перепривязка
            UnsubscribeInventory();

            this.inventory = inventory;
            invManager = inventory as InventoryManager;

            SubscribeInventory();

            initialized = true;
            EquipFromInventory();
        }

        private void SubscribeInventory()
        {
            if (inventory == null)
                return;

            // ✅ симметрично с UnsubscribeInventory()
            if (invManager != null)
                invManager.OnInventoryChanged += EquipFromInventory;
            else if (inventory.Service != null)
                inventory.Service.OnChanged += EquipFromInventory;
        }

        private void UnsubscribeInventory()
        {
            if (inventory == null)
                return;

            if (invManager != null)
                invManager.OnInventoryChanged -= EquipFromInventory;
            else if (inventory.Service != null)
                inventory.Service.OnChanged -= EquipFromInventory;
        }

        // ======================================================
        // INVENTORY → EQUIPMENT
        // ======================================================

        private void EquipFromInventory()
        {
            if (!initialized || inventory == null)
                return;

            var model = inventory.Model;
            if (model == null)
                return;

            // ---------- RIGHT HAND ----------
            EquipRightHand(model.rightHand.item);

            bool isTwoHanded =
                model.rightHand.item?.itemDefinition?.isTwoHanded == true;

            // ---------- LEFT HAND ----------
            if (isTwoHanded)
                ClearLeftHand();
            else
                EquipLeftHand(model.leftHand.item);

            // ---------- ANIMATION ----------
            UpdateWeaponPose(model.rightHand.item);

            // ---------- USAGE (LOCAL + NET) ----------
            usageLocal?.OnHandsUpdated(leftHandUsable, rightHandUsable, isTwoHanded);
            usageNet?.OnHandsUpdated(leftHandUsable, rightHandUsable, isTwoHanded);
        }

        // ======================================================
        // RIGHT HAND
        // ======================================================

        private void EquipRightHand(ItemInstance inst)
        {
            ClearRightHand();

            if (inst == null || inst.itemDefinition == null)
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

            if (inst == null || inst.itemDefinition == null)
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
        // ANIMATION
        // ======================================================

        private void UpdateWeaponPose(ItemInstance rightHandItem)
        {
            if (anim == null)
                return;

            if (rightHandItem == null || rightHandItem.itemDefinition == null)
            {
                anim.SetWeaponPose(0); // no weapon
                return;
            }

            anim.SetWeaponPose(
                rightHandItem.itemDefinition.isTwoHanded ? 2 : 1
            );
        }

        // ======================================================
        // INSTANTIATION
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
                return;

            obj = Instantiate(prefab, parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            var holder =
                obj.GetComponent<ItemRuntimeHolder>() ??
                obj.AddComponent<ItemRuntimeHolder>();
            holder.SetInstance(inst);

            usable = obj.GetComponent<IUsable>();

            // ✅ Камеру/Initialize даём только локальному владельцу
            var cam = GetLocalCameraOrNull();

            // Оружие имеет свою инициализацию (inventory + camera)
            var weapon = obj.GetComponent<WeaponController>();
            if (weapon != null)
            {
                weapon.Setup(inst);
                weapon.Init(inventory);
                if (cam != null)
                    weapon.Initialize(cam);
                return;
            }

            // Остальные usable
            if (usable != null && cam != null)
                usable.Initialize(cam);
        }

        private bool IsLocalOwner()
        {
            var nob = GetComponent<NetworkObject>();
            return nob != null && nob.IsOwner;
        }

        private UnityEngine.Camera GetLocalCameraOrNull()
        {
            if (!IsLocalOwner())
                return null;

            if (playerCamera != null)
                return playerCamera;

            if (CameraRegistry.Instance != null && CameraRegistry.Instance.CurrentCamera != null)
                return CameraRegistry.Instance.CurrentCamera;

            return UnityEngine.Camera.main;
        }

        // ======================================================
        // SOCKETS
        // ======================================================

        public void ApplySockets(CharacterSockets sockets)
        {
            if (sockets == null)
                return;

            rightHandTransform = sockets.rightHandSocket;
            leftHandTransform = sockets.leftHandSocket;

            EquipFromInventory();
        }

        // ======================================================
        // PUBLIC API
        // ======================================================

        public IUsable GetRightHandUsable() => rightHandUsable;
        public IUsable GetLeftHandUsable() => leftHandUsable;
    }
}
