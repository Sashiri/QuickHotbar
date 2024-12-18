using PugMod;
using System;
using Unity.Properties;
using UnityEngine;

#nullable enable

namespace CrossHotbar.InventoryObjectSlot {

    [GeneratePropertyBag]
    internal partial class InventoryObjectSlotUI : EquipmentSlotUI {
        public static GameObject Create() {
            var prefab = Manager.ui.itemSlotsBar.itemSlotPrefab;
            if (prefab is not InventorySlotUI itemSlotOriginalCast) {
                throw new InvalidOperationException($"Slot prefab is not of the expected type, expected: {typeof(InventorySlotUI)}, got {prefab.GetType()}");
            }
            return ObjectExtension.InstantiateWith(prefab, clone => {
                clone.gameObject.ConfigureComponent<InventoryObjectSlotUI>(slotUI => {
                    slotUI.MixWith((InventorySlotUI)clone);
                });

                DestroyImmediate(clone);
            });
        }

        public void SetTrackedObject(ObjectID objectID, InventoryObjectUtility.TrackingPreference preference) {
            _objectID = objectID;
            _trackingPreference = preference;
        }

        public string ButtonNumber { get; set; } = string.Empty;


        private ObjectID _objectID;
        private InventoryObjectUtility.TrackingPreference _trackingPreference = new(Variation: 0);
        protected void MixWith(InventorySlotUI original) {
            PropertyContainer.Accept(new ClonePropertiesVisitor<InventorySlotUI>(this), original);
            float alpha = 2 * darkBackground.color.a - MathF.Pow(darkBackground.color.a, 2);
            darkBackground.color = darkBackground.color.ColorWithNewAlpha(alpha);
        }

        internal const int NOT_FOUND = -404;


        /// <summary>
        /// Handle <see cref="NOT_FOUND"/> slot index, this is a result of using visible slot index as dynamic lookup variable
        /// </summary>
        /// <returns></returns>
        protected override ContainedObjectsBuffer GetSlotObject() {
            if (visibleSlotIndex < 0) {
                return default;
            }

            return base.GetSlotObject();
        }

        /// <summary>
        /// Tries to find a matching item in player's inventory based on tracked <see cref="ObjectID"/> and <see cref="InventoryObjectUtility.TrackingPreference"/>\
        /// Displays hint if the item could not be found
        /// </summary>
        public override void UpdateSlot() {
            UpdateVisibleSlotIndex();

            if (slotType == ItemSlotsUIType.PlayerInventorySlot) {
                if (Manager.main != null
                    && Manager.main.player != null
                    && Manager.main.player.GetEquippedSlot().inventoryIndexReference == visibleSlotIndex) {
                    OnSetSlotActive();
                }
                else {
                    OnSetSlotInactivate();
                }
            }

            base.UpdateSlot();

            RenderButtonNumber();

            var data = GetSlotObject();
            if (_objectID != ObjectID.None && data.objectID == ObjectID.None) {
                ShowHint(new ObjectDataCD {
                    objectID = _objectID,
                    amount = 1
                }, false, _itemIsRequired: true);
            }
        }

        private void UpdateVisibleSlotIndex() {
            if (_objectID == ObjectID.None) {
                visibleSlotIndex = NOT_FOUND;
                return;
            }

            var inventoryHandler = GetInventoryHandler();
            if (inventoryHandler is null) {
                visibleSlotIndex = NOT_FOUND;
                return;
            }


            var player = Manager.main.player;
            var inventories = player.querySystem.GetBufferLookup<ContainedObjectsBuffer>(true);
            var items = inventories[inventoryHandler.inventoryEntity];

            var databaseBank = API.Client.GetEntityQuery(typeof(PugDatabase.DatabaseBankCD))
                .GetSingleton<PugDatabase.DatabaseBankCD>();

            var slotIndex = InventoryObjectUtility.FindFirstOccurenceOfTrackedObject(_objectID, _trackingPreference, items, databaseBank);
            if (slotIndex < inventoryHandler.startPosInBuffer || slotIndex > inventoryHandler.startPosInBuffer + inventoryHandler.size) {
                visibleSlotIndex = NOT_FOUND;
                return;
            }

            visibleSlotIndex = slotIndex;
        }

        private void RenderButtonNumber() {
            if (buttonNumber == null) {
                return;
            }

            if (Manager.prefs.ShowHotbarKeyboardNumbers) {
                buttonNumber.gameObject.SetActive(value: true);
                if (buttonNumber.displayedTextString != ButtonNumber) {
                    buttonNumber.Render(ButtonNumber);
                }
            }
            else {
                buttonNumber.gameObject.SetActive(value: false);
            }
        }

    }
}
