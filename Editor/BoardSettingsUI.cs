using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Nomlas.CompactWorldBoard.Editor
{
    internal static class BoardSettingsUI
    {
        internal static string Describe(Board board)
        {
            if (board == null) return "ボード未選択";
            var access = board.userOrGroup == UserOrGroup.Public ? "Public" : board.userOrGroup == UserOrGroup.User ? board.instanceType.ToString() : board.groupType.ToString();
            return $"{access} · {board.region.ToString().ToUpperInvariant()}";
        }

        internal static VisualElement Build(Board board)
        {
            var root = new VisualElement { name = "instance-settings" };
            if (board == null)
            {
                root.Add(BoardUI.Text("編集するボードを指定してください。", "cwb-empty"));
                return root;
            }
            var serialized = new SerializedObject(board);
            var identity = BoardUI.Card(root, "インスタンス", board is WorldsBoard
                ? "すべてのワールドに共通のインスタンスIDを使用します。空欄の場合は自動生成されます。"
                : "同じワールドに作成するインスタンスを、表示順に並べます。");
            if (board is WorldsBoard) AddProperty(identity, serialized, "instanceId", "インスタンスID");
            else BuildInstanceList(identity, serialized);
            var access = BoardUI.Card(root, "アクセスとリージョン");
            AddProperty(access, serialized, "userOrGroup", "オーナー");
            var user = new VisualElement();
            AddProperty(user, serialized, "instanceType", "アクセス範囲");
            AddProperty(user, serialized, "userId", "ユーザーID");
            access.Add(user);
            var group = new VisualElement();
            AddProperty(group, serialized, "groupType", "アクセス範囲");
            AddProperty(group, serialized, "groupId", "グループID");
            access.Add(group);
            AddProperty(access, serialized, "region", "リージョン");
            var summary = BoardUI.Text(Describe(board), "cwb-meta");
            access.Add(summary);
            void UpdateAccess()
            {
                var owner = serialized.FindProperty("userOrGroup").enumValueIndex;
                user.style.display = owner == (int)UserOrGroup.User ? DisplayStyle.Flex : DisplayStyle.None;
                group.style.display = owner == (int)UserOrGroup.Group ? DisplayStyle.Flex : DisplayStyle.None;
                summary.text = Describe(board);
            }
            UpdateAccess();
            root.TrackSerializedObjectValue(serialized, _ => UpdateAccess());
            root.Bind(serialized);
            return root;
        }

        internal static PropertyField AddProperty(VisualElement parent, SerializedObject serialized, string name, string label)
        {
            var field = new PropertyField(serialized.FindProperty(name), label);
            parent.Add(field);
            return field;
        }

        private static void BuildInstanceList(VisualElement parent, SerializedObject serialized)
        {
            var array = serialized.FindProperty("instanceIds");
            var list = new VisualElement { name = "instance-id-list" };
            parent.Add(list);
            void Modify(Action<SerializedProperty> action)
            {
                serialized.Update();
                action(serialized.FindProperty("instanceIds"));
                serialized.ApplyModifiedProperties();
                Rebuild();
            }
            void Rebuild()
            {
                list.Clear();
                array = serialized.FindProperty("instanceIds");
                for (var i = 0; i < array.arraySize; i++)
                {
                    var index = i;
                    var row = BoardUI.Row();
                    row.AddToClassList("cwb-instance-row");
                    row.Add(BoardUI.Text((i + 1).ToString("00"), "cwb-index"));
                    var field = new TextField { isDelayed = true, tooltip = "インスタンスID（空欄で自動生成）" };
                    field.AddToClassList("cwb-grow");
                    row.Add(field);
                    field.BindProperty(array.GetArrayElementAtIndex(i));
                    var up = BoardUI.Button("↑", () => Modify(p => p.MoveArrayElement(index, index - 1)));
                    up.tooltip = "上へ移動";
                    up.SetEnabled(i > 0);
                    row.Add(up);
                    var down = BoardUI.Button("↓", () => Modify(p => p.MoveArrayElement(index, index + 1)));
                    down.tooltip = "下へ移動";
                    down.SetEnabled(i < array.arraySize - 1);
                    row.Add(down);
                    var delete = BoardUI.Button("", () => Modify(p => p.DeleteArrayElementAtIndex(index)), "cwb-icon-button", "delete");
                    delete.tooltip = "インスタンスを削除";
                    row.Add(delete);
                    list.Add(row);
                }
                if (array.arraySize == 0) list.Add(BoardUI.Text("インスタンスを追加してください。", "cwb-empty"));
            }
            Rebuild();
            parent.Add(BoardUI.Button("インスタンスを追加", () => Modify(p =>
            {
                var index = p.arraySize;
                p.InsertArrayElementAtIndex(index);
                p.GetArrayElementAtIndex(index).stringValue = "";
            }), icon: "add"));
            list.TrackPropertyValue(array, _ => Rebuild());
        }
    }
}