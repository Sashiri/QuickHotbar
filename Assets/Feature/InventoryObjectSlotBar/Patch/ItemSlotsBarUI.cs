using HarmonyLib;
using System.Diagnostics;

#nullable enable

namespace CrossHotbar.InventoryObjectSlotBar.Patch {

    [HarmonyPatch(typeof(global::ItemSlotsBarUI))]
    internal class ItemSlotsBarUI {

        [HarmonyPatch("Update")]
        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        public static void Update(global::ItemSlotsBarUI __instance) => throw new UnreachableException();
    }
}
