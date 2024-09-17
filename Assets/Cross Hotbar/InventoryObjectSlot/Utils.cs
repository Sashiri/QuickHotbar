using System;
using PugMod;
using UnityEngine;

namespace CrossHotbar.InventoryObjectSlot {

    static class UnityPrefabHelper {
        internal static void ApplyPrefab<TTarget, TSource>(this TTarget target, TSource source)
                where TTarget : MonoBehaviour, TSource
                where TSource : MonoBehaviour {

            if (target == null) {
                throw new ArgumentNullException(nameof(target));
            }
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            ApplyPrefab(typeof(TSource), target, source);
        }

        private static void ApplyPrefab(Type T, object target, object source) {
            if (T.BaseType != typeof(MonoBehaviour)) {
                ApplyPrefab(T.BaseType, target, source);
            }
            foreach (var member in T.GetMembersChecked()) {
                try {
                    API.Reflection.SetValue(member, target, API.Reflection.GetValue(member, source));
                }

                catch (Exception e) when (
                    e is InvalidOperationException
                    or FieldAccessException
                    || e.GetType().FullName == "System.Reflection.TargetInvocationException"
                ) { }
            }
        }
    }
}