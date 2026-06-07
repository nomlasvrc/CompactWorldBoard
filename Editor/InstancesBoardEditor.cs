using System.Linq;
using UnityEditor;

namespace Nomlas.CompactWorldBoard.Editor
{
    [CustomEditor(typeof(InstancesBoard))]
    public class InstancesBoardEditor : BoardEditor
    {
        SerializedProperty worldIdProp;
        SerializedProperty instanceIdPrefixProp;
        SerializedProperty instanceIdSuffixProp;
        SerializedProperty instanceCountProp;
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
            instanceIdPrefixProp = serializedObject.FindProperty("instanceIdPrefix");
            instanceIdSuffixProp = serializedObject.FindProperty("instanceIdSuffix");
            instanceCountProp = serializedObject.FindProperty("instanceCount");
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
            EditorGUILayout.PropertyField(instanceIdPrefixProp);
            EditorGUILayout.PropertyField(instanceIdSuffixProp);
            EditorGUILayout.PropertyField(instanceCountProp);
            EditorGUILayout.LabelField("作成されるインスタンス", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(Enumerable.Range(0, instanceCountProp.intValue).Select(i => $"#{instanceIdPrefixProp.stringValue}{(char)('A' + i)}{instanceIdSuffixProp.stringValue}").Aggregate((a, b) => $"{a}\n{b}"));
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

        private protected override void AddOrSetWorld(string worldId)
        {
            worldIdProp.stringValue = worldId;
        }
    }
}