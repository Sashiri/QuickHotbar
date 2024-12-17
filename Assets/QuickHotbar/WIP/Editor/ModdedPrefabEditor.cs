#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(UIelement), true)]
public class ModdedPrefabEditor : Editor {

    private ReorderableList dialogItemsList;
    private Func<string, bool> isModified;
    private IList<SerializedProperty> visibleProperties;

    private void OnEnable() {
        visibleProperties = serializedObject.Properties().ToList();
        var propertyPrefabModdedInfo = visibleProperties.FirstOrDefault(v => v.type == typeof(ModdedPrefab).FullName);

        if (propertyPrefabModdedInfo is null) {
            return;
        }

        var propertyModInfoClone = propertyPrefabModdedInfo.FindPropertyRelative(nameof(ModdedPrefab.propertiesToClone));
        if (propertyModInfoClone is null) {
            return;
        }


        isModified = (path) => {
            var item = propertyModInfoClone
                .GetEnumerator()
                .OfType<SerializedProperty>();

            while (item.MoveNext()) {
                if (item.Current.stringValue == path) {
                    return true;
                }
            }
            return false;
        };

        var properties = serializedObject.Properties().ToList();

        dialogItemsList = new ReorderableList(serializedObject, propertyModInfoClone) {
            displayAdd = true,
            displayRemove = true,
            draggable = true,

            drawHeaderCallback = rect => EditorGUI.LabelField(rect, propertyPrefabModdedInfo.displayName),
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = propertyModInfoClone.GetArrayElementAtIndex(index);
                EditorGUI.SelectableLabel(rect, serializedObject.FindProperty(element.stringValue).displayName);
            },
            elementHeightCallback = index => EditorGUIUtility.singleLineHeight,
            onAddDropdownCallback = (rect, list) => {
                GenericMenu dropdownMenu = new();

                // Add dropdown options
                foreach (var property in properties.Where(p => !isModified(p.propertyPath))) {
                    var path = property.propertyPath;
                    dropdownMenu.AddItem(new GUIContent(property.displayName), false, () => {
                        list.serializedProperty.arraySize++;
                        var addedElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                        addedElement.stringValue = path;
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                dropdownMenu.ShowAsContext();
            },
        };
    }

    public override void OnInspectorGUI() {
        if (dialogItemsList is null) {
            DrawDefaultInspector();
            return;
        }

        var iterator = visibleProperties.GetEnumerator();
        if (iterator.MoveNext()) {
            RenderPropertyField(iterator.Current);
            EditorGUILayout.Space();

            dialogItemsList.DoLayoutList();
            EditorGUILayout.Space();


            while (iterator.MoveNext()) {
                if (isModified(iterator.Current.propertyPath)) {
                    continue;
                }
                RenderPropertyField(iterator.Current);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    private void RenderPropertyField(SerializedProperty property) {
        bool isDisabled = property.propertyPath == "m_Script";
        EditorGUI.BeginDisabledGroup(isDisabled);
        _ = EditorGUILayout.PropertyField(property, true);
        EditorGUI.EndDisabledGroup();
    }

}

#endif