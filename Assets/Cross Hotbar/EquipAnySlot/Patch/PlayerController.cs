using HarmonyLib;
using PlayerState;
using System;
using UnityEngine;
using System.Threading;
using PugMod;
using System.Linq;

#nullable enable

static class PlayerControllerAccessor {
    internal static void SetEquippedSlotIndex(this PlayerController player, int value) =>
        API.Reflection.SetValue(equippedSlotIndex, player, value);

    internal static void SetLastUsedSlotIndex(this PlayerController player, int value) =>
        API.Reflection.SetValue(lastUsedSlotIndex, player, value);

    internal static Type InvokeGetSlotTypeForObjectType(PlayerController player, ObjectType objectType, ObjectDataCD objectData) =>
        API.Reflection.Invoke(GetSlotTypeForObjectType, player, objectType, objectData)
            as Type ?? throw new System.Diagnostics.UnreachableException();

    private static readonly MemberInfo equippedSlotIndex =
        typeof(PlayerController)
            .GetMembersChecked()
            .First(m => m.GetNameChecked() == nameof(equippedSlotIndex));

    private static readonly MemberInfo lastUsedSlotIndex =
        typeof(PlayerController)
            .GetMembersChecked()
            .First(m => m.GetNameChecked() == nameof(lastUsedSlotIndex));

    private static readonly MemberInfo GetSlotTypeForObjectType =
         typeof(PlayerController)
            .GetMembersChecked()
            .First(m => m.GetNameChecked() == nameof(GetSlotTypeForObjectType));
}

namespace CrossHotbar.EquipAnySlot.Patch {
    [HarmonyPatch(typeof(global::PlayerController))]
    internal class PlayerController {
        private static AnySlotController GetState(global::PlayerController player) {
            if (!player.TryGetComponent<AnySlotController>(out var state)) {
                state = player.gameObject.AddComponent<AnySlotController>();
            }
            return state;
        }

        private static bool IsModResponsibility(int index) => index > global::PlayerController.MAX_EQUIPMENT_SLOTS;


        [HarmonyPatch(nameof(global::PlayerController.EquipSlot))]
        [HarmonyPrefix]
        static bool EquipSlot(global::PlayerController __instance, ref bool __result, int slotIndex, ref int ___lastUsedSlotIndex) {
            if (!IsModResponsibility(slotIndex)) {
                return true;
            }

            if (!IsModResponsibility(__instance.equippedSlotIndex)) {
                Manager.ui.itemSlotsBar.itemSlots[__instance.equippedSlotIndex].OnSetSlotInactivate();
                if (__instance.TryGetComponent<SlotBarIntegrationManager>(out var integrationManager)) {
                    integrationManager.Integration.UpdateIndex(__instance);
                }
            }

            if (__instance.IsAnySlotEquipped()) {
                __instance.UnequipEquippedSlot();
            }

            var slotController = GetState(__instance);
            var slot = slotController.Equip(GetSlotTypeForIndex(__instance, slotIndex), slotIndex);
            slot.OnPickUp(__instance, false);

            PlayerControllerAccessor.SetLastUsedSlotIndex(__instance, slotIndex);
            PlayerControllerAccessor.SetEquippedSlotIndex(__instance, slotIndex);
            slot.OnEquip(__instance);

            Manager.ui.playerInventoryUI.OnEquipmentSlotActivated(slotIndex);
            __instance.UpdateEquippedSlotVisuals();

            __result = true;
            return false;
        }

        private static Type GetSlotTypeForIndex(global::PlayerController player, int index) {
            var slotObject = player.GetInventorySlot(index).objectData;
            var objectType = (slotObject.objectID, PugDatabase.GetObjectInfo(slotObject.objectID, slotObject.variation)) switch {
                (ObjectID.None, _) or (_, null) => ObjectType.NonUsable,
                var (_, objectInfo) => objectInfo.objectType
            };
            return PlayerControllerAccessor.InvokeGetSlotTypeForObjectType(player, objectType, slotObject);
        }

