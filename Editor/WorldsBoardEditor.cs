using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nomlas.CompactWorldBoard.Editor
{
    [CustomEditor(typeof(WorldsBoard))]
    public class WorldsBoardEditor : BoardEditor
    {
        WorldsBoard worldsBoard;
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
            worldsBoard = (WorldsBoard)target;

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
            EditorGUILayout.LabelField("生成するインスタンスの設定", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(worldIdsProp, new GUIContent("ワールドID"), true);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(instanceIdProp, new GUIContent("インスタンスID"));
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

        private protected override void AddOrSetWorld(string worldId)
        {
            var worldIdsList = new List<string>(worldsBoard.worldIds);
            worldIdsList.Add(worldId);
            worldsBoard.worldIds = worldIdsList.ToArray();
        }
    }
}