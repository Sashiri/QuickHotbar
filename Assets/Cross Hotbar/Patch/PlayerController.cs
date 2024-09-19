using CrossHotbar.InventoryObjectSlotBar;
using HarmonyLib;

#nullable enable

namespace CrossHotbar.Patch {
    [HarmonyPatch(typeof(global::PlayerController))]
    class PlayerController {
        private static InventoryObjectSlotBarUI? _slotBarInstance;
        public static void SetSlotBarUIInstance(InventoryObjectSlotBarUI? instance) {
            _slotBarInstance = instance;
        }

        [HarmonyPatch(nameof(UpdateInventoryStuff))]
        [HarmonyPostfix]
        private static void UpdateInventoryStuff(global::PlayerController __instance) {
            if (__instance.isInteractionBlocked) {
                return;
            }

            if (__instance.inputModule.IsButtonCurrentlyDown(global::PlayerInput.InputType.QUICK_SWAP_TORCH)) {
                return;
            }

            if (_slotBarInstance == null) {
                return;
            }

            for (int i = 0; i < 10; i++) {
                if (PlayerInput.WasSlotButtonPressedDownThisFrame(__instance.inputModule, i, false)) {
                    var objectSlot = _slotBarInstance.GetEquipmentSlot(i);
                    if (objectSlot == null) {
                        return;
                    }

                    var buffer = objectSlot.GetContainedObject();
                    if (buffer.objectID == ObjectID.None) {
                        return;
                    }

                    if (!objectSlot.gameObject.activeInHierarchy) {
                        return;
                    }

                    __instance.EquipSlot(objectSlot.inventorySlotIndex);
                }
            }

        }

    }
}
