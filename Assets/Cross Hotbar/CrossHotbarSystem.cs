using PugMod;
using Unity.Assertions;
using UnityEditor;
using UnityEngine;

public class CrossHotbarMod : IMod {
    private GameObject crossbarUIPrefab;

    private GameObject crossbarUI;

    private const float TIMER_INTERVAL = 5.0f;
    private float timer = TIMER_INTERVAL;
    private int i = 0;

    public void EarlyInit() {
    }

    public void Init() {
        Assert.IsNotNull(crossbarUIPrefab, "Missing crossbar UI Prefab");
    }

    private void OnWorldCreated() {
        crossbarUI = Object.Instantiate(crossbarUIPrefab);
    }

    public void Shutdown() {
        if (crossbarUI != null) {
            Object.Destroy(crossbarUI);
        }
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

        if ((timer -= Time.deltaTime) > 0) {
            return;
        }

        Manager.main.player.EquipSlot(10 + i % 10);
        timer += TIMER_INTERVAL;
        i++;

        //Debug.Log(Manager.ui.itemSlotsBar.itemSlotPrefab);
        //initialized = true;


        //Manager.main.player.UnequipEquippedSlot();
        //Manager.ui.OnEquipmentSlotActivated(11);
        //Manager.main.player.UpdateEquippedSlotVisuals();

    }
}
