using UnityEditor;
using UnityEngine;

namespace Nomlas.CompactWorldBoard.Editor
{
    [CustomEditor(typeof(InstancesBoard))]
    public class InstancesBoardEditor : BoardEditor
    {
        SerializedProperty worldIdProp;
        SerializedProperty instanceIdsProp;
        private protected override void GetProperties()
        {
            worldIdProp = serializedObject.FindProperty("worldId");
            instanceIdsProp = serializedObject.FindProperty("instanceIds");
        }

        private protected override void DrawInspectorGUI()
        {
            EditorGUILayout.LabelField("生成するインスタンスの設定", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(worldIdProp, new GUIContent("ワールドID"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(instanceIdsProp, new GUIContent("インスタンスID"));
            DrawCommonInspectorGUI();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            DrawPortalInspectorGUI();
        }
    }
}