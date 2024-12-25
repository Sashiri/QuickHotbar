using CrossHotbar.InventoryObjectSlot;
using CrossHotbar.InventoryObjectSlotBar;
using HarmonyLib;
using PlayFab.ExperimentationModels;

#nullable enable

namespace CrossHotbar.Patch {
    [HarmonyPatch(typeof(global::UIMouse))]
    class UIMouse {
        private static InventoryObjectSlotBarUI? _slotBarInstance;
        internal static void SetSlotBarUIInstance(InventoryObjectSlotBarUI? instance) {
            _slotBarInstance = instance;
        }

        [HarmonyPatch(nameof(global::UIMouse.UpdateMouseUIInput))]
        [HarmonyPostfix]
        private static void UpdateMouseUIInput() {
            if (!Manager.ui.isMouseShowing) {
                return;
            }
            if (Manager.ui.currentSelectedUIElement == null) {
                return;
            }

            InventorySlotUI? inventorySlotUI = Manager.ui.currentSelectedUIElement as InventorySlotUI;
            if (inventorySlotUI != null) {
                for (int j = 0; j < global::PlayerController.MAX_EQUIPMENT_SLOTS; j++) {
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

            var item = slotToSwapWith.GetObjectData();
            objectSlot.UpdateSlot(new(item.objectID, new(item.variation)));
        }

    }
}