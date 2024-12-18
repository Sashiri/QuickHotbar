using HarmonyLib;

#nullable enable

namespace CrossHotbar.InventoryObjectSlot.Patch {

    [HarmonyPatch(typeof(global::InventorySlotUI))]
    internal class InventorySlotUI {

        [HarmonyPatch(nameof(global::InventorySlotUI.GetContainedObjectData))]
        [HarmonyPrefix]
        public static bool GetContainedObjectData(global::InventorySlotUI __instance, ref ContainedObjectsBuffer __result) {
            if (__instance is InventoryObjectSlotUI) {
                var slotObjectData = __instance.GetContainedObject();
                __result = slotObjectData;
                return false;
            }
            return true;
        }

    }
}
