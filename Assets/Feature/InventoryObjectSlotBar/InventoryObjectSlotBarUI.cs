using System.Linq;
using CrossHotbar.InventoryObjectSlot;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Rendering;

#nullable enable

namespace CrossHotbar.InventoryObjectSlotBar {
    [GeneratePropertyBag]
    partial class InventoryObjectSlotBarUI : ItemSlotsBarUI {
        public SpriteRenderer? backgroundSprite;
        private UIelement? targetedUI;

        public static GameObject Create() {
            return ObjectExtension.InstantiateWith(Manager.ui.itemSlotsBar, clonedSlotBarScript => {
                foreach (var slot in clonedSlotBarScript.itemSlots) {
                    DestroyImmediate(slot.gameObject);
                }
                clonedSlotBarScript.itemSlots.Clear();

                clonedSlotBarScript.gameObject
                    .ConfigureComponent<SortingGroup>(group => {
                        group.sortingLayerID = SortingLayerID.GUI;
                        group.sortingOrder = 20;
                    })
                    .ConfigureComponent<InventoryObjectSlotBarUI>(hotbar => {
                        var slotPrefab = InventoryObjectSlotUI.Create();
                        {
                            slotPrefab.name = "Custom UI Slot Prefab";
                            slotPrefab.SetActive(false);
                            slotPrefab.transform.SetParent(clonedSlotBarScript.gameObject.transform);
                        }

                        hotbar.MixWith(clonedSlotBarScript);
                        hotbar.itemSlotPrefab = slotPrefab.GetComponent<SlotUIBase>();

                        if (Manager.ui.playerInventoryUI is InventoryUI inventory) {
                            var bgSprite = Instantiate(inventory.topRowBackgroundSR, hotbar.itemSlotsRoot.transform);
                            bgSprite.transform.localPosition = Vector3.zero;
                            bgSprite.SetAlpha(0.8f);
                            hotbar.backgroundSprite = bgSprite;
                        }
                    });

                DestroyImmediate(clonedSlotBarScript);
            });
        }

        public override void Init() {
            base.Init();

            foreach (var (i, slotUI) in itemSlots.Enumerate()) {
                if (slotUI == null || slotUI is not InventoryObjectSlotUI objectSlotUI) {
                    continue;
                }

                objectSlotUI.UpdateSlot();
                objectSlotUI.ButtonNumber = ((i + 1) % 10).ToString();
            }
        }

        protected override void LateUpdate() {
            base.LateUpdate();

            var crossHotbarKeyDown = Input.GetKey(KeyCode.LeftControl);
            if (backgroundSprite != null) {
                backgroundSprite.gameObject.SetActive(false);
            }
            if (!crossHotbarKeyDown) {
                itemSlotsRoot.SetActive(false);
            }
            else if (Manager.ui.isAnyInventoryShowing && crossHotbarKeyDown) {
                UpdateInInventory();
            }
            UpdatePosition();
        }

        private void UpdateInInventory() {
            Ray ray = new(Manager.ui.mouse.pointer.transform.position + Vector3.back * 5f, Vector3.forward);

            RaycastHit[] raycastHitsNoSpan = new RaycastHit[8];
            var hits = Physics.SphereCastNonAlloc(ray, Constants.PIXEL_STEP * 1.5f, raycastHitsNoSpan, 10f, ObjectLayerID.UILayerMask);
            targetedUI = raycastHitsNoSpan.Take(hits)
                .Select(hit => (hit.distance, ui: hit.collider.GetComponent<UIelement>()))
                .Where(v =>
                    v.ui.isVisibleOnScreen
                    && v.ui is InventorySlotUI slotUI
                    && slotUI.GetInventoryHandler() == Manager.main.player.playerInventoryHandler
                )
                .SortedTakeBy(v => v.distance, 1)
                .FirstOrDefault().ui as InventorySlotUI;

            if (targetedUI == null) {
                return;
            }

            itemSlotsRoot.SetActive(true);
            if (backgroundSprite != null) {
                backgroundSprite.gameObject.SetActive(true);
            }
        }

        public void UpdatePosition() {
            if (Manager.ui.isAnyInventoryShowing && itemSlotsRoot.activeInHierarchy && targetedUI != null) {
                var offset = Vector3.up * spread + (new Vector3(0, 2, -2) * Constants.PIXEL_STEP);
                transform.position = Vector3.Scale(targetedUI.transform.position, Vector3.up) + offset;
            }
            else {
                transform.position = Manager.ui.itemSlotsBar.transform.position + (new Vector3(1, 2, -2) * Constants.PIXEL_STEP);
            }
        }

        internal void MixWith(ItemSlotsBarUI original) {
            PropertyContainer.Accept(new ClonePropertiesVisitor<ItemSlotsBarUI>(this), original);
        }
    }
}
