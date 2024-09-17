using System.Linq;
using PugMod;
using UnityEngine;

namespace CrossHotbar.EquipFromInventory {
    internal class ObjectPreviewSlotUI : SlotUIBase {
        static public SlotUIBase Prefab {
            get {
                var basePrefab = Manager.ui.itemSlotsBar.itemSlotPrefab;
                var prefab = Instantiate(basePrefab.gameObject);
                prefab.SetActive(false);

                var slotBase = prefab.GetComponent<InventorySlotUI>();
                if (slotBase.hintContainer != null) {
                    slotBase.hintContainer.SetActive(false);
                }
                if (slotBase.barContainer != null) {
                    slotBase.barContainer.SetActive(false);
                }
                if (slotBase.cooldownPivot != null) {
                    slotBase.cooldownPivot.localScale = Vector3.zero;
                }

                prefab.AddComponent<ObjectPreviewSlotUI>();
                var slot = prefab.GetComponent<ObjectPreviewSlotUI>();

                slot.uiSlotXPosition = 0;
                slot.uiSlotYPosition = 0;
                slot.visibleSlotIndex = 0;

                slot.topUIElements = slotBase.topUIElements;
                slot.bottomUIElements = slotBase.bottomUIElements;
                slot.leftUIElements = slotBase.leftUIElements;
                slot.rightUIElements = slotBase.rightUIElements;
                slot.childElements = slotBase.childElements;

                slot.activeBorder = slotBase.activeBorder;
                slot.hoverBorder = slotBase.hoverBorder;

                slot.icon = slotBase.icon;
                slot.overlayIcon = slotBase.overlayIcon;
                slot.underlayIcon = slotBase.underlayIcon;
                slot.upgradeIcon = slotBase.upgradeIcon;
                slot.upgradeSprites = slotBase.upgradeSprites;
                slot.slotType = ItemSlotsUIType.PlayerInventorySlot;

                slot.background = slotBase.background;
                slot.darkBackground = slotBase.darkBackground;

                slot.amountNumber = slotBase.amountNumber;
                slot.amountNumberShadow = slotBase.amountNumberShadow;

                slot.disableAfterSeconds = slotBase.disableAfterSeconds;
                slot.autoDisableAnimator = slotBase.autoDisableAnimator;
                slot.selectFirstEnabledElementInList = slotBase.selectFirstEnabledElementInList;
                slot.hoverTitleWhenEmpty = slotBase.hoverTitleWhenEmpty;
                slot.dontWrapAroundNavigation = slotBase.dontWrapAroundNavigation;

                var animatorInfo = typeof(SlotUIBase).GetMembersChecked()
                    .FirstOrDefault(m => m.GetNameChecked() == "animator");

                Debug.Assert(animatorInfo != null, "Breaking change in the SlotUi structure, TODO: disable mod");

                API.Reflection.SetValue(animatorInfo, slot, API.Reflection.GetValue(animatorInfo, slotBase));

                slot.name = nameof(ObjectPreviewSlotUI);
                slot.tag = slotBase.tag;
                slot.useGUILayout = slotBase.useGUILayout;
                slot.hideFlags = slotBase.hideFlags;
                slot.enabled = slotBase.enabled;

                Destroy(slotBase);

                return slot;
            }
        }

        public override void UpdateSlot() {
            //Scale to 0,84 0,84 1?
            //this.icon.color = new Color(0.8f, 0.8f, 0.8f, 0.5f); if not found
            //		this.RenderButtonNumber();
            // 0.085625
            // 0,0625
            // 0,75 0,75 1

            ContainedObjectsBuffer containedObjectsBuffer = GetSlotObject();

            // if (containedObjectsBuffer.objectID == ObjectID.None && EntityUtility.HasComponentData<ChangeVariationWhenContainingObjectCD>(this.inventoryHandler.entityMonoBehaviour.entity, base.world))
            // {
            //     objectID = EntityUtility.GetComponentData<ChangeVariationWhenContainingObjectCD>(this.inventoryHandler.entityMonoBehaviour.entity, base.world).objectID;
            // }
            bool showOverlay = Manager.ui.ShouldShowCageOverlay(containedObjectsBuffer);
            if (overlayIcon != null) {
                overlayIcon.gameObject.SetActive(showOverlay);
            }
            if (underlayIcon != null) {
                underlayIcon.gameObject.SetActive(showOverlay);
            }
            if (darkBackground != null) {
                darkBackground.enabled = true;
            }

            UpdateIcon(containedObjectsBuffer);
            UpdateUpgradeIcon(containedObjectsBuffer);
        }

        private void UpdateIcon(ContainedObjectsBuffer buffer) {
            if (buffer.objectID == ObjectID.None) return;

            ObjectInfo objectInfo = PugDatabase.GetObjectInfo(buffer.objectID, buffer.variation);
            if (objectInfo == null) return;

            icon.sprite = GetIcon(objectInfo, buffer.objectData);
            Manager.ui.ApplyAnyIconGradientMap(buffer, icon);
            icon.transform.localPosition = objectInfo.iconOffset;
        }

    }
}