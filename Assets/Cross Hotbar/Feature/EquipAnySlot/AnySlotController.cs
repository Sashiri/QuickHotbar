using CrossHotbar.InventoryObjectSlot;
using CrossHotbar.InventoryObjectSlotBar;
using System;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable

namespace CrossHotbar.EquipAnySlot {
    internal class AnySlotController : MonoBehaviour {
        protected EquipmentSlot? _slot = null;
        public bool IsEquipped() => _slot != null;
        public bool IsTracking(int index) => _slot != null && _slot.inventoryIndexReference == index;

        public EquipmentSlot GetSlot() {
            if (_slot == null) {
                throw new InvalidOperationException("Can be called only after Equip has been called, use other methods to check validity");
            }
            return _slot;
        }

        internal EquipmentSlot Equip(Type SlotT, int index) {
            if (_slot != null) {
                throw new InvalidOperationException();
            }
            _slot = Create(SlotT, true);
            _slot.inventoryIndexReference = index;
            _slot.OnOccupied();
            return _slot;
        }

        internal void Unequip() {
            if (_slot != null) {
                _slot.Free();
                _slot = null;
            }
        }

        private EquipmentSlot Create(Type SlotT, bool deferOnOccupied = false) {
            if (!SlotT.IsSubclassOf(typeof(EquipmentSlot))) {
                throw new System.Diagnostics.UnreachableException("Type SlotT is not a subtype of EquipmentSlot");
            }

            var slot = Manager.memory.GetFreeComponent(
                SlotT,
                deferOnOccupied: deferOnOccupied
            ) as EquipmentSlot;

            if (slot == null) {
                throw new System.Diagnostics.UnreachableException("Memory pool for equipment slot is exhausted");
            }

            return slot;
        }
    }

    sealed class SlotBarIntegrationManager : MonoBehaviour {
        internal ISlotBarIntegrationStrategy? Integration { get; set; }
    }

    interface ISlotBarIntegrationStrategy {
        void Enable(PlayerController playerController);
        void Disable(PlayerController playerController);
        bool IsEnabled(PlayerController playerController);
        ValueTask OnInventoryUpdate(PlayerController playerController, Task inventoryUpdated);
    }

    class DefaultSlotBarIntegration : MonoBehaviour, ISlotBarIntegrationStrategy {
        private const int DISABLED = -1;
        protected EquipedSlotUI? equipedSlotUI;
        private int _previousSlotbarIndex = DISABLED;

        private void Start() {
            equipedSlotUI = EquipedSlotUI.Create().GetComponent<EquipedSlotUI>();
            equipedSlotUI.gameObject.SetActive(false);
        }
        private void OnDestroy() {
            DestroyImmediate(equipedSlotUI);
        }

        void ISlotBarIntegrationStrategy.Enable(PlayerController playerController) {
            if (playerController.equippedSlotIndex >= PlayerController.MAX_EQUIPMENT_SLOTS) {
                throw new InvalidOperationException($"Expected to track slots of the basic hotbar, got slot index of {playerController.equippedSlotIndex} instead");
            }
            _previousSlotbarIndex = playerController.equippedSlotIndex;
            if (equipedSlotUI != null) {
                equipedSlotUI.gameObject.transform.position = Manager.ui.itemSlotsBar.GetEquipmentSlot(_previousSlotbarIndex).gameObject.transform.position;
                equipedSlotUI.gameObject.SetActive(true);
            }
        }

        void ISlotBarIntegrationStrategy.Disable(PlayerController playerController) {
            if (equipedSlotUI != null) {
                equipedSlotUI.gameObject.SetActive(false);
            }
            _previousSlotbarIndex = DISABLED;
        }

        bool ISlotBarIntegrationStrategy.IsEnabled(PlayerController playerController) => _previousSlotbarIndex != DISABLED;

        async ValueTask ISlotBarIntegrationStrategy.OnInventoryUpdate(PlayerController playerController, Task inventoryTask) {
            if (_previousSlotbarIndex == DISABLED) {
                return;
            }
            PlayerControllerAccessor.SetEquippedSlotIndex(playerController, _previousSlotbarIndex);

            await inventoryTask;

            if (playerController.TryGetComponent<AnySlotController>(out var slotController) && slotController.IsEquipped()) {
                PlayerControllerAccessor.SetEquippedSlotIndex(playerController, slotController.GetSlot().inventoryIndexReference);
                if (equipedSlotUI != null && equipedSlotUI.GetEquipmentSlot(0) is InventoryObjectSlotUI inventoryObjectSlot) {
                    inventoryObjectSlot.ObjectID = playerController.GetEquippedSlot().objectData.objectID;
                }
            }
        }
    }
}