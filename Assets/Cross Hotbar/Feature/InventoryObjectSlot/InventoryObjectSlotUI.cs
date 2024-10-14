using System;
using Inventory;
using PugMod;
using Unity.Properties;
using UnityEngine;

#nullable enable

namespace CrossHotbar.InventoryObjectSlot {
    [GeneratePropertyBag]
    internal partial class InventoryObjectSlotUI : EquipmentSlotUI {
        public const int NOT_FOUND = -404;

        private ObjectID _objectID;
        public ObjectID ObjectID { get => _objectID; set => _objectID = value; }
        public string ButtonNumber { get; set; } = string.Empty;

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

        protected override ContainedObjectsBuffer GetSlotObject() {
            if (visibleSlotIndex < 0) {
                return default;
            }

            return base.GetSlotObject();
        }

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
            if (ObjectID != ObjectID.None && data.objectID == ObjectID.None) {
                ShowHint(new ObjectDataCD {
                    objectID = ObjectID,
                    amount = 1
                }, false, _itemIsRequired: true);
            }
        }

        private void UpdateVisibleSlotIndex() {
            if (ObjectID == ObjectID.None) {
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

            var slotIndex = InventoryUtility.FindFirstOccurenceOfObject(ObjectID, items, databaseBank);
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

        internal void MixWith(InventorySlotUI original) {
            PropertyContainer.Accept(new ClonePropertiesVisitor<InventorySlotUI>(this), original);
            float alpha = 2 * darkBackground.color.a - MathF.Pow(darkBackground.color.a, 2);
            darkBackground.color = darkBackground.color.ColorWithNewAlpha(alpha);
        }

    }
}
