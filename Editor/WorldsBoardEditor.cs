using UnityEditor;

namespace Nomlas.CompactWorldBoard.Editor
{
    [CustomEditor(typeof(WorldsBoard))]
    public class WorldsBoardEditor : BoardEditor
    {
        SerializedProperty worldIdsProp;
        SerializedProperty instanceIdProp;
        SerializedProperty userOrGroupProp;
        SerializedProperty instanceTypeProp;
        SerializedProperty userIdProp;
        SerializedProperty groupTypeProp;
        SerializedProperty groupIdProp;
        SerializedProperty regionProp;

        SerializedProperty canEnterProp;
        private protected override void GetProperties()
        {
            worldIdsProp = serializedObject.FindProperty("worldIds");
            instanceIdProp = serializedObject.FindProperty("instanceId");
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
            EditorGUILayout.PropertyField(worldIdsProp, true);
            EditorGUILayout.PropertyField(instanceIdProp);
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
            worldIdsProp.arraySize++;
            worldIdsProp.GetArrayElementAtIndex(worldIdsProp.arraySize - 1).stringValue = worldId;
        }
    }
}