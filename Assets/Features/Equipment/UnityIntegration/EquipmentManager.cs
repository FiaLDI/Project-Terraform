using Features.Camera.UnityIntegration;
using Features.Equipment.Domain;
using Features.Game;
using Features.Inventory;
using Features.Inventory.UnityIntegration;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Player.UnityIntegration;
using Features.Weapons.UnityIntegration;
using FishNet.Object;
using GameKit.Dependencies.Utilities;
using UnityEngine;

namespace Features.Equipment.UnityIntegration
{
    public sealed class EquipmentManager : NetworkBehaviour
    {
        [Header("Hands (World Model)")]
        [SerializeField] private Transform rightHandTransform;
        [SerializeField] private Transform leftHandTransform;

        private PlayerAnimationController anim;
        private PlayerUsageNetAdapter usageNet;
        private EquipmentItemBuffApplier buffApplier;

        private GameObject currentRightHandObject;
        private GameObject currentLeftHandObject;
        private GameObject currentViewWeapon;

        private IUsable rightHandUsable;
        private IUsable leftHandUsable;

        private IInventoryContext inventory;
        private InventoryManager invManager;

        private bool initialized;

        private Animator fpsArmsAnimator;

        // ======================================================
        // UNITY
        // ======================================================

        private void Awake()
        {
            anim = GetComponent<PlayerAnimationController>();
            usageNet = GetComponent<PlayerUsageNetAdapter>();
            buffApplier = GetComponent<EquipmentItemBuffApplier>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // 🔥 ВАЖНО: повторная экипировка после owner-ready
            if (IsOwner && initialized)
                EquipFromInventory();
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

            bool isTwoHanded =
                model.rightHand.item?.itemDefinition?.isTwoHanded == true;

            if (isTwoHanded)
                ClearLeftHand();
            else
                EquipLeftHand(model.leftHand.item);

            UpdateWeaponPose(model.rightHand.item);

            usageNet?.OnHandsUpdated(leftHandUsable, rightHandUsable, isTwoHanded);
        }

        // ======================================================
        // RIGHT HAND
        // ======================================================

        private void EquipRightHand(ItemInstance inst)
        {
            buffApplier?.Remove();
            ClearRightHand();

            if (inst == null || inst.itemDefinition == null)
                return;
            
            if (IsOwner)
            {
                var camReg = CameraRegistry.Instance;

                if (inst.itemDefinition.viewModelPrefab == null)
                {
                    camReg?.SetFPSVisible(false);
                    return;
                }

                camReg?.InitializeFPS();
                camReg?.SetFPSVisible(true);
            }

            buffApplier?.Apply(inst);

            // WORLD MODEL
            var prefab = inst.itemDefinition.equippedPrefab;
            if (prefab != null)
            {
                currentRightHandObject = Instantiate(prefab, rightHandTransform);
                currentRightHandObject.transform.localPosition = Vector3.zero;
                currentRightHandObject.transform.localRotation = Quaternion.identity;

                var holder =
                    currentRightHandObject.GetComponent<ItemRuntimeHolder>() ??
                    currentRightHandObject.AddComponent<ItemRuntimeHolder>();

                holder.SetInstance(inst);

                rightHandUsable = currentRightHandObject.GetComponent<IUsable>();

                if (rightHandUsable is ScannerTool scanner)
                {
                    scanner.Setup(inst);
                }
            }

            // VIEW MODEL (ТОЛЬКО owner)
            if (IsOwner)
            {
                var camReg = CameraRegistry.Instance;

                if (camReg != null)
                {
                    camReg.InitializeFPS();

                    //fpsArmsAnimator = camReg.CurrentFPSAnimator;

                    var socket = camReg.WeaponSocket;

                    if (socket != null && inst.itemDefinition.viewModelPrefab != null)
                    {
                        currentViewWeapon =
                            Instantiate(inst.itemDefinition.viewModelPrefab, socket);

                        currentViewWeapon.transform.localPosition = Vector3.zero;
                        currentViewWeapon.transform.localRotation = Quaternion.identity;
                    }
                }
            }

            InitializeLogic(inst);
        }

        private void ClearRightHand()
        {
            buffApplier?.Remove();

            if (currentRightHandObject != null)
                Destroy(currentRightHandObject);

            if (currentViewWeapon != null)
                Destroy(currentViewWeapon);

            currentRightHandObject = null;
            currentViewWeapon = null;
            rightHandUsable = null;

            if (IsOwner)
            {
                CameraRegistry.Instance?.SetFPSVisible(false);
            }
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
                currentLeftHandObject = Instantiate(prefab, leftHandTransform);
                currentLeftHandObject.transform.localPosition = Vector3.zero;
                currentLeftHandObject.transform.localRotation = Quaternion.identity;

                leftHandUsable = currentLeftHandObject.GetComponent<IUsable>();
            }
        }

        private void ClearLeftHand()
        {
            if (currentLeftHandObject != null)
                Destroy(currentLeftHandObject);

            currentLeftHandObject = null;
            leftHandUsable = null;
        }

        // ======================================================
        // LOGIC
        // ======================================================

        private void InitializeLogic(ItemInstance inst)
        {
            if (!IsOwner)
                return;

            var cam = CameraRegistry.Instance?.CurrentCamera;

            if (cam == null)
            {
                Debug.Log("[EQUIP] Camera NULL for logic");
                return;
            }

            if (rightHandUsable != null)
            {
                Debug.Log("[EQUIP] Initializing usable with camera");
                rightHandUsable.Initialize(cam);
            }

            var weapon = currentRightHandObject != null
                ? currentRightHandObject.GetComponent<WeaponController>()
                : null;

            if (weapon != null)
            {
                weapon.Setup(inst);
                weapon.Init(inventory);
            }
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
                anim.SetWeaponPose(0);
                return;
            }

            anim.SetWeaponPose(
                rightHandItem.itemDefinition.isTwoHanded ? 2 : 1
            );
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

        public IUsable GetRightHandUsable() => rightHandUsable;
        public IUsable GetLeftHandUsable() => leftHandUsable;
    }
}