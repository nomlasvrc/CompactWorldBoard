
using UnityEditor;

namespace Nomlas.CompactWorldBoard.Editor
{
    public abstract class BoardEditor : UnityEditor.Editor
    {
        private protected abstract void GetProperties();
        private protected abstract void DrawInspectorGUI();

        bool fold;

        private void OnEnable()
        {
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
    }
}