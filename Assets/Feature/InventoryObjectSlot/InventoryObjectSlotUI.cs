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

        internal const int NOT_FOUND = -404;
        public string ButtonNumber { get; set; } = string.Empty;
        public InventoryObjectTracker SlotTracker { get; private set; } = new(ObjectID.None, new(null));

        public void UpdateSlot(InventoryObjectTracker objectTracker) {
            SlotTracker = objectTracker;
            OnTrackingChanged?.Invoke(objectTracker);
            UpdateSlot();
        }

        /// <summary>
        /// UI can be hit casted from anywhere and exists as an entity in the world
        /// we could make <see cref="TrackedObject"/> private but it would only make
        /// the code less readable
        /// </summary>
        internal event Action<InventoryObjectTracker>? OnTrackingChanged;

        protected void MixWith(InventorySlotUI original) {
            PropertyContainer.Accept(new ClonePropertiesVisitor<InventorySlotUI>(this), original);
            float alpha = 2 * darkBackground.color.a - MathF.Pow(darkBackground.color.a, 2);
            darkBackground.color = darkBackground.color.ColorWithNewAlpha(alpha);
        }

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

            if (SlotTracker.ObjectID is ObjectID.None) {
                HideHint();
                return;
            }

            var data = GetSlotObject();
            if (data.objectID is ObjectID.None) {
                var itemCD = new ObjectDataCD {
                    objectID = SlotTracker.ObjectID,
                    amount = 1
                };
                ShowHint(itemCD, false, _itemIsRequired: true);
            }
        }

        private void UpdateVisibleSlotIndex() {
            if (SlotTracker.ObjectID == ObjectID.None) {
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

            var slotIndex = InventoryObjectUtility.FindFirstOccurenceOfTrackedObject(SlotTracker.ObjectID, SlotTracker.Tracking, items, databaseBank);
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
