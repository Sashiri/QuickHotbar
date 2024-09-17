using System.Collections.Generic;
using CrossHotbar.InventoryObjectSlot;
using UnityEngine;
using UnityEngine.Rendering;

#nullable enable

namespace CrossHotbar.InventoryObjectSlotBar {
    class InventoryObjectSlotBarUI : ItemSlotsBarUI {
        protected override void Awake() {
            var group = gameObject.AddComponent<SortingGroup>() ?? throw new System.Diagnostics.UnreachableException();
            group.sortingLayerName = "GUI";
            group.sortingOrder = 10;

            itemSlotPrefab = InventoryObjectSlotUI.Prefab.Value;

            var vanillaHotbar = Manager.ui.itemSlotsBar;
            transform.position = vanillaHotbar.transform.position + new Vector3(UIConst.PIXEL_STEP, UIConst.PIXEL_STEP * 2, 0);
            spread = vanillaHotbar.spread;
            backgroundSR = vanillaHotbar.backgroundSR;
            backgroundBlockCollider = vanillaHotbar.backgroundBlockCollider;

            foreach (var (i, slot) in itemSlots.Enumerate()) {
                if (slot == null || slot is not InventoryObjectSlotUI ui) {
                    return;
                }

                ui.buttonNumber.textString = ((i + 1) % 10).ToString();
            }

            base.Awake();
        }

        protected override void LateUpdate() {
            var crossHotbarKeyDown = Input.GetKey(KeyCode.LeftControl);
            if (itemSlotsRoot.activeSelf && !crossHotbarKeyDown) {
                itemSlotsRoot.SetActive(false);
            }
            else if (Manager.ui.isAnyInventoryShowing && crossHotbarKeyDown) {
                itemSlotsRoot.SetActive(true);
            }
        }
    }

    static class EnumerableExtensions {
        public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> source) {
            var i = 0;
            foreach (var item in source) {
                yield return (i++, item);
            }
        }
    }
}
