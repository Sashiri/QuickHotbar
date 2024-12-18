using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;

static class IEnumeratorExtension {
    internal static object FirstOrDefault(this IEnumerator enumerator, Func<object, bool> predicate) {
        while (enumerator.MoveNext()) {
            if (predicate(enumerator.Current)) {
                return enumerator.Current;
            }
        }
        return null;
    }

    internal static T FirstOrDefault<T>(this IEnumerator<T> enumerator, Func<T, bool> predicate)
        where T : class {
        return FirstOrDefault((IEnumerator)enumerator, (value) => {
            if (value is not T cast) {
                throw new System.Diagnostics.UnreachableException("This condition should never be reached, as we pass both instance and function");
            }
            return predicate(cast);
        }) as T;
    }

    internal static IEnumerator<T> OfType<T>(this IEnumerator enumerator) {
        while (enumerator.MoveNext()) {
            if (enumerator.Current is not T value) {
                continue;
            }
            yield return value;
        }
    }

    internal static T Configure<T, C>(this T instance, Action<C> configuration)
        where T : IEnumerator {
        if (instance is not C cast) {
            throw new InvalidOperationException($"{nameof(configuration)} expects different type of enumerator, Expected {typeof(C)}, got {instance.GetType()}");
        }
        configuration(cast);
        return instance;
    }

    internal static IList<T> ToList<T>(this IEnumerator<T> enumerable) {
        List<T> list = new();
        while (enumerable.MoveNext()) {
            list.Add(enumerable.Current);
        }
        return list;
    }
}

static class GenericExtension {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Apply<T>(this T self, Action<T> action) {
        action(self);
        return self;
    }
}

public class ClonePropertiesVisitor<T> : PropertyVisitor {
    private readonly T target;

    public ClonePropertiesVisitor(T target) {
        this.target = target;
    }

    protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value) {
        if (target is TContainer) {
            PropertyContainer.SetValue(target, property.Name, value);
        }
    }
}
