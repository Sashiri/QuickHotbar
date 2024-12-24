using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrossHotbar.EquipAnySlot;
using CrossHotbar.InventoryObjectSlotBar;
using PugMod;
using Unity.Properties;
using UnityEngine;

[assembly: GeneratePropertyBagsForAssembly]

static class ModBundles {
    internal static IEnumerable<AssetBundle> GetAssetBundles(IMod mod) => API.ModLoader.LoadedMods.First(m => m.Handlers.Contains(mod)).AssetBundles;
    internal static IEnumerable<object> LoadAsset(IMod mod, string path) => GetAssetBundles(mod).Select(bundle => bundle.LoadAsset(path));
    internal static IEnumerable<T> LoadAsset<T>(IMod mod, string path) where T : Object =>
        GetAssetBundles(mod).Select(bundle => bundle.LoadAsset<T>(path));
}

namespace CrossHotbar {

    public class CrossHotbarMod : IMod {
        private GameObject crossbarUI;

        void IMod.EarlyInit() {
        }

        void IMod.Init() {
            API.Client.OnWorldCreated += OnWorldCreated;
            API.Client.OnWorldDestroyed += OnWorldDestroyed;
        }

        void IMod.Shutdown() {
            API.Client.OnWorldCreated -= OnWorldCreated;
            API.Client.OnWorldDestroyed -= OnWorldDestroyed;
        }

        void IMod.ModObjectLoaded(Object obj) {
        }

        void IMod.Update() {
        }

        bool IMod.CanBeUnloaded() => true;

        private void OnWorldCreated() {
            Debug.Log("Configuring QuickHotbar, instantiating prefabs");

            Debug.Assert(crossbarUI == null, "CrossbarUI was already instantiated, dirty cleanup?");
            crossbarUI = InventoryObjectSlotBarUI.Create();
            // Required, multiplayer has a two stage load, world exists before the player is fully 
            // loaded at the character selection screen
            Object.DontDestroyOnLoad(crossbarUI);

            Debug.Log("Configuring QuickHotbar, integrating crossbar systems");
            var objectSlotBarUI = crossbarUI.GetComponent<InventoryObjectSlotBarUI>();
            Patch.UIMouse.SetSlotBarUIInstance(objectSlotBarUI);
            Patch.PlayerInput.SetSlotBarUIInstance(objectSlotBarUI);
            Patch.PlayerController.SetSlotBarUIInstance(objectSlotBarUI);
            Patch.PlayerController.OnPlayerOccupied += OnPlayerOccupied;
            Debug.Log("Configuring QuickHotbar, integration finished");
        }

        private void OnWorldDestroyed() {
            Patch.PlayerController.OnPlayerOccupied -= OnPlayerOccupied;

            if (crossbarUI != null) {
                Object.Destroy(crossbarUI);
            }
        }

        private void OnPlayerOccupied(PlayerController playerController) {
            if (!playerController.isLocal) {
                return;
            }
            playerController.gameObject.ConfigureComponent<SlotBarIntegrationManager>(manager => {
                manager.Integration = manager.gameObject.AddComponent<DefaultSlotBarIntegration>();
            });
        }
    }

}
