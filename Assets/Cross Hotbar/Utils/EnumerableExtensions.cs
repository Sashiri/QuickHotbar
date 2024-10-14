using System;
using System.Collections.Generic;

#nullable enable

namespace CrossHotbar.InventoryObjectSlotBar {
    static class EnumerableExtensions {
        public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> source) {
            var i = 0;
            foreach (var item in source) {
                yield return (i++, item);
            }
        }

        public static IEnumerable<T> SortedTakeBy<T>(this IEnumerable<T> enumerable, Func<T, float> keySelector, int n) {
            var priority = new List<float>(n);
            var queue = new List<T>(n);
            var enumerator = enumerable.GetEnumerator();
            while (queue.Count < n && enumerator.MoveNext()) {
                var key = keySelector(enumerator.Current);
                var index = priority.BinarySearch(key);
                index = index < 0 ? ~index : priority.LastIndexOf(priority[index], index);
                priority.Insert(index, key);
                queue.Insert(index, enumerator.Current);
            }
            while (enumerator.MoveNext()) {
                var key = keySelector(enumerator.Current);
                var index = priority.BinarySearch(key);
                index = index < 0 ? ~index : priority.LastIndexOf(priority[index], index);
                if (index >= n) {
                    continue;
                }

                priority.RemoveAt(n - 1);
                queue.RemoveAt(n - 1);

                priority.Insert(index, key);
                queue.Insert(index, enumerator.Current);
            }

            return queue;
        }
    }
}
