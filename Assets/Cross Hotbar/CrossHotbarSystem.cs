using CrossHotbar.EquipAnySlot;
using CrossHotbar.InventoryObjectSlotBar;
using PugMod;
using Unity.Assertions;
using UnityEngine;

namespace CrossHotbar {

    public class CrossHotbarMod : IMod {
        private GameObject crossbarUIPrefab;
        private GameObject crossbarUI;

        public void EarlyInit() {
        }

        public void Init() {
            Assert.IsNotNull(crossbarUIPrefab, "Missing crossbar UI Prefab");
            API.Client.OnWorldCreated += OnWorldCreated;
            API.Client.OnWorldDestroyed += OnWorldDestroyed;
        }

        private void OnPlayerOccupied(PlayerController playerController) {
            playerController.gameObject.AddComponent<SlotBarIntegrationManager>();
        }

        private void OnWorldCreated() {
            if (crossbarUI != null) {
                Debug.LogError("CrossbarUI was already instantiated, dirty cleanup?");
            }
            crossbarUI = Object.Instantiate(crossbarUIPrefab);
            var objectSlotBarUI = crossbarUI.GetComponent<InventoryObjectSlotBarUI>();
            Patch.UIMouse.SetSlotBarUIInstance(objectSlotBarUI);
            Patch.PlayerInput.SetSlotBarUIInstance(objectSlotBarUI);
            Patch.PlayerController.SetSlotBarUIInstance(objectSlotBarUI);
            Patch.PlayerController.OnPlayerOccupied += OnPlayerOccupied;
        }

        private void OnWorldDestroyed() {
            Patch.PlayerController.OnPlayerOccupied -= OnPlayerOccupied;

            if (crossbarUI != null) {
                Object.Destroy(crossbarUI);
            }
        }

        public void Shutdown() {
            API.Client.OnWorldCreated -= OnWorldCreated;
            API.Client.OnWorldDestroyed -= OnWorldDestroyed;
        }

        public void ModObjectLoaded(Object obj) {
            if (obj.name is "AlternativeHotbar" && obj is GameObject prefab) {
                crossbarUIPrefab = prefab;
            }
        }

        public void Update() {
        }
    }

}