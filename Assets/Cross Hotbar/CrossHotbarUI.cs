using CrossHotbar.EquipFromInventory;
using System.Collections.Generic;
using UnityEngine;


class CrossHotbarUI : ItemSlotsBarUI {
    private SlotUIBase blablabla;
    public override void Init() {
        var vanillaHotbar = Manager.ui.itemSlotsBar;
        itemSlotPrefab = ObjectPreviewSlotUI.Prefab;
        // itemSlotPrefab = vanillaHotbar.itemSlotPrefab;
        spread = vanillaHotbar.spread;
        backgroundSR = vanillaHotbar.backgroundSR;
        backgroundBlockCollider = vanillaHotbar.backgroundBlockCollider;

        base.Init();

        Debug.Log($"Initialized the {nameof(CrossHotbarUI)}");
    }
}

internal static class Ext {
    internal static IEnumerable<(uint, T)> Enumerate<T>(IEnumerable<T> enumerable) {
        uint i = 0;
        foreach (var item in enumerable) {
            yield return (i++, item);
        }
    }
}
