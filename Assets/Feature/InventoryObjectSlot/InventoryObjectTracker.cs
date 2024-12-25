using System;
using UnityEngine;

namespace CrossHotbar.InventoryObjectSlot {
    [Serializable]
    internal struct TrackingOptions : ISerializationCallbackReceiver {
        public TrackingOptions(int? variation = null) {
            Version = 1;
            Variation = variation;
        }

        public int Version { get; private set; }
        public int? Variation { get; private set; }

        public readonly void OnAfterDeserialize() {
        }

        public readonly void OnBeforeSerialize() {
        }
    }

    [Serializable]
    internal struct InventoryObjectTracker : ISerializationCallbackReceiver {
        public InventoryObjectTracker(ObjectID objectID, TrackingOptions tracking) {
            Version = 1;
            ObjectID = objectID;
            Tracking = tracking;
        }

        public int Version { get; private set; }
        public ObjectID ObjectID { get; private set; }
        public TrackingOptions Tracking { get; private set; }

        public readonly void OnAfterDeserialize() {
        }

        public readonly void OnBeforeSerialize() {
        }
    }
}
