using System;
using System.Linq;
using PugMod;
using UnityEngine;

[Serializable]
struct ModdedPrefab {
    public string[] propertiesToClone;

    public readonly void Apply(object target, object source) {
        var targetType = target.GetType();
        var sourceType = source.GetType();
        var targetMembers = targetType.GetMembersChecked();
        var sourceMembers = targetType.GetMembersChecked();

        if (!targetType.IsInstanceOfType(source)) {
            throw new InvalidOperationException($"{nameof(target)} is not a subtype of {nameof(source)}");
        }

        foreach (var property in propertiesToClone) {
            var targetProperty = targetMembers.FirstOrDefault(m => m.GetNameChecked() == property);
            if (targetProperty is null) {
                Debug.LogWarning($"{property} could not be found on target of type {targetType}");
                continue;
            }

            var sourceProperty = sourceMembers.FirstOrDefault(m => m.GetNameChecked() == property);
            if (sourceProperty is null) {
                Debug.LogWarning($"{property} could not be found on target of type {targetType}");
                continue;
            }

            if (targetProperty.GetDeclaringTypeChecked() != sourceProperty.GetDeclaringTypeChecked()) {
                //Maybe it should be an exception instead
                Debug.LogWarning($"{nameof(target)} and {nameof(source)} dont share member {property}");
                continue;
            }

            API.Reflection.SetValue(targetProperty, target, API.Reflection.GetValue(sourceProperty, source));
        }
    }
}