using CrossHotbar.InventoryObjectSlot;
using Unity.Properties;
using UnityEngine.Rendering;
using UnityEngine;

namespace CrossHotbar.EquipAnySlot {
    [GeneratePropertyBag]
    internal partial class EquipedSlotUI : ItemSlotsBarUI {
        public override int MAX_ROWS => 1;
        public override int MAX_COLUMNS => 1;

        public static GameObject Create() {

            return ObjectExtension.InstantiateWith(Manager.ui.itemSlotsBar, clone => {
                foreach (var slot in clone.itemSlots) {
                    DestroyImmediate(slot.gameObject);
                }
                clone.itemSlots.Clear();

                clone.gameObject
                    .ConfigureComponent<SortingGroup>(group => {
                        group.sortingLayerID = SortingLayerID.GUI;
                        group.sortingOrder = 15;
                    })
                    .ConfigureComponent<EquipedSlotUI>(hotbar => {
                        var slotPrefab = InventoryObjectSlotUI.Create();
                        {
                            slotPrefab.name = "Custom UI Slot Prefab";
                            slotPrefab.SetActive(false);
                            slotPrefab.transform.SetParent(clone.gameObject.transform);
                        }

                        hotbar.MixWith(clone);
                        hotbar.itemSlotPrefab = slotPrefab.GetComponent<SlotUIBase>();
                    });

                clone.itemSlotsRoot.transform.localPosition += (new Vector3(1, 2, -1) * UIConst.PIXEL_STEP); 

                DestroyImmediate(clone);
            });
        }

        internal void MixWith(ItemSlotsBarUI original) {
            PropertyContainer.Accept(new ClonePropertiesVisitor<ItemSlotsBarUI>(this), original);
        }
    }
}

