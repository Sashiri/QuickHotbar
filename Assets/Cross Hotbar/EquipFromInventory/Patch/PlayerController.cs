using HarmonyLib;
using PlayerState;
using System;
using UnityEngine;
using System.Threading;

#nullable enable

namespace CrossHotbar.EquipFromInventory {

    [HarmonyPatch(typeof(global::PlayerController))]
    class PlayerController {
        internal static readonly object _lock = new();
        internal static EquipmentSlot? _slot = null;
        public static EquipmentSlot? Slot => _slot;

        [HarmonyPatch(nameof(global::PlayerController.equippedSlotIndex), MethodType.Setter)]
        [HarmonyReversePatch]
        static private void SetEquippedSlotIndex(global::PlayerController __instance, int value) => throw new NotImplementedException();

        [HarmonyPatch(nameof(global::PlayerController.OnFree))]
        [HarmonyPrefix]
        static void OnFree(PlayerController __instance) {
            var slot = Interlocked.Exchange(ref _slot, null);
            if (slot != null) {
                slot.Free();
            }
        }

        [HarmonyPatch(nameof(global::PlayerController.EquipSlot))]
        [HarmonyPrefix]
        static bool EquipSlot(global::PlayerController __instance, ref bool __result, int slotIndex, ref int ___lastUsedSlotIndex) {
            if (slotIndex < global::PlayerController.MAX_EQUIPMENT_SLOTS) {
                return true;
            }

            if (__instance.equippedSlotIndex < global::PlayerController.MAX_EQUIPMENT_SLOTS) {
                Manager.ui.itemSlotsBar.itemSlots[__instance.equippedSlotIndex].OnSetSlotInactivate();
            }

            if (__instance.IsAnySlotEquipped()) {
                __instance.UnequipEquippedSlot();
            }

            lock (_lock) {
                _slot = CreateEquipmentSlot(__instance, slotIndex);

                ___lastUsedSlotIndex = slotIndex;
                SetEquippedSlotIndex(__instance, slotIndex);
                _slot.OnEquip(__instance);
                Manager.ui.playerInventoryUI.OnEquipmentSlotActivated(slotIndex);
                __instance.UpdateEquippedSlotVisuals();
            }

            __result = true;
            return false;
        }

        [HarmonyPatch(nameof(global::PlayerController.IsAnySlotEquipped))]
        [HarmonyPrefix]
        /// On the possibility of equippedSlotIndex pointing to managed slot
        /// check if the slot is ours and skip execution if it is
        static bool IsAnySlotEquipped(global::PlayerController __instance, ref bool __result) {
            if (__instance.equippedSlotIndex < global::PlayerController.MAX_EQUIPMENT_SLOTS) {
                return true;
            }

            if (_slot == null) {
                Debug.LogError("The slot is not initialized but should be managed already");
                __result = false;
                return false;
            }

            if (_slot.inventoryIndexReference != __instance.equippedSlotIndex) {
                Debug.LogError("The slot is managed but the player holds a different item than tracked");
                return true;
            }

            __result = true;
            return false;
        }

        [HarmonyPatch(nameof(global::PlayerController.UnequipEquippedSlot))]
        [HarmonyPostfix]
        static void UnequipEquippedSlot(global::PlayerController __instance) {
            if (_slot == null || __instance.equippedSlotIndex < global::PlayerController.MAX_EQUIPMENT_SLOTS) {
                return;
            }

            var slot = Interlocked.Exchange(ref _slot, null);
            if (slot != null) {
                slot.Free();
            }
        }

        private static EquipmentSlot CreateEquipmentSlot(global::PlayerController player, int index) {
            var slotObject = player.playerInventoryHandler.GetObjectData(index);
            var objectType = (slotObject.objectID, PugDatabase.GetObjectInfo(slotObject.objectID, slotObject.variation)) switch {
                (ObjectID.None, _) or (_, null) => ObjectType.NonUsable,
                var (_, objectInfo) => objectInfo.objectType
            };
            EquipmentSlot? equipmentSlot = Manager.memory.GetFreeComponent(
                GetSlotTypeForObjectType(player, objectType, slotObject),
                true,
                false
            ) as EquipmentSlot;

            if (equipmentSlot != null) {
                equipmentSlot.inventoryIndexReference = index;
                equipmentSlot.OnPickUp(player, false);
                equipmentSlot.OnOccupied();
            }
            else {
                Debug.LogError("could not allocate equipment slot");
            }
            return equipmentSlot!;
        }

