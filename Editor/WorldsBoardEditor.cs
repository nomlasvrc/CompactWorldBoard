using UnityEditor;
using UnityEngine.UIElements;

namespace Nomlas.CompactWorldBoard.Editor
{
    [CustomEditor(typeof(WorldsBoard))]
    public class WorldsBoardEditor : BoardEditor
    {
        private protected override void BuildBoardSettings(VisualElement parent)
        {
            var board = (WorldsBoard)target;
            var section = BoardUI.Section(parent, "ワールドボード");
            AddSummary(section, () => $"{board.worldIds?.Length ?? 0} ワールド · {BoardSettingsUI.Describe(board)}");
            section.Add(BoardUI.Button("ボードを編集", () => WorldsBoardWindow.Open(board), icon: "open"));
        }
    }
}