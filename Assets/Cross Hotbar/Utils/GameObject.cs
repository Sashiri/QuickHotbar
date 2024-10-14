using System;
using System.Runtime.CompilerServices;
using UnityEngine;

internal static class GameObjectExtension {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static GameObject ConfigureComponent<T>(this GameObject gameObject, Action<T> configuration) where T : Component {
        return gameObject.Apply(go => configuration(go.AddComponent<T>()));
    }
}