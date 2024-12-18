#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;

static class SerializedObjectExtension {
    internal static IEnumerator<SerializedProperty> Properties(this SerializedObject serialized, bool enterChildren = false) {
        SerializedProperty property = serialized.GetIterator();
        if (!property.NextVisible(true)) {
            yield break;
        }

        do {
            yield return property.Copy();
        } while (property.NextVisible(enterChildren));
    }
}

#endif
