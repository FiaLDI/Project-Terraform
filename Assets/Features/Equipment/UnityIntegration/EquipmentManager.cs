using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Camera.UnityIntegration;
using Features.Game;
using Features.Inventory;
using Features.Inventory.UnityIntegration;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Player.UnityIntegration;
using FishNet.Object;
using UnityEngine;

namespace Features.Equipment.UnityIntegration
{
    public sealed class EquipmentManager : NetworkBehaviour
    {
        [Header("Sockets")]
        [SerializeField] private Transform activeItemSocket;

        private PlayerUsageNetAdapter usageNet;

        private GameObject currentEquippedObject;
        private GameObject currentViewWeapon;

        private IInventoryContext inventory;
        private InventoryManager invManager;

        private bool initialized;

        // ======================================================
        // UNITY
        // ======================================================

        private void Awake()
        {
            usageNet = GetComponent<PlayerUsageNetAdapter>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (initialized)
                EquipFromInventory();
        }

        private void OnDestroy()
        {
            UnsubscribeInventory();
            ClearEquipped();
        }

        // ======================================================
        // INIT
        // ======================================================

        public void Init(IInventoryContext inventory)
        {
            if (inventory == null)
                return;

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
        // EQUIP
        // ======================================================

        public void EquipFromInventory()
        {
            if (!initialized || inventory == null)
                return;

            var model = inventory.Model;
            if (model == null)
                return;

            var activeSlot = model.GetActiveSlot(model.ActiveSlotIndex);
            var activeItem = activeSlot != null ? activeSlot.item : ItemInstance.Empty;

            int pose = 0;
            var def = activeItem?.itemDefinition;
            if (def != null)
                pose = def.GetWeaponPose();

            if (IsOwner)
            {
                var anim = GetComponent<PlayerAnimationController>();
                anim?.SetWeaponPose(pose);

                var cameraController = GetComponent<PlayerCameraController>();
                cameraController?.SetWeaponPose(pose);
            }

            EquipActiveItem(activeItem);

            if (IsOwner && (activeItem == null || activeItem.IsEmpty || activeItem.itemDefinition == null))
                CameraRegistry.Instance?.SetFPSVisible(false);

            Transform worldMuzzle = null;
            Transform viewMuzzle = null;

            if (currentEquippedObject != null)
            {
                var provider = currentEquippedObject.GetComponent<WeaponMuzzleProvider>();
                worldMuzzle = provider != null ? provider.Muzzle : null;
            }

            if (currentViewWeapon != null)
            {
                var provider = currentViewWeapon.GetComponent<WeaponMuzzleProvider>();
                viewMuzzle = provider != null ? provider.Muzzle : null;
            }

            usageNet?.OnEquippedItemUpdated(currentEquippedObject);
            usageNet?.SetMuzzles(worldMuzzle, viewMuzzle);

            var net = GetComponent<PlayerEquipmentNetwork>();

            net?.SetWeaponPose(pose);
        }

        // ======================================================
        // ACTIVE ITEM
        // ======================================================

        private void EquipActiveItem(ItemInstance inst)
        {
            ClearEquipped();

            if (inst == null || inst.itemDefinition == null)
                return;

            var prefab = inst.itemDefinition.equippedPrefab;
            if (prefab == null || activeItemSocket == null)
                return;

            currentEquippedObject = Instantiate(prefab, activeItemSocket);

            currentEquippedObject.transform.localPosition = Vector3.zero;
            currentEquippedObject.transform.localRotation = Quaternion.identity;

            var holder =
                currentEquippedObject.GetComponent<ItemRuntimeHolder>() ??
                currentEquippedObject.AddComponent<ItemRuntimeHolder>();

            var owner = GetComponent<IBuffSource>();
            holder.SetInstance(inst, owner);

            ApplyItemBuffs(holder);

            if (IsOwner)
                SpawnViewModel(inst);
        }

        private void ClearEquipped()
        {
            var holder = currentEquippedObject?.GetComponent<ItemRuntimeHolder>();

            if (holder != null)
                RemoveItemBuffs(holder);

            if (currentEquippedObject != null)
                Destroy(currentEquippedObject);

            if (currentViewWeapon != null)
                Destroy(currentViewWeapon);

            currentEquippedObject = null;
            currentViewWeapon = null;
        }

        // ======================================================
        // VIEW MODEL
        // ======================================================

        private void SpawnViewModel(ItemInstance inst)
        {
            var camReg = CameraRegistry.Instance;
            if (camReg == null)
                return;

            camReg.InitializeFPS();
            bool isFPS = ResolveIsFpsView();
            camReg.SetFPSVisible(isFPS);

            if (inst == null || inst.itemDefinition == null || inst.itemDefinition.viewModelPrefab == null)
                return;

            var socket = camReg.WeaponSocket;
            if (socket == null)
                return;

            currentViewWeapon = Instantiate(inst.itemDefinition.viewModelPrefab, socket);

            currentViewWeapon.transform.localPosition = Vector3.zero;
            currentViewWeapon.transform.localRotation = Quaternion.identity;

            var holder =
                currentViewWeapon.GetComponent<ItemRuntimeHolder>() ??
                currentViewWeapon.AddComponent<ItemRuntimeHolder>();

            var owner = GetComponent<IBuffSource>();
            holder.SetInstance(inst, owner);
        }

        private bool ResolveIsFpsView()
        {
            var cameraController = GetComponent<PlayerCameraController>();
            if (cameraController != null)
                return cameraController.IsFPS();

            var control = CameraServiceProvider.Control;
            return control != null && control.State.Blend < 0.5f;
        }

        public void RefreshViewModelVisibility()
        {
            if (!IsOwner)
                return;

            var camReg = CameraRegistry.Instance;
            if (camReg == null)
                return;

            bool shouldShow = currentViewWeapon != null && ResolveIsFpsView();
            camReg.SetFPSVisible(shouldShow);
        }

        // ======================================================
        // BUFFS
        // ======================================================

        private void ApplyItemBuffs(ItemRuntimeHolder holder)
        {
            if (!IsServerInitialized)
                return;

            var inst = holder.Instance;
            var source = holder.Source;

            if (inst == null || inst.itemDefinition == null)
                return;

            var buffs = inst.itemDefinition.equippedBuffs;
            if (buffs == null || buffs.Length == 0)
                return;

            var buffSystem = GetComponent<BuffSystem>();

            foreach (var buff in buffs)
            {
                buffSystem.Add(
                    buff,
                    source,
                    BuffLifetimeMode.WhileSourceAlive
                );
            }
        }

        private void RemoveItemBuffs(ItemRuntimeHolder holder)
        {
            if (!IsServerInitialized)
                return;

            var inst = holder.Instance;
            var source = holder.Source;

            if (inst == null || inst.itemDefinition == null)
                return;

            var buffs = inst.itemDefinition.equippedBuffs;
            if (buffs == null || buffs.Length == 0)
                return;

            var buffSystem = GetComponent<BuffSystem>();

            foreach (var buff in buffs)
                buffSystem.RemoveBySourceAndId(source, buff.buffId);
        }

        // ======================================================
        // SOCKETS
        // ======================================================

        public void ApplySockets(CharacterSockets sockets)
        {
            if (sockets == null)
                return;

            activeItemSocket = sockets.rightHandSocket != null
                ? sockets.rightHandSocket
                : sockets.leftHandSocket;

            if (initialized)
                EquipFromInventory();
        }

        // ======================================================
        // PUBLIC API
        // ======================================================

        public GameObject GetEquippedObject() => currentEquippedObject;
    }
}
