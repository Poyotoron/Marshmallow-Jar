using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.IrodoriColorizer.Editor
{
    internal sealed class IrodoriFilterPopup : EditorWindow
    {
        private const float PopupWidth = 260f;
        private const float MaxPopupHeight = 420f;
        private const float Padding = 8f;
        private const float HeaderHeight = 18f;
        private const float RowHeight = 20f;
        private const float FooterHeight = 36f;
        private static readonly GUIContent TitleContent = new GUIContent("色で絞り込む");
        private static readonly GUIContent ClearContent = new GUIContent("すべて解除");
        private static readonly GUIContent EmptyContent = new GUIContent(
            "有効なプリセットがありません。設定からプリセットを有効にしてください。");
        private IReadOnlyList<IrodoriLabelSection> _sections;
        private Dictionary<string, GUIContent> _rowContents;
        private GUIContent[] _sectionContents;
        private Vector2 _scroll;
        private float _contentHeight;

        internal static void Open()
        {
            IrodoriFilterPopup window = CreateInstance<IrodoriFilterPopup>();
            window.titleContent = TitleContent;
            window.Prepare();
            Vector2 point = IrodoriDrawer.ContextScreenPosition;
            Rect anchor = new Rect(point.x, point.y, 1f, 1f);
            window.ShowAsDropDown(anchor, new Vector2(PopupWidth, Mathf.Min(window._contentHeight + FooterHeight, MaxPopupHeight)));
        }

        private void Prepare()
        {
            _sections = IrodoriLabelResolver.GetVisibleSections();
            _rowContents = new Dictionary<string, GUIContent>();
            _sectionContents = new GUIContent[_sections.Count];
            _contentHeight = Padding + HeaderHeight + Padding;
            if (_sections.Count == 0)
            {
                _contentHeight += 42f;
            }

            for (int i = 0; i < _sections.Count; i++)
            {
                IrodoriLabelSection section = _sections[i];
                _sectionContents[i] = new GUIContent(section.Title);
                _contentHeight += 4f + HeaderHeight + section.Labels.Length * RowHeight;
                for (int j = 0; j < section.Labels.Length; j++)
                {
                    IrodoriLabel label = section.Labels[j];
                    _rowContents[label.Id] = new GUIContent(label.Name);
                }
            }
        }

        private void OnGUI()
        {
            // NOTE: 再コンパイルで表示用のキャッシュが失われた場合だけ、有効なラベルから作り直す。
            if (_sections == null)
            {
                Prepare();
            }

            float scrollHeight = position.height - FooterHeight;
            float width = position.width - (_contentHeight > scrollHeight ? 16f : 0f);
            Rect scrollRect = new Rect(0f, 0f, position.width, scrollHeight);
            Rect contentRect = new Rect(0f, 0f, width, _contentHeight);
            _scroll = GUI.BeginScrollView(scrollRect, _scroll, contentRect);
            float y = Padding;
            GUI.Label(new Rect(Padding, y, width - Padding * 2f, HeaderHeight), TitleContent, EditorStyles.boldLabel);
            y += HeaderHeight + Padding;
            if (_sections.Count == 0)
            {
                GUI.Label(new Rect(Padding, y, width - Padding * 2f, 42f), EmptyContent, EditorStyles.wordWrappedMiniLabel);
            }

            for (int i = 0; i < _sections.Count; i++)
            {
                IrodoriLabelSection section = _sections[i];
                y += 4f;
                GUI.Label(new Rect(Padding, y, width - Padding * 2f, HeaderHeight), _sectionContents[i], EditorStyles.miniBoldLabel);
                y += HeaderHeight;
                for (int j = 0; j < section.Labels.Length; j++)
                {
                    IrodoriLabel label = section.Labels[j];
                    Rect rowRect = new Rect(Padding, y, width - Padding * 2f - 20f, RowHeight);
                    bool wasSelected = IrodoriFilter.IsSelected(label.Id);
                    bool selected = EditorGUI.ToggleLeft(rowRect, _rowContents[label.Id], wasSelected);
                    if (selected != wasSelected)
                    {
                        IrodoriFilter.SetSelected(label.Id, selected);
                    }

                    EditorGUI.DrawRect(new Rect(width - Padding - 12f, y + 4f, 12f, 12f), label.Color);
                    y += RowHeight;
                }
            }

            GUI.EndScrollView();
            EditorGUI.BeginDisabledGroup(!IrodoriFilter.IsActive);
            if (GUI.Button(new Rect(Padding, scrollHeight + 6f, position.width - Padding * 2f, 22f), ClearContent))
            {
                IrodoriFilter.Clear();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
