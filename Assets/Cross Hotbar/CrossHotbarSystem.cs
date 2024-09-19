using CrossHotbar.InventoryObjectSlotBar;
using PugMod;
using Unity.Assertions;
using UnityEngine;

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

    private void OnWorldCreated() {
        if (crossbarUI != null) {
            Debug.LogError("CrossbarUI was already instantiated, dirty cleanup?");
        }
        crossbarUI = Object.Instantiate(crossbarUIPrefab);
        var objectSlotBarUI = crossbarUI.GetComponent<InventoryObjectSlotBarUI>();
        CrossHotbar.InventoryObjectSlotBar.Patch.UIMouse.SetSlotBarUIInstance(objectSlotBarUI);
        CrossHotbar.InventoryObjectSlotBar.Patch.PlayerInput.SetSlotBarUIInstance(objectSlotBarUI);
        CrossHotbar.InventoryObjectSlotBar.Patch.PlayerController.SetSlotBarUIInstance(objectSlotBarUI);
    }

    private void OnWorldDestroyed() {
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
        if (API.Client.World is null) {
            return;
        }

        if (Manager.ui.itemSlotsBar == null || Manager.ui.itemSlotsBar.itemSlotPrefab == null) {
            return;
        }

        if (Manager.main.player == null) {
            return;
        }

        if (crossbarUI == null) {
            OnWorldCreated();
        }
    }
}
