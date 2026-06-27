
using UnityEditor;
using UnityEngine;

namespace Nomlas.CompactWorldBoard.Editor
{
    public abstract class BoardEditor : UnityEditor.Editor
    {
        private protected abstract void GetProperties();
        private protected abstract void DrawInspectorGUI();

        bool fold;

        private void OnEnable()
        {
            GetCommonProperties();
            GetProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawInspectorGUI();
            EditorGUILayout.Space(20);
            EditorGUI.indentLevel++;
            fold = EditorGUILayout.Foldout(fold, "Default Inspector");
            if (fold) DrawDefaultInspector();
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }


        
        SerializedProperty userOrGroupProp;
        SerializedProperty instanceTypeProp;
        SerializedProperty userIdProp;
        SerializedProperty groupTypeProp;
        SerializedProperty groupIdProp;
        SerializedProperty regionProp;
        SerializedProperty canEnterProp;
        SerializedProperty createDelaySecondsMinProp;
        SerializedProperty createDelaySecondsMaxProp;
        private void GetCommonProperties()
        {
            userOrGroupProp = serializedObject.FindProperty("userOrGroup");
            instanceTypeProp = serializedObject.FindProperty("instanceType");
            userIdProp = serializedObject.FindProperty("userId");
            groupTypeProp = serializedObject.FindProperty("groupType");
            groupIdProp = serializedObject.FindProperty("groupId");
            regionProp = serializedObject.FindProperty("region");
            canEnterProp = serializedObject.FindProperty("canEnter");
            createDelaySecondsMinProp = serializedObject.FindProperty("createDelaySecondsMin");
            createDelaySecondsMaxProp = serializedObject.FindProperty("createDelaySecondsMax");
        }

        private protected void DrawCommonInspectorGUI()
        {
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
        }

        private protected void DrawPortalInspectorGUI()
        {
            EditorGUILayout.LabelField("ポータルの設定", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(canEnterProp, new GUIContent("ポータルに入れるか（通常オフ）"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(createDelaySecondsMinProp, new GUIContent("生成までの最小遅延時間（秒）"));
            EditorGUILayout.PropertyField(createDelaySecondsMaxProp, new GUIContent("生成までの最大遅延時間（秒）"));
            EditorGUI.indentLevel--;
        }
    }
}