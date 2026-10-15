using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    [InitializeOnLoad]
    internal static class HierarchyToggleDrawer
    {
        private const float ToggleSize = 16f;
        private const string EditorOnlyTag = "EditorOnly";
        private const string UntaggedTag = "Untagged";

        private static readonly GUIContent ActiveContent = new GUIContent(string.Empty, "表示（Active）");
        private static readonly GUIContent EditorOnlyContent = new GUIContent(string.Empty, "EditorOnly（アップロードに含めない）");
        private static readonly GUIContent LockedTagContent =
            new GUIContent(string.Empty, "独自のタグが付いているため EditorOnly を切り替えられません");

        static HierarchyToggleDrawer()
        {
            // NOTE: 色付け機能も同じ行に塗る。塗りの上にチェックボックスを重ねるため、
            //       ほかの [InitializeOnLoad] が購読し終えたあとで購読して、描画を最後に回す。
            EditorApplication.delayCall += Subscribe;
        }

        private static void Subscribe()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyItem;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItem;
        }

        private static void OnHierarchyItem(int instanceID, Rect selectionRect)
        {
            if (!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.HierarchyToggles))
            {
                return;
            }

            MarshmallowJarSettings settings = MarshmallowJarSettings.instance;
            float rightOffset = Mathf.Clamp(settings.hierarchyToggleRightOffset, 0f,
                MarshmallowJarSettings.MaxHierarchyToggleRightOffset);
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null)
            {
                return;
            }

            float y = selectionRect.y + (selectionRect.height - ToggleSize) * 0.5f;
            Rect editorOnlyRect = new Rect(selectionRect.xMax - rightOffset - ToggleSize, y, ToggleSize, ToggleSize);
            Rect activeRect = new Rect(editorOnlyRect.x - ToggleSize, y, ToggleSize, ToggleSize);

            EditorGUI.BeginChangeCheck();
            bool active = GUI.Toggle(activeRect, go.activeSelf, ActiveContent);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyActive(go, active);
            }

            bool isEditorOnly = go.CompareTag(EditorOnlyTag);
            bool editable = isEditorOnly || go.CompareTag(UntaggedTag);
            EditorGUI.BeginDisabledGroup(!editable);
            EditorGUI.BeginChangeCheck();
            bool editorOnly = GUI.Toggle(editorOnlyRect, isEditorOnly, editable ? EditorOnlyContent : LockedTagContent);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyEditorOnly(go, editorOnly);
            }
            EditorGUI.EndDisabledGroup();
        }

        private static GameObject[] CollectTargets(GameObject clicked)
        {
            if (Selection.Contains(clicked))
            {
                GameObject[] selected = Selection.gameObjects;
                if (selected.Length > 1)
                {
                    return selected;
                }
            }

            return new[] { clicked };
        }

        private static void ApplyActive(GameObject clicked, bool value)
        {
            GameObject[] targets = CollectTargets(clicked);
            Undo.RecordObjects(targets, "表示の切り替え");
            foreach (GameObject target in targets)
            {
                target.SetActive(value);
                if (PrefabUtility.IsPartOfPrefabInstance(target))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                }
            }
        }

        private static void ApplyEditorOnly(GameObject clicked, bool value)
        {
            List<GameObject> editableTargets = new List<GameObject>();
            foreach (GameObject target in CollectTargets(clicked))
            {
                if (target.CompareTag(EditorOnlyTag) || target.CompareTag(UntaggedTag))
                {
                    editableTargets.Add(target);
                }
            }

            if (editableTargets.Count == 0)
            {
                return;
            }

            GameObject[] targets = editableTargets.ToArray();
            Undo.RecordObjects(targets, "EditorOnly の切り替え");
            foreach (GameObject target in targets)
            {
                target.tag = value ? EditorOnlyTag : UntaggedTag;
                if (PrefabUtility.IsPartOfPrefabInstance(target))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                }
            }
        }
    }
}
