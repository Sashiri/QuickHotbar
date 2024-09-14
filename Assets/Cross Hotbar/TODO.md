[X] Show UI
[X] Show Inventory Space
[X] Allow equiping items from inventory
	Patches:
	 - GetEquippedSlot
		Breaks the array access of equipmentSlots
	 - GetHeldObject
		Asserts hotbar range of 0 - 10, otherwise logic is good
	 - IsAnySlotEquipped
		Breaks the array access of equipmentSlots, depends on the list of equipment slots
	 - UpdateEquipmentSlot
		Range is asserted, Breaks the array access of equipmentSlots
		- OnEquipmentSlotUpdated called by UpdateEquipmentSlot, will break hotbar ui with range access
		- OnEquipmentSlotActivated
		!! After evaluation, it seems that there's no reason to modify this code, we dont wanna recreate equipment slot after creation, we want to track objectId afterall and enable it if it's in the inventory
	 - UpdateEquippedSlotVisuals
		OK;
		NVM, access to equipmentSlots
	 - IsEquippedSlotButtonDown
		OK; Will not work for the default bindings
	 - UpdateInventoryStuff
		OK
	 
