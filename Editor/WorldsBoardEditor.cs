using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nomlas.CompactWorldBoard.Editor.Api;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.Api;

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

            EditorGUILayout.LabelField("ワールドID", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            foreach (var worldId in worldsBoard.worldIds)
            {
                EditorGUILayout.BeginHorizontal();
                VRCWorld? vrcWorld = worldsCollection?.FirstOrDefault(w => w.ID == worldId);
                EditorGUILayout.LabelField(vrcWorld?.Name ?? worldId);
                if (GUILayout.Button("削除"))
                {
                    worldsBoard.worldIds = Enumerable.ToArray(Enumerable.Where(worldsBoard.worldIds, id => id != worldId));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;

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

            EditorGUILayout.Space(20);
            DrawGetWorldsButton();
            DrawFavoriteWorldsGUI();
        }

        // -----------
        HashSet<VRCWorld> worldsCollection = new HashSet<VRCWorld>();

        FavoriteGroups favoriteWorldGroups;
        string[] favoriteWorldGroupNames;
        int selectedFavoriteWorldGroupIndex = 0;
        List<FavoritedWorld> favorites;

        bool isFetchingWorlds;
        bool isFetchingFavoriteWorldGroups;
        bool isFetchingFavoritedWorlds;

        private void DrawFavoriteWorldsGUI()
        {
            EditorGUILayout.LabelField("お気に入り登録されたワールドを追加", EditorStyles.boldLabel);

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
                var isWorldIdInList = worldsBoard.worldIds.Contains(favorite.world.ID);
                EditorGUI.BeginDisabledGroup(isWorldIdInList);
                if (GUILayout.Button(isWorldIdInList ? "追加済み" : "追加"))
                {
                    worldsBoard.worldIds = worldsBoard.worldIds.Append(favorite.world.ID).ToArray();

                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawGetWorldsButton()
        {
            EditorGUILayout.LabelField("ワールド情報を取得", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(isFetchingWorlds);
            if (GUILayout.Button("ワールド情報を取得"))
            {
                GetWorldsAsync().Forget();
            }
            EditorGUI.EndDisabledGroup();
        }

        private async UniTask GetWorldsAsync()
        {
            try
            {
                isFetchingWorlds = true;
                var progressId = UnityEditor.Progress.Start("ワールド情報を取得中...");
                foreach (var worldId in worldsBoard.worldIds)
                {
                    if (string.IsNullOrEmpty(worldId) || worldsCollection.Any(w => w.ID == worldId)) continue;
                    var world = await VRCApi.GetWorld(worldId);
                    worldsCollection.Add(world);
                    UnityEditor.Progress.Report(progressId, (float)worldsCollection.Count / worldsBoard.worldIds.Length, $"ワールド情報を取得中... ({worldsCollection.Count}/{worldsBoard.worldIds.Length})");
                }
                UnityEditor.Progress.Remove(progressId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ワールド情報の取得に失敗しました: {ex.Message}");
            }
            finally
            {
                isFetchingWorlds = false;
                Repaint();
            }
        }

        private async UniTask FetchFavoriteWorldGroupNamesAsync()
        {
            isFetchingFavoriteWorldGroups = true;
            try
            {
                favoriteWorldGroups = await WorldsApi.GetFavoriteWorldsList();
                favoriteWorldGroupNames = favoriteWorldGroups.Groups.ConvertAll(g => g.DisplayName).ToArray();
                selectedFavoriteWorldGroupIndex = 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"お気に入りワールドグループの読み込みに失敗しました: {ex.Message}");
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
            isFetchingFavoritedWorlds = true;
            try
            {
                if (selectedFavoriteWorldGroupIndex < 0 || selectedFavoriteWorldGroupIndex >= favoriteWorldGroupNames.Length)
                {
                    Debug.LogError("無効なお気に入りワールドグループが選択されています。");
                    return;
                }

                var favoriteWorlds = await WorldsApi.GetFavoriteWorlds(favoriteWorldGroups.Groups[selectedFavoriteWorldGroupIndex].Name);
                favorites = favoriteWorlds.Favorites;
                worldsCollection.UnionWith(favorites.ConvertAll(f => f.world));
            }
            catch (Exception ex)
            {
                Debug.LogError($"お気に入りワールドの読み込みに失敗しました: {ex.Message}");
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