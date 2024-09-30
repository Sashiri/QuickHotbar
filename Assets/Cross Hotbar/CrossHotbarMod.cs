using System.Collections.Generic;
using System.Linq;
using CrossHotbar.EquipAnySlot;
using CrossHotbar.InventoryObjectSlotBar;
using PugMod;
using Rewired.UI.ControlMapper;
using Unity.Assertions;
using UnityEngine;

static class ModBundles {
    internal static IEnumerable<AssetBundle> Of(IMod mod) => API.ModLoader.LoadedMods.First(m => m.Handlers.Contains(mod)).AssetBundles;
    internal static IEnumerable<object> LoadAsset(IMod mod, string path) => Of(mod).Select(bundle => bundle.LoadAsset(path));
    internal static IEnumerable<T> LoadAsset<T>(IMod mod, string path) where T : Object =>
        Of(mod).Select(bundle => bundle.LoadAsset<T>(path));
}

namespace CrossHotbar {

    public class CrossHotbarMod : IMod {
        private GameObject crossbarUIPrefab;
        private GameObject crossbarUI;

        public void EarlyInit() {
            crossbarUIPrefab = ModBundles.LoadAsset<GameObject>(this, "Assets/Cross Hotbar/AlternativeHotbar.prefab").Single();
        }

        public void Init() {
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
        }

        public void Update() {
        }

        bool IMod.CanBeUnloaded() => true;
    }

}