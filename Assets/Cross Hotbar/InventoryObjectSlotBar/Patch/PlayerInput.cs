using HarmonyLib;

#nullable enable

namespace CrossHotbar.InventoryObjectSlotBar.Patch {
    [HarmonyPatch(typeof(global::PlayerInput))]
    class PlayerInput {
        private static InventoryObjectSlotBarUI? _slotBarInstance;
        public static void SetSlotBarUIInstance(InventoryObjectSlotBarUI? instance) {
            _slotBarInstance = instance;
        }

        [HarmonyPatch(nameof(global::PlayerInput.WasSlotButtonPressedDownThisFrame))]
        [HarmonyPostfix]
        private static void Override_WasSlotButtonPressedDownThisFrame(ref bool __result) {
            if (_slotBarInstance == null) {
                return;
            }

            if (_slotBarInstance.itemSlotsRoot.activeSelf) {
                __result = false;
            }
        }

        [HarmonyPatch(nameof(global::PlayerInput.WasSlotButtonPressedDownThisFrame))]
        [HarmonyReversePatch(HarmonyReversePatchType.Original)]
        public static bool WasSlotButtonPressedDownThisFrame(global::PlayerInput __instance, int index, bool discardDisabledInput = false) => throw new System.NotImplementedException();
    }

}