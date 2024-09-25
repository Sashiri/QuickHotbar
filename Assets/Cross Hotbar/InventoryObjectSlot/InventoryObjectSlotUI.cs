using System;
using System.Linq;
using Inventory;
using PugMod;
using Unity.Entities.UniversalDelegates;

#nullable enable

namespace CrossHotbar.InventoryObjectSlot {
    internal class InventoryObjectSlotUI : InventorySlotUI {
        private ObjectID _objectID;
        public ObjectID ObjectID { get => _objectID; set => _objectID = value; }
        public string ButtonNumber { get; set; } = string.Empty;
        private void UpdateVisibleSlotIndex() {
            if (ObjectID == ObjectID.None) {
                visibleSlotIndex = -1;
                return;
            }

            var inventoryHandler = GetInventoryHandler();
            if (inventoryHandler is null) {
                visibleSlotIndex = -1;
                return;
            }


            var player = Manager.main.player;
            var inventories = player.querySystem.GetBufferLookup<ContainedObjectsBuffer>(true);
            var items = inventories[inventoryHandler.inventoryEntity];

            var databaseBank = API.Client.World.EntityManager
                .CreateEntityQuery(typeof(PugDatabase.DatabaseBankCD))
                .GetSingleton<PugDatabase.DatabaseBankCD>();

            var slotIndex = InventoryUtility.FindFirstOccurenceOfObject(ObjectID, items, databaseBank);
            if (slotIndex < inventoryHandler.startPosInBuffer || slotIndex > inventoryHandler.startPosInBuffer + inventoryHandler.size) {
                visibleSlotIndex = -1;
                return;
            }

            visibleSlotIndex = slotIndex;
        }

        protected override ContainedObjectsBuffer GetSlotObject() {
            if (visibleSlotIndex < 0) {
                return default;
            }

            return base.GetSlotObject();
        }

        public override void UpdateSlot() {
            UpdateVisibleSlotIndex();
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

        public static Lazy<InventoryObjectSlotUI> Prefab => new(static () => {
            var basePrefab = Manager.ui.itemSlotsBar.itemSlotPrefab;
            var prefab = Instantiate(basePrefab.gameObject);
            prefab.SetActive(false);

            var slotUI = prefab.GetComponent<InventorySlotUI>();
            var slotOverride = prefab.AddComponent<InventoryObjectSlotUI>();

            slotOverride.ApplyPrefab(slotUI);
            float alpha = 2 * slotOverride.darkBackground.color.a - MathF.Pow(slotOverride.darkBackground.color.a, 2);
            slotOverride.darkBackground.color = slotOverride.darkBackground.color.ColorWithNewAlpha(alpha);

            slotUI.enabled = false;

            return slotOverride;
        });

    }
}
