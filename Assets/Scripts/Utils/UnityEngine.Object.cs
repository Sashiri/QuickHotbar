using System;
using UnityEngine;

internal static class ObjectExtension {
    public static GameObject InstantiateWith<TSource>(TSource original, Action<TSource> action)
    where TSource : UnityEngine.Object {
        var goOriginal = original.GetGameObject();

        var active = goOriginal.activeSelf;

        goOriginal.SetActive(false);
        var copy = UnityEngine.Object.Instantiate(original);
        goOriginal.SetActive(active);

        var goCopy = copy.GetGameObject();

        action(copy);

        goCopy.SetActive(active);
        return goCopy;
    }

    private static GameObject GetGameObject(this UnityEngine.Object o) => o switch {
        GameObject go => go,
        Component c => c.gameObject,
        _ => throw new NotImplementedException(),
    };
}