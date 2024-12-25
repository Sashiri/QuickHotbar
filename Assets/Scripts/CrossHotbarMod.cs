using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CrossHotbar.EquipAnySlot;
using CrossHotbar.InventoryObjectSlotBar;
using PugMod;
using Unity.Properties;
using UnityEngine;

#nullable enable

[assembly: GeneratePropertyBagsForAssembly]

static class ModBundles {
    internal static IEnumerable<AssetBundle> GetAssetBundles(IMod mod) => API.ModLoader.LoadedMods.First(m => m.Handlers.Contains(mod)).AssetBundles;
    internal static IEnumerable<object> LoadAsset(IMod mod, string path) => GetAssetBundles(mod).Select(bundle => bundle.LoadAsset(path));
    internal static IEnumerable<T> LoadAsset<T>(IMod mod, string path) where T : Object =>
        GetAssetBundles(mod).Select(bundle => bundle.LoadAsset<T>(path));
}

namespace CrossHotbar {

    public class CrossHotbarMod : IMod {
        private GameObject? crossbarUI;
#pragma warning disable CS8618 // Initialized in Early init.
        private LoadedMod info;
#pragma warning restore CS8618

        void IMod.EarlyInit() {
            info = API.ModLoader.LoadedMods.First(m => m.Handlers.Contains(this));
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
            Patch.PlayerController.OnPlayerOccupied += OnPlayerOccupied;
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

            Debug.Log("Configuring QuickHotbar, instantiating prefabs");

            Debug.Assert(crossbarUI == null, "CrossbarUI was already instantiated, dirty cleanup?");

            crossbarUI = InventoryObjectSlotBarUI.Create(OnTrackableSlotInitialization);
            Object.DontDestroyOnLoad(crossbarUI);

            Debug.Log("Configuring QuickHotbar, integrating crossbar systems");

            var objectSlotBarUI = crossbarUI.GetComponent<InventoryObjectSlotBarUI>();
            Patch.UIMouse.SetSlotBarUIInstance(objectSlotBarUI);
            Patch.PlayerInput.SetSlotBarUIInstance(objectSlotBarUI);
            Patch.PlayerController.SetSlotBarUIInstance(objectSlotBarUI);

            Debug.Log("Configuring QuickHotbar, integration finished");

            playerController.gameObject.ConfigureComponent<SlotBarIntegrationManager>(manager => {
                manager.Integration = manager.gameObject.AddComponent<DefaultSlotBarIntegration>();
            });
        }

        private void OnTrackableSlotInitialization(int index, InventoryObjectSlot.InventoryObjectSlotUI slotUI) {
            var characterId = Manager.saves.GetCharacterId();
            if (TryLoadSlotForCharacter(characterId, index, out var tracker)) {
                slotUI.UpdateSlot(tracker);
            }

            slotUI.OnTrackingChanged += (tracker) => SaveSlotForCharacter(characterId, index, tracker);
        }

        private bool TryLoadSlotForCharacter(int characterId, int index, out InventoryObjectSlot.InventoryObjectTracker tracker) {
            tracker = default;
            var mod = info.ModId.ToString();
            var section = characterId.ToString();
            var key = index.ToString();

            if (API.Config.TryGet(mod, section, key, out string config)) {
                tracker = JsonSerializer.Deserialize<InventoryObjectSlot.InventoryObjectTracker>(config);
                return true;
            }
            return false;
        }

        private void SaveSlotForCharacter(int characterId, int index, InventoryObjectSlot.InventoryObjectTracker tracker) {
            var mod = info.ModId.ToString();
            var section = characterId.ToString();
            var key = index.ToString();

            API.Config.Set(info.ModId.ToString(), characterId.ToString(), index.ToString(), JsonSerializer.Serialize(tracker));
        }
    }

}

