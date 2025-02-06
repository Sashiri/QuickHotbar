using System;
using UnityEngine;

namespace CrossHotbar.InventoryObjectSlot {
    [Serializable]
    public struct TrackingOptions {
        public TrackingOptions(int? variation = null) {
            Version = 1;
            Variation = variation;
        }

        public int Version { get; init; }
        public int? Variation { get; init; }
    }

    [Serializable]
    public struct InventoryObjectTracker {
        public InventoryObjectTracker(ObjectID objectID, TrackingOptions tracking) {
            Version = 1;
            ObjectID = objectID;
            Tracking = tracking;
        }

        public static InventoryObjectTracker FromObjectData(ObjectData objectData) {
            return new(
                objectID: objectData.objectID,
                new(variation: objectData.variation)
            );
        }

        public int Version { get; init; }
        public ObjectID ObjectID { get; init; }
        public TrackingOptions Tracking { get; init; }
    }
}
