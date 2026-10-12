using UnityEditor;
using UnityEngine.UIElements;

namespace Nomlas.CompactWorldBoard.Editor
{
    [CustomEditor(typeof(InstancesBoard))]
    public class InstancesBoardEditor : BoardEditor
    {
        private protected override void BuildBoardSettings(VisualElement parent)
        {
            var board = (InstancesBoard)target;
            var section = BoardUI.Card(parent, "インスタンスボード");
            AddSummary(section, () => $"{board.instanceIds?.Length ?? 0} インスタンス · {BoardSettingsUI.Describe(board)}");
            section.Add(BoardUI.Button("ボードを編集", () => InstancesBoardWindow.Open(board), icon: "open"));
        }
    }
}