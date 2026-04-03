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
        [Header("Hands")]
        [SerializeField] private Transform rightHandTransform;
        [SerializeField] private Transform leftHandTransform;

        private PlayerUsageNetAdapter usageNet;

        private GameObject currentRightHandObject;
        private GameObject currentLeftHandObject;
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

            if (IsOwner && initialized)
                EquipFromInventory();
        }

        private void OnDestroy()
        {
            UnsubscribeInventory();

            ClearRightHand();
            ClearLeftHand();
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

            EquipRightHand(model.rightHand.item);

            bool twoHanded =
                model.rightHand.item?.itemDefinition?.isTwoHanded == true;

            if (twoHanded)
                ClearLeftHand();
            else
                EquipLeftHand(model.leftHand.item);

            usageNet?.OnHandsUpdated(
                currentLeftHandObject,
                currentRightHandObject,
                twoHanded
            );

            //if (IsOwner && !IsServerInitialized)
            //    usageNet.SyncHands_Server();
        }

        // ======================================================
        // RIGHT HAND
        // ======================================================

        private void EquipRightHand(ItemInstance inst)
        {
            ClearRightHand();

            if (inst == null || inst.itemDefinition == null)
                return;

            var prefab = inst.itemDefinition.equippedPrefab;

            if (prefab != null)
            {
                currentRightHandObject =
                    Instantiate(prefab, rightHandTransform);

                currentRightHandObject.transform.localPosition = Vector3.zero;
                currentRightHandObject.transform.localRotation = Quaternion.identity;

                var holder =
                    currentRightHandObject.GetComponent<ItemRuntimeHolder>() ??
                    currentRightHandObject.AddComponent<ItemRuntimeHolder>();

                var owner = GetComponent<IBuffSource>();
                holder.SetInstance(inst, owner);

                ApplyItemBuffs(holder);
            }

            if (IsOwner)
                SpawnViewModel(inst);
        }

        private void ClearRightHand()
        {
            var holder = currentRightHandObject?.GetComponent<ItemRuntimeHolder>();

            if (holder != null)
                RemoveItemBuffs(holder);

            if (currentRightHandObject != null)
                Destroy(currentRightHandObject);

            if (currentViewWeapon != null)
                Destroy(currentViewWeapon);

            currentRightHandObject = null;
            currentViewWeapon = null;

            if (IsOwner)
                CameraRegistry.Instance?.SetFPSVisible(false);
        }

        // ======================================================
        // LEFT HAND
        // ======================================================

        private void EquipLeftHand(ItemInstance inst)
        {
            ClearLeftHand();

            if (inst == null || inst.itemDefinition == null)
                return;

            var prefab = inst.itemDefinition.equippedPrefab;

            if (prefab != null)
            {
                currentLeftHandObject =
                    Instantiate(prefab, leftHandTransform);

                currentLeftHandObject.transform.localPosition = Vector3.zero;
                currentLeftHandObject.transform.localRotation = Quaternion.identity;

                var holder =
                    currentLeftHandObject.GetComponent<ItemRuntimeHolder>() ??
                    currentLeftHandObject.AddComponent<ItemRuntimeHolder>();

                var owner = GetComponent<IBuffSource>();
                holder.SetInstance(inst, owner);

                ApplyItemBuffs(holder);
            }
        }

        private void ClearLeftHand()
        {
            var holder = currentLeftHandObject?.GetComponent<ItemRuntimeHolder>();

            if (holder != null)
                RemoveItemBuffs(holder);

            if (currentLeftHandObject != null)
                Destroy(currentLeftHandObject);

            currentLeftHandObject = null;
        }

        // ======================================================
        // VIEW MODEL
        // ======================================================

        private void SpawnViewModel(ItemInstance inst)
        {
            var camReg = CameraRegistry.Instance;

            if (camReg == null)
                return;

            if (inst.itemDefinition.viewModelPrefab == null)
            {
                camReg.SetFPSVisible(false);
                return;
            }

            var control = CameraServiceProvider.Control;

            camReg.InitializeFPS();

            // 🔥 показываем ТОЛЬКО если реально FPS
            bool isFPS = control != null && control.State.Blend < 0.5f;

            camReg.SetFPSVisible(isFPS);

            var socket = camReg.WeaponSocket;

            if (socket == null)
                return;

            currentViewWeapon =
                Instantiate(inst.itemDefinition.viewModelPrefab, socket);

            currentViewWeapon.transform.localPosition = Vector3.zero;
            currentViewWeapon.transform.localRotation = Quaternion.identity;

            var holder =
                currentViewWeapon.GetComponent<ItemRuntimeHolder>() ??
                currentViewWeapon.AddComponent<ItemRuntimeHolder>();

            var owner = GetComponent<IBuffSource>();
            holder.SetInstance(inst, owner);
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

            rightHandTransform = sockets.rightHandSocket;
            leftHandTransform = sockets.leftHandSocket;

            if (initialized)
                EquipFromInventory();
        }

        // ======================================================
        // PUBLIC API
        // ======================================================

        public GameObject GetRightHandObject() => currentRightHandObject;
        public GameObject GetLeftHandObject() => currentLeftHandObject;
    }
}