        [HarmonyPatch(nameof(global::PlayerController.GetEquippedSlot))]
        [HarmonyPrefix]
        static bool GetEquippedSlot(global::PlayerController __instance, ref EquipmentSlot? __result) {
            if (__instance.equippedSlotIndex < global::PlayerController.MAX_EQUIPMENT_SLOTS) {
                return true;
            }

            if (!__instance.IsAnySlotEquipped()) {
                __result = null;
                return false;
            }

            if (_slot == null) {
                Debug.LogError("The slot is not managed but the player reports that it holds a slot equipped");
                return true;
            }

            if (_slot.inventoryIndexReference != __instance.equippedSlotIndex) {
                Debug.LogError("The slot is managed but the player holds a different item than tracked");
                return true;
            }

            __result = _slot;
            return false;
        }

        [HarmonyPatch(nameof(global::PlayerController.GetHeldObject))]
        [HarmonyPrefix]
        static bool GetHeldObject(global::PlayerController __instance, ref ObjectDataCD __result) {
            if (__instance.equippedSlotIndex < global::PlayerController.MAX_EQUIPMENT_SLOTS || _slot == null) {
                return true;
            }
            if (_slot.inventoryIndexReference != __instance.equippedSlotIndex) {
                Debug.LogError("The slot is managed but the player holds a different item than tracked");
                return true;
            }

            __result = __instance.playerInventoryHandler.GetObjectData(__instance.equippedSlotIndex);
            return false;
        }

        [HarmonyPatch(nameof(global::PlayerController.UpdateEquippedSlotVisuals))]
        [HarmonyPrefix]
        static void UpdateEquippedSlotVisuals_Prefix(global::PlayerController __instance, out int? __state) {
            if (__instance.equippedSlotIndex < global::PlayerController.MAX_EQUIPMENT_SLOTS
                || __instance.entityExist is false
                || _slot == null
                || __instance.isLocal is false
            ) {
                __state = null;
                return;
            }

            var currentEquipment = EntityUtility.GetComponentData<EquippedObjectCD>(__instance.entity, __instance.world);
            //#!#!#!#! IMPORTANT: For completion, rollback the data after
            __state = currentEquipment.equippedSlotIndex;
            currentEquipment.equippedSlotIndex = __instance.equippedSlotIndex;
            EntityUtility.SetComponentData(__instance.entity, __instance.world, currentEquipment);
            __instance.isLocal = false;

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


            if ((int)__instance.clientInput.equippedSlotIndex != __instance.equippedSlotIndex
                || __instance.visuallyEquippedContainedObject.objectID != objectID
                || __instance.visuallyEquippedContainedObject.variation != variation
                || __instance.visuallyEquippedContainedObject.amount != amount
                || __instance.visuallyEquippedContainedObject.auxDataIndex != auxDataIndex
            ) {
                __instance.clientInput.equippedSlotIndex = (byte)__instance.equippedSlotIndex;
                _slot.OnEquip(__instance);

                Debug.Log(objectID);
                Debug.Log(variation);
                Debug.Log(amount);
                Debug.Log(auxDataIndex);
            }
        }


        [HarmonyPatch(nameof(global::PlayerController.UpdateEquippedSlotVisuals))]
        [HarmonyPostfix]
        static void UpdateEquippedSlotVisuals_Postfix(global::PlayerController __instance, int? __state) {
            if (__state == null) {
                return;
            }

            var currentEquipment = EntityUtility.GetComponentData<EquippedObjectCD>(__instance.entity, __instance.world);
            currentEquipment.equippedSlotIndex = __state.Value;
            EntityUtility.SetComponentData(__instance.entity, __instance.world, currentEquipment);
            __instance.isLocal = true;
        }


        [HarmonyPatch(nameof(GetSlotTypeForObjectType))]
        [HarmonyReversePatch]
        static private Type GetSlotTypeForObjectType(global::PlayerController __instance, ObjectType objectType, ObjectDataCD objectData) => throw new NotImplementedException();
    }
}