        [HarmonyPatch(nameof(global::PlayerController.IsAnySlotEquipped))]
        [HarmonyPrefix]
        /// On the possibility of equippedSlotIndex pointing to managed slot
        /// check if the slot is ours and skip execution if it is
        static bool IsAnySlotEquipped(global::PlayerController __instance, ref bool __result) {
            if (!IsModResponsibility(__instance.equippedSlotIndex)) {
                return true;
            }

            var slotController = GetState(__instance);
            if (!slotController.IsEquipped()) {
                Debug.LogWarning("The slot is not initialized but should be managed already");
                __result = false;
                return false;
            }

            if (!slotController.IsTracking(__instance.equippedSlotIndex)) {
                Debug.LogWarning("The slot is managed but the player holds a different item than tracked");
                Debug.LogWarning($"Index of managed slot: ${slotController.GetSlot().inventoryIndexReference}");
                Debug.LogWarning($"Current index of equipped slot: ${__instance.equippedSlotIndex}");
                return true;
            }

            __result = true;
            return false;
        }

        [HarmonyPatch(nameof(global::PlayerController.UnequipEquippedSlot))]
        [HarmonyPostfix]
        static void UnequipEquippedSlot(global::PlayerController __instance) {
            if (!IsModResponsibility(__instance.equippedSlotIndex)) {
                return;
            }

            var slotController = GetState(__instance);
            if (slotController.IsEquipped()) {
                slotController.Unequip();
            }
        }

        [HarmonyPatch(nameof(global::PlayerController.GetEquippedSlot))]
        [HarmonyPrefix]
        static bool GetEquippedSlot(global::PlayerController __instance, ref EquipmentSlot? __result) {
            if (!IsModResponsibility(__instance.equippedSlotIndex)) {
                return true;
            }

            if (!__instance.IsAnySlotEquipped()) {
                __result = null;
                return false;
            }

            var slotController = GetState(__instance);
            if (!slotController.IsEquipped()) {
                Debug.LogWarning("The slot is not managed but the player reports that it holds a slot equipped");
                return true;
            }
            if (!slotController.IsTracking(__instance.equippedSlotIndex)) {
                Debug.LogWarning("The slot is managed but the player holds a different item than tracked");
                return true;
            }

            __result = slotController.GetSlot();
            return false;
        }

        [HarmonyPatch(nameof(global::PlayerController.GetHeldObject))]
        [HarmonyPrefix]
        static bool GetHeldObject(global::PlayerController __instance, ref ObjectDataCD __result) {
            if (!IsModResponsibility(__instance.equippedSlotIndex)) {
                return true;
            }

            var slotController = GetState(__instance);
            if (!slotController.IsEquipped()) {
                return true;
            }
            if (!slotController.IsTracking(__instance.equippedSlotIndex)) {
                Debug.LogWarning("The slot is managed but the player holds a different item than tracked");
                return true;
            }

            __result = __instance.playerInventoryHandler.GetObjectData(__instance.equippedSlotIndex);
            return false;
        }

