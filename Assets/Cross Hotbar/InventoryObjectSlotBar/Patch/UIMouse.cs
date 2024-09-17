using CrossHotbar.InventoryObjectSlot;
using HarmonyLib;
using UnityEngine;

#nullable enable

namespace CrossHotbar.InventoryObjectSlotBar.Patch {
    [HarmonyPatch(typeof(global::UIMouse))]
    class UIMouse {
        private static InventoryObjectSlotBarUI? _slotBarInstance;
        public static void SetSlotBarUIInstance(InventoryObjectSlotBarUI? instance) {
            _slotBarInstance = instance;
        }

        [HarmonyPatch(nameof(global::UIMouse.UpdateMouseUIInput))]
        [HarmonyPostfix]
        public static void UpdateMouseUIInput() {
            if (!Manager.ui.isMouseShowing) {
                return;
            }
            if (Manager.ui.currentSelectedUIElement == null) {
                return;
            }

            InventorySlotUI? inventorySlotUI = Manager.ui.currentSelectedUIElement as InventorySlotUI;
            if (inventorySlotUI != null) {
                for (int j = 0; j < 10; j++) {
                    if (PlayerInput.WasSlotButtonPressedDownThisFrame(Manager.main.player.inputModule, j, false)) {
                        SwapHotBarItemSlot(inventorySlotUI, j);
                    }
                }
            }
        }

        private static void SwapHotBarItemSlot(InventorySlotUI slotToSwapWith, int objectSlotIndex) {
            if (_slotBarInstance == null) {
                return;
            }

            InventoryObjectSlotUI? objectSlot = _slotBarInstance.GetEquipmentSlot(objectSlotIndex) as InventoryObjectSlotUI;
            if (objectSlot == null) {
                return;
            }

            if (!objectSlot.gameObject.activeInHierarchy) {
                return;
            }

            objectSlot.ObjectID = slotToSwapWith.GetObjectData().objectID;
            objectSlot.UpdateSlot();
        }

    }
}