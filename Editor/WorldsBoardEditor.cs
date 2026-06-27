using UnityEditor;
using UnityEngine;

namespace Nomlas.CompactWorldBoard.Editor
{
    [CustomEditor(typeof(WorldsBoard))]
    public class WorldsBoardEditor : BoardEditor
    {
        WorldsBoard worldsBoard;
        SerializedProperty instanceIdProp;
        private protected override void GetProperties()
        {
            worldsBoard = (WorldsBoard)target;
            instanceIdProp = serializedObject.FindProperty("instanceId");
        }

        private protected override void DrawInspectorGUI()
        {
            EditorGUILayout.LabelField("生成するインスタンスの設定", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("ワールド", $"{worldsBoard.worldIds?.Length ?? 0} 件");
                if (GUILayout.Button("ワールド一覧を編集", GUILayout.Width(160)))
                {
                    WorldsBoardWindow.Open(worldsBoard);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(instanceIdProp, new GUIContent("インスタンスID"));
            DrawCommonInspectorGUI();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            DrawPortalInspectorGUI();
        }
    }
}