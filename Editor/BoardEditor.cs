using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Nomlas.CompactWorldBoard.Editor
{
    public abstract class BoardEditor : UnityEditor.Editor
    {
        private protected abstract void BuildBoardSettings(VisualElement parent);

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            BoardUI.ApplyTheme(root);
            root.AddToClassList("cwb-inspector");
            var header = BoardUI.Row();
            header.AddToClassList("cwb-wrap");
            var title = BoardUI.Text("Compact World Board", "cwb-section-title");
            title.AddToClassList("cwb-grow");
            header.Add(title);
            header.Add(BoardUI.VersionBadge());
            root.Add(header);
            BuildBoardSettings(root);

            var portal = BoardUI.Section(root, "ポータル");
            AddProperty(portal, "canEnter", "入場を許可（通常オフ）");
            AddProperty(portal, "createDelaySecondsMin", "最小遅延（秒）");
            AddProperty(portal, "createDelaySecondsMax", "最大遅延（秒）");
            var warning = new BoardStatusLine("遅延は0以上、最大値は最小値以上に設定してください。") { messageType = BoardMessageKind.Warning };
            portal.Add(warning);
            void ValidateDelay()
            {
                var min = serializedObject.FindProperty("createDelaySecondsMin").intValue;
                var max = serializedObject.FindProperty("createDelaySecondsMax").intValue;
                warning.style.display = min < 0 || max < min ? DisplayStyle.Flex : DisplayStyle.None;
            }
            ValidateDelay();
            root.TrackPropertyValue(serializedObject.FindProperty("createDelaySecondsMin"), _ => ValidateDelay());
            root.TrackPropertyValue(serializedObject.FindProperty("createDelaySecondsMax"), _ => ValidateDelay());

            var references = BoardUI.Section(root, "参照");
            AddProperty(references, "portalMarkerPrefab", "ポータルPrefab");
            AddProperty(references, "portalParent", "ポータルの親");
            AddProperty(references, "content", "一覧テンプレート");
            AddProperty(references, "contentParent", "一覧の親");
            return root;
        }

        private protected void AddProperty(VisualElement parent, string name, string label)
        {
            BoardSettingsUI.AddProperty(parent, serializedObject, name, label);
        }

        private protected void AddSummary(VisualElement parent, System.Func<string> describe)
        {
            var summary = BoardUI.Text(describe(), "cwb-meta");
            parent.Add(summary);
            summary.TrackSerializedObjectValue(serializedObject, _ => summary.text = describe());
        }
    }
}