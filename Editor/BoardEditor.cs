
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Nomlas.CompactWorldBoard.Editor
{
    public abstract class BoardEditor : UnityEditor.Editor
    {
        private protected abstract void GetProperties();
        private protected abstract void DrawInspectorGUI();
        private protected abstract void AddOrSetWorld(string worldId);

        bool fold;

        private void OnEnable()
        {
            GetProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawInspectorGUI();

            EditorGUI.indentLevel++;
            fold = EditorGUILayout.Foldout(fold, "Default Inspector");
            if (fold) DrawDefaultInspector();
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            DrawFavoriteWorldsGui();

            serializedObject.ApplyModifiedProperties();
        }

        Api.FavoriteGroups favoriteWorldGroups;
        string[] favoriteWorldGroupNames;
        int selectedFavoriteWorldGroupIndex = 0;
        List<Api.FavoritedWorld> favorites;
        bool isFetchingFavoriteWorldGroups;
        bool isFetchingFavoritedWorlds;
        string favoriteWorldsError;

        private void DrawFavoriteWorldsGui()
        {
            EditorGUILayout.LabelField("お気に入り登録されたワールドを追加", EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(favoriteWorldsError))
            {
                EditorGUILayout.HelpBox(favoriteWorldsError, MessageType.Error);
            }

            EditorGUI.BeginDisabledGroup(isFetchingFavoriteWorldGroups || favoriteWorldGroupNames != null);
            if (GUILayout.Button(isFetchingFavoriteWorldGroups ? "お気に入りワールドのグループを読み込み中..." : "お気に入りワールドのグループを読み込む"))
            {
                FetchFavoriteWorldGroupNamesAsync().Forget();
            }
            EditorGUI.EndDisabledGroup();

            if (favoriteWorldGroupNames == null) return;

            EditorGUILayout.Space();

            selectedFavoriteWorldGroupIndex = EditorGUILayout.Popup("ワールドグループ", selectedFavoriteWorldGroupIndex, favoriteWorldGroupNames);
            EditorGUI.BeginDisabledGroup(isFetchingFavoritedWorlds);
            if (GUILayout.Button(isFetchingFavoritedWorlds ? "お気に入りワールドを読み込み中..." : "お気に入りワールドを読み込む"))
            {
                FetchFavoritedWorldsAsync().Forget();
            }
            EditorGUI.EndDisabledGroup();

            if (favorites == null) return;
            foreach (var favorite in favorites)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(favorite.world.Name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(favorite.world.AuthorName, GUILayout.MaxWidth(100));
                if (GUILayout.Button("追加"))
                {
                    AddOrSetWorld(favorite.world.ID);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private async UniTask FetchFavoriteWorldGroupNamesAsync()
        {
            favoriteWorldsError = null;
            isFetchingFavoriteWorldGroups = true;
            try
            {
                favoriteWorldGroups = await Api.WorldsApi.GetFavoriteWorldsList();
                favoriteWorldGroupNames = favoriteWorldGroups.Groups.ConvertAll(g => g.DisplayName).ToArray();
                selectedFavoriteWorldGroupIndex = 0;
            }
            catch (System.Exception ex)
            {
                favoriteWorldsError = $"お気に入りワールドグループの読み込みに失敗しました: {ex.Message}";
                favoriteWorldGroupNames = null;
            }
            finally
            {
                isFetchingFavoriteWorldGroups = false;
                Repaint();
            }
        }

        private async UniTask FetchFavoritedWorldsAsync()
        {
            favoriteWorldsError = null;
            isFetchingFavoritedWorlds = true;
            try
            {
                if (selectedFavoriteWorldGroupIndex < 0 || selectedFavoriteWorldGroupIndex >= favoriteWorldGroupNames.Length)
                {
                    favoriteWorldsError = "無効なお気に入りワールドグループが選択されています。";
                    return;
                }

                var favoriteWorlds = await Api.WorldsApi.GetFavoriteWorlds(favoriteWorldGroups.Groups[selectedFavoriteWorldGroupIndex].Name);
                favorites = favoriteWorlds.Favorites;
            }
            catch (System.Exception ex)
            {
                favoriteWorldsError = $"お気に入りワールドの読み込みに失敗しました: {ex.Message}";
                favorites = null;
            }
            finally
            {
                isFetchingFavoritedWorlds = false;
                Repaint();
            }
        }
    }
}