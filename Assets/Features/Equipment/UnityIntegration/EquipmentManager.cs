using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Camera.UnityIntegration;
using Features.Game;
using Features.Inventory;
using Features.Inventory.UnityIntegration;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Player.UnityIntegration;
using Features.Stats.UnityIntegration;
using FishNet.Object;
using UnityEngine;

namespace Features.Equipment.UnityIntegration
{
    public sealed class EquipmentManager : NetworkBehaviour, IBuffSource
    {
        [Header("Sockets")]
        [SerializeField] private Transform activeItemSocket;

        private PlayerUsageNetAdapter usageNet;
        private BuffSystem buffSystem;
        private MovementStatsSync movementStatsSync;

        private GameObject currentEquippedObject;
        private GameObject currentViewWeapon;
        private ItemInstance currentBuffedItem;

        private IInventoryContext inventory;
        private InventoryManager invManager;

        private bool initialized;

        // ======================================================
        // UNITY
        // ======================================================

        private void Awake()
        {
            usageNet = GetComponent<PlayerUsageNetAdapter>();
            buffSystem = GetComponent<BuffSystem>();
            movementStatsSync = GetComponent<MovementStatsSync>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (buffSystem != null)
            {
                buffSystem.OnServiceReady -= OnBuffSystemReady;
                buffSystem.OnServiceReady += OnBuffSystemReady;

                if (buffSystem.ServiceReady && initialized)
                    EquipFromInventory();
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (initialized)
                EquipFromInventory();
        }

        public override void OnStopServer()
        {
            if (buffSystem != null)
                buffSystem.OnServiceReady -= OnBuffSystemReady;

            base.OnStopServer();
        }

        private void OnDestroy()
        {
            UnsubscribeInventory();

            if (buffSystem != null)
                buffSystem.OnServiceReady -= OnBuffSystemReady;

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

        private void OnBuffSystemReady()
        {
            if (initialized)
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

            ApplyItemBuffs(inst);

            var prefab = inst.itemDefinition.equippedPrefab;
            if (prefab != null && activeItemSocket != null)
            {
                currentEquippedObject = Instantiate(prefab, activeItemSocket);

                currentEquippedObject.transform.localPosition = Vector3.zero;
                currentEquippedObject.transform.localRotation = Quaternion.identity;

                var holder =
                    currentEquippedObject.GetComponent<ItemRuntimeHolder>() ??
                    currentEquippedObject.AddComponent<ItemRuntimeHolder>();

                holder.SetInstance(inst, this);
            }

            if (IsOwner)
                SpawnViewModel(inst);
        }

        private void ClearEquipped()
        {
            if (currentBuffedItem != null && !currentBuffedItem.IsEmpty)
            {
                RemoveItemBuffs(currentBuffedItem);
                currentBuffedItem = null;
            }

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

            holder.SetInstance(inst, this);
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

        private void ApplyItemBuffs(ItemInstance inst)
        {
            if (!IsServerInitialized)
                return;

            if (inst == null || inst.itemDefinition == null)
                return;

            buffSystem ??= GetComponent<BuffSystem>();
            if (buffSystem == null || !buffSystem.ServiceReady)
                return;

            currentBuffedItem = inst;

            bool changed = false;

            var buffs = inst.itemDefinition.equippedBuffs;
            if (buffs != null)
            {
                foreach (var buff in buffs)
                {
                    if (buff == null)
                        continue;

                    if (buffSystem.Add(buff, this, BuffLifetimeMode.WhileSourceAlive) != null)
                        changed = true;
                }
            }

            if (inst.itemDefinition.upgrades != null &&
                inst.level >= 0 &&
                inst.level < inst.itemDefinition.upgrades.Length)
            {
                var upgrade = inst.itemDefinition.upgrades[inst.level];

                if (upgrade?.levelBuffs != null)
                {
                    foreach (var buff in upgrade.levelBuffs)
                    {
                        if (buff == null)
                            continue;

                        if (buffSystem.Add(buff, this, BuffLifetimeMode.WhileSourceAlive) != null)
                            changed = true;
                    }
                }
            }

            if (changed)
                movementStatsSync?.SendSnapshot();
        }

        private void RemoveItemBuffs(ItemInstance inst)
        {
            if (!IsServerInitialized)
                return;

            if (inst == null || inst.itemDefinition == null)
                return;

            buffSystem ??= GetComponent<BuffSystem>();
            if (buffSystem == null || !buffSystem.ServiceReady)
                return;

            bool changed = false;

            var buffs = inst.itemDefinition.equippedBuffs;
            if (buffs != null)
            {
                foreach (var buff in buffs)
                {
                    if (buff == null)
                        continue;

                    buffSystem.RemoveBySourceAndId(this, buff.buffId);
                    changed = true;
                }
            }

            if (inst.itemDefinition.upgrades != null &&
                inst.level >= 0 &&
                inst.level < inst.itemDefinition.upgrades.Length)
            {
                var upgrade = inst.itemDefinition.upgrades[inst.level];

                if (upgrade?.levelBuffs != null)
                {
                    foreach (var buff in upgrade.levelBuffs)
                    {
                        if (buff == null)
                            continue;

                        buffSystem.RemoveBySourceAndId(this, buff.buffId);
                        changed = true;
                    }
                }
            }

            if (changed)
                movementStatsSync?.SendSnapshot();
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