        [HarmonyPatch(nameof(global::PlayerController.UpdateEquippedSlotVisuals))]
        [HarmonyPrefix]
        static void UpdateEquippedSlotVisuals_Prefix(global::PlayerController __instance, ref int? __state) {
            if (!IsModResponsibility(__instance.equippedSlotIndex)
                || __instance.entityExist is false
                || __instance.isLocal is false
            ) {
                return;
            }

            var slotController = GetState(__instance);
            if (!slotController.IsEquipped()) {
                return;
            }

            var currentEquipment = EntityUtility.GetComponentData<EquippedObjectCD>(__instance.entity, __instance.world);

            #region UpdateEquippedSlotVisuals Unwind 

            __state = currentEquipment.equippedSlotIndex;
            currentEquipment.equippedSlotIndex = __instance.equippedSlotIndex;
            EntityUtility.SetComponentData(__instance.entity, __instance.world, currentEquipment);
            Debug.Assert(__instance.isLocal);
            __instance.isLocal = false;

            #endregion 

            var isShielding = EntityUtility.GetComponentData<PlayerRoutineCD>(__instance.entity, __instance.world).activeRoutine == PlayerRoutines.Shielding
                && EntityUtility.GetComponentData<UseOffHandStateCD>(__instance.entity, __instance.world).shieldedAmount > 0f;
            var isItemBroken = __instance.HeldItemIsBroken();

            var visuallyEquipped = isShielding ? __instance.GetOffHand() : __instance.playerInventoryHandler.GetContainedObjectData(__instance.equippedSlotIndex);

            var objectID = (isShielding, isItemBroken) switch {
                (true, _) or (false, false) => visuallyEquipped.objectID,
                (false, true) => ObjectID.None
            };
            var variation = visuallyEquipped.variation;
            var amount = visuallyEquipped.amount;
            var auxDataIndex = visuallyEquipped.auxDataIndex;


            if (__instance.clientInput.equippedSlotIndex != __instance.equippedSlotIndex
                || __instance.visuallyEquippedContainedObject.objectID != objectID
                || __instance.visuallyEquippedContainedObject.variation != variation
                || __instance.visuallyEquippedContainedObject.amount != amount
                || __instance.visuallyEquippedContainedObject.auxDataIndex != auxDataIndex
            ) {
                __instance.clientInput.equippedSlotIndex = (byte)__instance.equippedSlotIndex;
                slotController.GetSlot().OnEquip(__instance);
            }
        }

        [HarmonyPatch(nameof(global::PlayerController.UpdateEquippedSlotVisuals))]
        [HarmonyPostfix]
        static void UpdateEquippedSlotVisuals_Postfix(global::PlayerController __instance, int? __state) {
            if (__state == null) {
                return;
            }

            #region UpdateEquippedSlotVisuals Unwind 

            var currentEquipment = EntityUtility.GetComponentData<EquippedObjectCD>(__instance.entity, __instance.world);
            currentEquipment.equippedSlotIndex = __state.Value;
            EntityUtility.SetComponentData(__instance.entity, __instance.world, currentEquipment);
            __instance.isLocal = true;

            #endregion
        }

        [HarmonyPatch(nameof(global::PlayerController.UpdateAllEquipmentSlots))]
        [HarmonyPostfix]
        private static void UpdateAllEquipmentSlots(global::PlayerController __instance) {
            var slotController = GetState(__instance);
            if (!slotController.IsEquipped()) {
                return;
            }

            __instance.EquipSlot(slotController.GetSlot().inventoryIndexReference);
        }

        [HarmonyPatch(nameof(global::PlayerController.UpdateEquipmentSlot))]
        [HarmonyPostfix]
        private static void UpdateEquipmentSlot(global::PlayerController __instance, int index) {
            if (!IsModResponsibility(index)) {
                return;
            }

            var slotController = GetState(__instance);
            if (!slotController.IsEquipped()) {
                return;
            }

            if (!slotController.IsTracking(__instance.equippedSlotIndex)) {
                Debug.LogWarning("The slot is managed but the player holds a different item than tracked");
                return;
            }

            __instance.EquipSlot(slotController.GetSlot().inventoryIndexReference);
        }

        [HarmonyPatch("UpdateInventoryStuff")]
        [HarmonyPrefix]
        private static void UpdateInventoryStuff_Prefix(global::PlayerController __instance, out bool __state) {
            __state = false;

            var slotController = GetState(__instance);
            if (!slotController.IsEquipped() || !__instance.TryGetComponent<SlotBarIntegrationManager>(out var integrationManager)) {
                return;
            }

            __state = true;
            integrationManager.Integration.RevertIndexBeforeUpdate(__instance);
        }

        [HarmonyPatch("UpdateInventoryStuff")]
        [HarmonyPostfix]
        private static void UpdateInventoryStuff_Postfix(global::PlayerController __instance, bool __state) {
            if (!__state || !__instance.TryGetComponent<SlotBarIntegrationManager>(out var integrationManager)) {
                return;
            }

            integrationManager.Integration.UpdateIndex(__instance);
            integrationManager.Integration.ApplyIndexAfterUpdate(__instance);
        }
    }
}
