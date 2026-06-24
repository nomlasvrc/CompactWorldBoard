
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
            EditorGUILayout.PropertyField(worldIdProp);
            EditorGUILayout.Space();
            DrawInstanceIdsGUI();
            EditorGUILayout.PropertyField(userOrGroupProp);
            if (userOrGroupProp.enumValueIndex == (int)UserOrGroup.User)
            {
                EditorGUILayout.PropertyField(instanceTypeProp);
                EditorGUILayout.PropertyField(userIdProp);
            }
            else if (userOrGroupProp.enumValueIndex == (int)UserOrGroup.Group)
            {
                EditorGUILayout.PropertyField(groupTypeProp);
                EditorGUILayout.PropertyField(groupIdProp);
            }
            EditorGUILayout.PropertyField(regionProp);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(canEnterProp);
        }

        private void DrawInstanceIdsGUI()
        {
            EditorGUILayout.LabelField("インスタンスID", EditorStyles.boldLabel);
            for (int i = 0; i < instanceIdsProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(instanceIdsProp.GetArrayElementAtIndex(i), new GUIContent($"インスタンスID {i + 1}"));
                if (GUILayout.Button("削除"))
                {
                    instanceIdsProp.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("インスタンスIDを追加"))
            {
                instanceIdsProp.arraySize++;
            }
            EditorGUILayout.Space();
        }

        private protected override void AddOrSetWorld(string worldId)
        {
            worldIdProp.stringValue = worldId;
        }
    }
}