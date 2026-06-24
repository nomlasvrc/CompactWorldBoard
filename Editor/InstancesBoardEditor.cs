
using UnityEditor;
using UnityEngine;

namespace Nomlas.CompactWorldBoard.Editor
{
    [CustomEditor(typeof(InstancesBoard))]
    public class InstancesBoardEditor : BoardEditor
    {
        SerializedProperty worldIdProp;
        SerializedProperty instanceIdsProp;
        SerializedProperty userOrGroupProp;
        SerializedProperty instanceTypeProp;
        SerializedProperty userIdProp;
        SerializedProperty groupTypeProp;
        SerializedProperty groupIdProp;
        SerializedProperty regionProp;

        SerializedProperty canEnterProp;
        private protected override void GetProperties()
        {
            worldIdProp = serializedObject.FindProperty("worldId");
            instanceIdsProp = serializedObject.FindProperty("instanceIds");
            userOrGroupProp = serializedObject.FindProperty("userOrGroup");
            instanceTypeProp = serializedObject.FindProperty("instanceType");
            userIdProp = serializedObject.FindProperty("userId");
            groupTypeProp = serializedObject.FindProperty("groupType");
            groupIdProp = serializedObject.FindProperty("groupId");
            regionProp = serializedObject.FindProperty("region");

            canEnterProp = serializedObject.FindProperty("canEnter");
        }

        private protected override void DrawInspectorGUI()
        {
            EditorGUILayout.LabelField("生成するインスタンスの設定", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(worldIdProp, new GUIContent("ワールドID"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(instanceIdsProp, new GUIContent("インスタンスID"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(userOrGroupProp, new GUIContent("インスタンスのオーナー"));
            EditorGUILayout.Space();
            if (userOrGroupProp.enumValueIndex == (int)UserOrGroup.User)
            {
                EditorGUILayout.PropertyField(instanceTypeProp, new GUIContent("インスタンスの種類"));
                EditorGUILayout.PropertyField(userIdProp, new GUIContent("ユーザーID"));
            }
            else if (userOrGroupProp.enumValueIndex == (int)UserOrGroup.Group)
            {
                EditorGUILayout.PropertyField(groupTypeProp, new GUIContent("グループの種類"));
                EditorGUILayout.PropertyField(groupIdProp, new GUIContent("グループID"));
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(regionProp, new GUIContent("サーバーリージョン"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(canEnterProp, new GUIContent("ポータルに入れるか（通常オフ）"));
        }
    }
}