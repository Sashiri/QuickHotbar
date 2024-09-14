using PugMod;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class CrossHotbarUI : ItemSlotsBarUI {
    public override void Init() {
        var vanillaHotbar = Manager.ui.itemSlotsBar;
        itemSlotPrefab = vanillaHotbar.itemSlotPrefab;
        spread = vanillaHotbar.spread;
        backgroundSR = vanillaHotbar.backgroundSR;
        backgroundBlockCollider = vanillaHotbar.backgroundBlockCollider;
        base.Init();

        foreach (var slot in itemSlots) {
            slot.visibleSlotIndex = slot.visibleSlotIndex + 10;
        }

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
