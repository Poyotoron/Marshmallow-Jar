using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    internal sealed class BlendShapeSliderWindow : EditorWindow
    {
        private const string MenuPath = "Tools/" + MarshmallowJarInfo.DisplayName + "/BlendShape Sliders";
        private const string ClampWarning = "Player Settings の Clamp BlendShapes が有効なため、0〜100 の外の値は反映されません。";
        private const string NoTargetMessage = "SkinnedMeshRenderer を持つオブジェクトを選んでください。";
        private const string NoShapesMessage = "このメッシュにはブレンドシェイプがありません。";
        private static readonly GUIContent LockContent = new GUIContent(
            "固定", "オンにすると、ほかのオブジェクトを選んでも対象が変わりません。");
        private static readonly GUIContent NonZeroContent = new GUIContent(
            "0 以外", "値が 0 でないブレンドシェイプだけを表示します。");
        private static readonly GUIContent MinContent = new GUIContent("下限");
        private static readonly GUIContent MaxContent = new GUIContent("上限");
        private static readonly GUIContent ResetRangeContent = new GUIContent("0〜100", "範囲を 0〜100 に戻します。");
        private static readonly GUIContent NoMatchContent = new GUIContent("該当するブレンドシェイプがありません。");
        private static readonly GUILayoutOption[] SearchOptions = { GUILayout.MinWidth(80f) };
        private static readonly GUILayoutOption[] ResetOptions = { GUILayout.ExpandWidth(false) };

        [SerializeField] private SkinnedMeshRenderer _pinned;
        [SerializeField] private bool _locked;
        [SerializeField] private string _search = string.Empty;
        [SerializeField] private bool _nonZeroOnly;
        private Vector2 _scroll;
        private SkinnedMeshRenderer _cachedRenderer;
        private Mesh _cachedMesh;
        private int _cachedCount;
        private GUIContent[] _nameContents;
        private string[] _names;
        private SerializedObject _serialized;
        private SerializedProperty _weights;
        private string _cachedSearch;
        private readonly List<int> _searchHits = new List<int>();

        [MenuItem(MenuPath, false, 1050)]
        private static void OpenFromTools()
        {
            Open(null);
        }

        [MenuItem("CONTEXT/SkinnedMeshRenderer/BlendShape Sliders")]
        private static void OpenFromComponent(MenuCommand command)
        {
            Open(command.context as SkinnedMeshRenderer);
        }

        internal static void Open(SkinnedMeshRenderer pinned)
        {
            BlendShapeSliderWindow window = GetWindow<BlendShapeSliderWindow>(false, "BlendShape Sliders", true);
            window.minSize = new Vector2(320f, 240f);
            if (pinned != null)
            {
                window._pinned = pinned;
                window._locked = true;
            }

            window.Repaint();
        }

        private SkinnedMeshRenderer ResolveTarget()
        {
            if (_locked && _pinned != null)
            {
                return _pinned;
            }

            if (_locked)
            {
                _locked = false;
            }

            GameObject selected = Selection.activeGameObject;
            return selected != null ? selected.GetComponent<SkinnedMeshRenderer>() : null;
        }

        private void EnsureCache(SkinnedMeshRenderer renderer)
        {
            Mesh mesh = renderer != null ? renderer.sharedMesh : null;
            int count = mesh != null ? mesh.blendShapeCount : 0;
            if (_cachedRenderer == null || renderer != _cachedRenderer || mesh != _cachedMesh || count != _cachedCount)
            {
                ReleaseCache();
                _cachedRenderer = renderer;
                _cachedMesh = mesh;
                _cachedCount = count;
                _names = new string[count];
                _nameContents = new GUIContent[count];
                for (int i = 0; i < count; i++)
                {
                    _names[i] = mesh.GetBlendShapeName(i);
                    _nameContents[i] = new GUIContent(_names[i]);
                }

                if (renderer != null)
                {
                    _serialized = new SerializedObject(renderer);
                    _weights = _serialized.FindProperty("m_BlendShapeWeights");
                }
            }

            if (_search != _cachedSearch)
            {
                _searchHits.Clear();
                for (int i = 0; i < count; i++)
                {
                    if (string.IsNullOrEmpty(_search) || _names[i].IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _searchHits.Add(i);
                    }
                }

                _cachedSearch = _search;
            }
        }

        private void ReleaseCache()
        {
            if (_serialized != null)
            {
                _serialized.Dispose();
            }

            _serialized = null;
            _weights = null;
            _cachedRenderer = null;
            _cachedMesh = null;
            _cachedCount = 0;
            _cachedSearch = null;
            _searchHits.Clear();
        }

        private void OnGUI()
        {
            SkinnedMeshRenderer target = ResolveTarget();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            bool locked = GUILayout.Toggle(_locked, LockContent, EditorStyles.toolbarButton);
            if (locked != _locked)
            {
                _locked = locked;
                _pinned = locked ? target : null;
            }

            SkinnedMeshRenderer chosen = EditorGUILayout.ObjectField(target, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
            if (chosen != target)
            {
                _pinned = chosen;
                _locked = chosen != null;
            }

            _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField, SearchOptions);
            _nonZeroOnly = GUILayout.Toggle(_nonZeroOnly, NonZeroContent, EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();

            MarshmallowJarSettings settings = MarshmallowJarSettings.instance;
            DrawRangeFields(settings);
            if (PlayerSettings.legacyClampBlendShapeWeights &&
                (settings.blendShapeSliderMin < 0f || settings.blendShapeSliderMax > 100f))
            {
                EditorGUILayout.HelpBox(ClampWarning, MessageType.Info);
            }

            target = ResolveTarget();
            if (target == null)
            {
                if (_serialized != null)
                {
                    ReleaseCache();
                }

                EditorGUILayout.HelpBox(NoTargetMessage, MessageType.Info);
                return;
            }

            EnsureCache(target);
            if (_cachedMesh == null || _cachedCount == 0)
            {
                EditorGUILayout.HelpBox(NoShapesMessage, MessageType.Info);
                return;
            }

            _serialized.Update();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Mathf.Max(120f, position.width * 0.45f);
            bool drewRow = false;
            for (int hit = 0; hit < _searchHits.Count; hit++)
            {
                int i = _searchHits[hit];
                if (i < _weights.arraySize)
                {
                    SerializedProperty weight = _weights.GetArrayElementAtIndex(i);
                    if (_nonZeroOnly && Mathf.Approximately(weight.floatValue, 0f))
                    {
                        continue;
                    }

                    EditorGUILayout.Slider(weight, settings.blendShapeSliderMin, settings.blendShapeSliderMax, _nameContents[i]);
                }
                else
                {
                    if (_nonZeroOnly)
                    {
                        continue;
                    }

                    // NOTE: 未編集の配列はメッシュより短いことがあるため、触れるまでは値 0 として表示する。
                    EditorGUI.BeginChangeCheck();
                    float value = EditorGUILayout.Slider(_nameContents[i], 0f, settings.blendShapeSliderMin, settings.blendShapeSliderMax);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _weights.arraySize = _cachedCount;
                        _weights.GetArrayElementAtIndex(i).floatValue = value;
                    }
                }

                drewRow = true;
            }

            if (!drewRow)
            {
                EditorGUILayout.LabelField(NoMatchContent);
            }

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUILayout.EndScrollView();
            // NOTE: シリアライズされた値として編集すると Undo、Prefab の上書き、Animation の記録に対応できる。
            _serialized.ApplyModifiedProperties();
        }

        internal static void DrawRangeFields(MarshmallowJarSettings settings)
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 36f;
            EditorGUILayout.BeginHorizontal();
            // NOTE: 入力中の途中の値で確定すると、幅を保つ処理がもう一方の値を書き換えてしまうため、確定を待つ欄にする。
            float min = EditorGUILayout.DelayedFloatField(MinContent, settings.blendShapeSliderMin);
            float max = EditorGUILayout.DelayedFloatField(MaxContent, settings.blendShapeSliderMax);
            bool reset = GUILayout.Button(ResetRangeContent, ResetOptions);
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = labelWidth;

            bool changed = reset || min != settings.blendShapeSliderMin || max != settings.blendShapeSliderMax;
            if (reset)
            {
                settings.SetBlendShapeSliderRange(MarshmallowJarSettings.DefaultBlendShapeSliderMin,
                    MarshmallowJarSettings.DefaultBlendShapeSliderMax, true);
            }
            else if (min != settings.blendShapeSliderMin)
            {
                settings.SetBlendShapeSliderRange(min, max, true);
            }
            else if (max != settings.blendShapeSliderMax)
            {
                settings.SetBlendShapeSliderRange(min, max, false);
            }

            if (changed)
            {
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += Repaint;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;
            ReleaseCache();
        }

        private void OnSelectionChange()
        {
            if (!_locked)
            {
                Repaint();
            }
        }

        private void OnHierarchyChange()
        {
            Repaint();
        }

        private void OnFocus()
        {
            Repaint();
        }
    }
}
