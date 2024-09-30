using System;
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

        public EquipmentSlot Equip(Type SlotT, int index) {
            if (_slot != null) {
                throw new InvalidOperationException();
            }
            _slot = Create(SlotT);
            _slot.inventoryIndexReference = index;
            _slot.OnOccupied();
            return _slot;
        }

        public void Unequip() {
            if (_slot != null) {
                _slot.Free();
                _slot = null;
            }
        }

        private EquipmentSlot Create(Type SlotT) {
            if (!SlotT.IsSubclassOf(typeof(EquipmentSlot))) {
                throw new System.Diagnostics.UnreachableException("Type SlotT is not a subtype of EquipmentSlot");
            }

            var slot = Manager.memory.GetFreeComponent(
                SlotT,
                deferOnOccupied: true
            ) as EquipmentSlot;

            if (slot == null) {
                throw new System.Diagnostics.UnreachableException("Memory pool for equipment slot is exhausted");
            }

            return slot;
        }
    }

    sealed class SlotBarIntegrationManager : MonoBehaviour {
        internal ISlotBarIntegrationStrategy Integration { get; private set; } = new DefaultSlotBarIntegration();
    }

    interface ISlotBarIntegrationStrategy {
        void ApplyIndexAfterUpdate(PlayerController playerController);
        void RevertIndexBeforeUpdate(PlayerController playerController);
        void UpdateIndex(PlayerController playerController);
    }

    class DefaultSlotBarIntegration : ISlotBarIntegrationStrategy {
        private int _previousSlotbarIndex = -1;
        public void UpdateIndex(PlayerController playerController) {
            if (playerController.equippedSlotIndex != _previousSlotbarIndex
                && playerController.equippedSlotIndex < PlayerController.MAX_EQUIPMENT_SLOTS
            ) {
                _previousSlotbarIndex = playerController.equippedSlotIndex;
            }
        }

        public void RevertIndexBeforeUpdate(PlayerController playerController) {
            if (_previousSlotbarIndex == -1) {
                return;
            }
            
            PlayerControllerAccessor.SetEquippedSlotIndex(playerController, _previousSlotbarIndex);
        }

        public void ApplyIndexAfterUpdate(PlayerController playerController) {
            if (playerController.TryGetComponent<AnySlotController>(out var slotController) && slotController.IsEquipped()) {
                PlayerControllerAccessor.SetEquippedSlotIndex(playerController, slotController.GetSlot().inventoryIndexReference);
            }
        }
    }
}