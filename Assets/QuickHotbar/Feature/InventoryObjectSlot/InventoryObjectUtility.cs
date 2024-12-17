using Unity.Entities;

internal static class InventoryObjectUtility {
    internal record TrackingPreference(int Variation);


    /// <summary>
    /// Finds the first object in the item buffer with preference for tracking parameters
    /// Code based on <see cref="Inventory.InventoryUtility.FindFirstOccurenceOfObject"/>
    /// </summary>
    /// <param name="objectID"></param>
    /// <param name="containedObjectsBuffers"></param>
    /// <param name="databaseBankCD"></param>
    /// <param name="excludeIndex"></param>
    /// <returns></returns>
    internal static int FindFirstOccurenceOfTrackedObject(ObjectID objectID, TrackingPreference preference, DynamicBuffer<ContainedObjectsBuffer> containedObjectsBuffers, PugDatabase.DatabaseBankCD databaseBankCD, int excludeIndex = -1) {
        const int NOT_FOUND = -1;
        var firstOccurenceIndex = NOT_FOUND;

        // For one preference searh we dont need to store match score, return first full match or first ever found
        for (int i = 0; i < containedObjectsBuffers.Length; i++) {
            if (containedObjectsBuffers[i].objectID == ObjectID.None || i == excludeIndex) {
                continue;
            }

            var item = containedObjectsBuffers[i];

            if (item.objectID != objectID) {
                continue;
            }

            // We dont have any preference, any match is good
            if (preference.Variation == 0) {
                return i;
            }

            // Full match
            if (item.variation == preference.Variation) {
                return i;
            }

            // Not a full match, save first occurence
            if (firstOccurenceIndex == NOT_FOUND) {
                firstOccurenceIndex = i;
            }
        }

        return firstOccurenceIndex;
    }

}