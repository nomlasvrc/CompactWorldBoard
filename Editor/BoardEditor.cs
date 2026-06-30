
using UnityEditor;
using UnityEngine;

namespace Nomlas.CompactWorldBoard.Editor
{
    public abstract class BoardEditor : UnityEditor.Editor
    {
        private protected abstract void GetProperties();
        private protected abstract void DrawInspectorGUI();

        private bool fold;

        private void OnEnable()
        {
            GetCommonProperties();
            GetProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawUpdateCheckStatus();
            EditorGUILayout.Space();
            DrawInspectorGUI();
            EditorGUILayout.Space(20);
            EditorGUI.indentLevel++;
            fold = EditorGUILayout.Foldout(fold, "Default Inspector");
            if (fold) DrawDefaultInspector();
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }



        private SerializedProperty userOrGroupProp;
        private SerializedProperty instanceTypeProp;
        private SerializedProperty userIdProp;
        private SerializedProperty groupTypeProp;
        private SerializedProperty groupIdProp;
        private SerializedProperty regionProp;
        private SerializedProperty canEnterProp;
        private SerializedProperty createDelaySecondsMinProp;
        private SerializedProperty createDelaySecondsMaxProp;

        private void DrawUpdateCheckStatus()
        {
            if (!VPMUpdateCheckCache.IsCompleted)
            {
                EditorGUILayout.HelpBox("更新チェック中です。", MessageType.Info);
                return;
            }

            if (VPMUpdateCheckCache.LastError != null)
            {
                EditorGUILayout.HelpBox($"更新チェックに失敗しました。\n{VPMUpdateCheckCache.LastError.Message}", MessageType.Error);
                return;
            }

            var result = VPMUpdateCheckCache.Result;
            if (result == null)
            {
                EditorGUILayout.HelpBox("更新チェック結果がありません。", MessageType.Error);
                return;
            }

            if (result.HasUpdate)
            {
                EditorGUILayout.HelpBox($"CompactWorldBoardのアップデートが利用可能です。\n{result.CurrentVersion} -> {result.LatestVersion}", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"CompactWorldBoardは最新です。\n{result.CurrentVersion}", MessageType.None);
            }
        }

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