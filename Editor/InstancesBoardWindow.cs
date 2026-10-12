using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nomlas.CompactWorldBoard.Editor
{
    public class InstancesBoardWindow : EditorWindow
    {
        [SerializeField] private InstancesBoard board;
        private ObjectField boardField;
        private VisualElement content;

        [MenuItem("Tools/Compact World Board/Instances Board")]
        public static void OpenFromMenu()
        {
            Open(Selection.activeGameObject == null ? null : Selection.activeGameObject.GetComponent<InstancesBoard>());
        }

        public static void Open(InstancesBoard target)
        {
            var window = GetWindow<InstancesBoardWindow>("Instances Board");
            if (target != null) window.SetBoard(target);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            minSize = new Vector2(520, 440);
            titleContent = new GUIContent("Instances Board");
        }

        private void OnSelectionChange()
        {
            var selected = Selection.activeGameObject == null ? null : Selection.activeGameObject.GetComponent<InstancesBoard>();
            if (selected != null && selected != board) SetBoard(selected);
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();
            BoardUI.ApplyTheme(root);
            var header = BoardUI.Row();
            header.Add(BoardUI.Icon("list"));
            var title = BoardUI.Text("Instances Board", "cwb-title");
            title.AddToClassList("cwb-grow");
            header.Add(title);
            header.Add(BoardUI.VersionBadge());
            root.Add(header);
            var targetRow = BoardUI.Row();
            targetRow.AddToClassList("cwb-target-row");
            boardField = new ObjectField("ボード") { objectType = typeof(InstancesBoard), allowSceneObjects = true };
            boardField.AddToClassList("cwb-grow");
            boardField.RegisterValueChangedCallback(e => SetBoard(e.newValue as InstancesBoard));
            targetRow.Add(boardField);
            var select = BoardUI.Button("", () => { if (board != null) Selection.activeObject = board.gameObject; }, "cwb-icon-button", "open");
            select.tooltip = "シーンでボードを選択";
            targetRow.Add(select);
            root.Add(targetRow);
            content = new ScrollView(ScrollViewMode.Vertical);
            content.AddToClassList("cwb-page");
            root.Add(content);
            Rebuild();
        }

        private void SetBoard(InstancesBoard target)
        {
            board = target;
            Rebuild();
        }

        private void Rebuild()
        {
            if (content == null) return;
            boardField.SetValueWithoutNotify(board);
            content.Unbind();
            content.Clear();
            if (board == null)
            {
                content.Add(BoardUI.Text("シーンのInstancesBoardを選択するか、上の欄に指定してください。", "cwb-empty"));
                return;
            }
            var world = BoardUI.Card(content, "ワールド", "このワールドに、下のインスタンスを作成します。");
            var serialized = new SerializedObject(board);
            BoardSettingsUI.AddProperty(world, serialized, "worldId", "ワールドID");
            world.Bind(serialized);
            content.Add(BoardSettingsUI.Build(board));
        }
    }
}