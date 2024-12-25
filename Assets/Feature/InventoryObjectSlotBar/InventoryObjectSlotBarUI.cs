using System;
using System.Linq;
using CrossHotbar.InventoryObjectSlot;
using PugMod;
using Unity.Entities.UniversalDelegates;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Rendering;

#nullable enable

namespace CrossHotbar.InventoryObjectSlotBar {
    [GeneratePropertyBag]
    partial class InventoryObjectSlotBarUI : ItemSlotsBarUI {
        public static GameObject Create(Action<int, InventoryObjectSlotUI>? onSlotInitialization = null) {
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
                        if (onSlotInitialization != null) {
                            hotbar.OnSlotInitialization += onSlotInitialization;
                        }
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

        public SpriteRenderer? backgroundSprite;
        private readonly GameObject _visibilityTracker = new("Visibility Tracker");

        /// <summary>
        /// Called with both index of the slot and slot itself
        /// </summary>
        public event Action<int, InventoryObjectSlotUI>? OnSlotInitialization;

        protected virtual void Start() {
            _visibilityTracker.transform.parent = gameObject.transform;
        }
        private void MixWith(ItemSlotsBarUI original) {
            PropertyContainer.Accept(new ClonePropertiesVisitor<ItemSlotsBarUI>(this), original);
        }

        public override void Init() {
            base.Init();

            for(var i = 0; i < itemSlots.Count; ++i) {
                if (itemSlots[i] is not null and InventoryObjectSlotUI objectSlotUI) {
                    InitializeSlotUI(i, objectSlotUI);
                }
            }
        }

        private void InitializeSlotUI(int index, InventoryObjectSlotUI slotUI) {
            slotUI.ButtonNumber = ((index + 1) % 10).ToString();
            slotUI.UpdateSlot();
            OnSlotInitialization?.Invoke(index, slotUI);
        }

        // Have mercy on my soul, it's private and we dont want to redeclare logic
        // ItemSlotsRoot cannot be reenabled, it causes Unity to drop update for contained slots
        private void Update() {
            // Workaround for root reenabling, track how it would behave and then apply
            var rootTemp = itemSlotsRoot;
            itemSlotsRoot = _visibilityTracker;
            Patch.ItemSlotsBarUI.Update(this);
            itemSlotsRoot = rootTemp;

            var crossHotbarKeyDown = Input.GetKey(KeyCode.LeftControl);

            if (!crossHotbarKeyDown) {
                if (itemSlotsRoot.activeSelf) {
                    itemSlotsRoot.SetActive(false);
                }
                return;
            }

            if (Manager.ui.isAnyInventoryShowing) {
                UpdateInInventory();
            }
            else {
                UpdateInGame();
            }
        }

        /// <summary>
        /// Tries to show hotbar above any player's inventory slot
        /// </summary>
        private void UpdateInInventory() {
            Ray ray = new(Manager.ui.mouse.pointer.transform.position + Vector3.back * 5f, Vector3.forward);

            RaycastHit[] raycastHitsNoSpan = new RaycastHit[8];
            var hits = Physics.SphereCastNonAlloc(ray, Constants.PIXEL_STEP * 1.5f, raycastHitsNoSpan, 10f, ObjectLayerID.UILayerMask);
            var targetedUI = raycastHitsNoSpan.Take(hits)
                .Select(hit => (hit.distance, ui: hit.collider.GetComponent<UIelement>()))
                .Where(v =>
                    v.ui.isVisibleOnScreen
                    && v.ui is InventorySlotUI slotUI
                    && slotUI.GetInventoryHandler() == Manager.main.player.playerInventoryHandler
                )
                .SortedTakeBy(v => v.distance, 1)
                .FirstOrDefault().ui as InventorySlotUI;

            if (targetedUI == null) {
                // Brain lag, could be implemented better, rn just apply the visibility
                if (itemSlotsRoot.activeSelf != _visibilityTracker.activeSelf) {
                    itemSlotsRoot.SetActive(_visibilityTracker.activeSelf);
                }
                return;
            }

            // We have all informations and prerequisites to show the hotbar
            itemSlotsRoot.SetActive(true);

            var offset = Vector3.up * spread + (new Vector3(0, 2, -2) * Constants.PIXEL_STEP);
            transform.position = Vector3.Scale(targetedUI.transform.position, Vector3.up) + offset;


            if (backgroundSprite != null) {
                backgroundSprite.gameObject.SetActive(true);
            }
        }

        private void UpdateInGame() {
            // Brain lag, could be implemented better, rn just apply the visibility
            if (itemSlotsRoot.activeSelf != _visibilityTracker.activeSelf) {
                itemSlotsRoot.SetActive(_visibilityTracker.activeSelf);
            }
            transform.position = Manager.ui.itemSlotsBar.transform.position + (new Vector3(1, 2, -2) * Constants.PIXEL_STEP);

            if (backgroundSprite != null) {
                backgroundSprite.gameObject.SetActive(false);
            }
        }
    }
}
