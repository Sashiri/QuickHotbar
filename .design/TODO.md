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

[X] Item slot that tracks if item is in inventory, and if it is it shows it
	* Handle the mouse over, add error border and remove selection on click
	* Instantiate slot at last hotbar position + 3 * pixelSize (0,0625)
	* Move all SpriteRenderer's sort order by a 100 (or at least by 10)
		- If a 2nd mod is gonna require the same position, think about making a stack framework for the inventory
	* Add category into controlls
		* Add Hotbar shortcut ('s?)
		* Add Hotbar selection keys (What if someone wants to use a slot as is?)

[] Prefab Bisection research:
	* Clone selected properties of monos
	* Instantiate sub objects on the relative path to root object
	When does it make sense to select sub paths to mix prefabs, we mostly want to havr the "native" experience of the game, that includes the assets like shadows, borders etc.
	It would be great to depend only on the base assets, but it really makes me annoyed to have the composition in the code level.
	Unfortunately it's not extendable and also creates useless wrecks of objects in the hierarchy, as the hide flags dont have any effect in runtime.
	Using the editor has an advantage of seeing all visible properties that are used to make the prefab, we dont have to set private fields that potentialy should not be touched.

