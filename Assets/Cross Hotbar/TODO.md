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

[] Item slot that tracks if item is in inventory, and if it is it shows it
	* Handle the mouse over, add error border and remove selection on click
	* Instantiate slot at last hotbar position + 3 * pixelSize (0,0625)
	* Move all SpriteRenderer's sort order by a 100 (or at least by 10)
		- If a 2nd mod is gonna require the same position, think about making a stack framework for the inventory
	* Add category into controlls
		* Add Hotbar shortcut ('s?)
		* Add Hotbar selection keys (What if someone wants to use a slot as is?)

	
	 
Needed fixes:
	Items selected from the quick torch, swap the items in the inventory, Abut no visual update is made on the local user
	Slot error on death

Research:
	Singleton Components as system state?
