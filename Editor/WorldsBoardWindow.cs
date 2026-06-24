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
    public class WorldsBoardWindow : EditorWindow
    {
        [SerializeField] WorldsBoard worldsBoard;
        [SerializeField] List<WorldCache> worlds = new List<WorldCache>();
        [SerializeField] List<WorldCache> favorites = new List<WorldCache>();
        [SerializeField] string[] favoriteWorldGroupNames;
        [SerializeField] string[] favoriteWorldGroupApiNames;
        [SerializeField] int selectedFavoriteWorldGroupIndex;
        [SerializeField] Vector2 worldListScroll;
        [SerializeField] Vector2 favoriteListScroll;
        [SerializeField] string newWorldId;
        [SerializeField] string filterText;
        [SerializeField] bool showFavorites = true;

        bool isFetchingWorlds;
        bool isFetchingFavoriteWorldGroups;
        bool isFetchingFavoritedWorlds;

        const float WindowMinWidth = 760f;
        const float WindowMinHeight = 520f;
        const float WorldListMinHeight = 360f;
        const float FavoriteListHeight = 180f;
        const float IndexWidth = 28f;
        const float AuthorWidth = 120f;
        const float CapacityWidth = 60f;
        const float OperationWidth = 190f;

        [MenuItem("Tools/Compact World Board/Worlds Board")]
        public static void OpenFromMenu()
        {
            var selectedBoard = GetSelectedBoard();
            if (selectedBoard != null)
            {
                Open(selectedBoard);
                return;
            }

            var window = GetWindow<WorldsBoardWindow>("Worlds Board");
            window.minSize = new Vector2(WindowMinWidth, WindowMinHeight);
            window.Show();
        }

        public static void Open(WorldsBoard board)
        {
            var window = GetWindow<WorldsBoardWindow>("Worlds Board");
            window.SetBoard(board);
            window.minSize = new Vector2(WindowMinWidth, WindowMinHeight);
            window.Show();
            window.Focus();
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Worlds Board");
            SyncCacheWithBoard();
        }

        void OnSelectionChange()
        {
            var selectedBoard = GetSelectedBoard();
            if (selectedBoard == null || selectedBoard == worldsBoard) return;

            SetBoard(selectedBoard);
            Repaint();
        }

        void OnGUI()
        {
            if (worldsBoard == null)
            {
                DrawNoBoardGUI();
                return;
            }

            DrawToolbar();
            if (worldsBoard == null) return;

            EditorGUILayout.Space();
            DrawAddWorldGUI();
            EditorGUILayout.Space();
            DrawWorldListGUI();
            EditorGUILayout.Space();
            DrawFavoriteWorldsGUI();
        }

        void DrawNoBoardGUI()
        {
            EditorGUILayout.HelpBox("編集する WorldsBoard をインスペクターから開くか、シーン上で選択してください。", MessageType.Info);
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                var selectedBoard = (WorldsBoard)EditorGUILayout.ObjectField(worldsBoard, typeof(WorldsBoard), true);
                if (EditorGUI.EndChangeCheck())
                {
                    SetBoard(selectedBoard);
                }
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(worldsBoard == null))
                {
                    if (GUILayout.Button("選択", EditorStyles.toolbarButton, GUILayout.Width(48)))
                    {
                        Selection.activeObject = worldsBoard.gameObject;
                    }
                }
            }
        }

        void DrawAddWorldGUI()
        {
            EditorGUILayout.LabelField("ワールドを追加", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                newWorldId = EditorGUILayout.TextField("ワールドID", newWorldId);
                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(newWorldId)))
                {
                    if (GUILayout.Button("追加", GUILayout.Width(80)))
                    {
                        AddWorld(newWorldId.Trim());
                        newWorldId = string.Empty;
                    }
                }
            }
        }

        void DrawWorldListGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"ワールド一覧 ({WorldIds.Length} 件)", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                filterText = GUILayout.TextField(filterText ?? string.Empty, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField, GUILayout.Width(220));
                if (GUILayout.Button("x", GUI.skin.FindStyle("ToolbarSeachCancelButton") ?? EditorStyles.toolbarButton, GUILayout.Width(22)))
                {
                    filterText = string.Empty;
                    GUI.FocusControl(null);
                }
                using (new EditorGUI.DisabledScope(isFetchingWorlds))
                {
                    if (GUILayout.Button(isFetchingWorlds ? "取得中..." : "ワールド情報を取得", GUILayout.Width(110)))
                    {
                        GetWorldsAsync().Forget();
                    }
                }
            }

            DrawWorldListHeader();
            worldListScroll = EditorGUILayout.BeginScrollView(worldListScroll, GUILayout.MinHeight(WorldListMinHeight), GUILayout.ExpandHeight(true));
            var worldIds = WorldIds;
            for (var i = 0; i < worldIds.Length; i++)
            {
                if (!MatchesFilter(worldIds[i])) continue;
                DrawWorldRow(i, worldIds[i]);
            }
            EditorGUILayout.EndScrollView();
        }

        void DrawWorldListHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("#", EditorStyles.miniBoldLabel, GUILayout.Width(IndexWidth));
                GUILayout.Label("ワールドID", EditorStyles.miniBoldLabel, GUILayout.MinWidth(180));
                GUILayout.Label("名前", EditorStyles.miniBoldLabel, GUILayout.MinWidth(180));
                GUILayout.Label("作者", EditorStyles.miniBoldLabel, GUILayout.Width(AuthorWidth));
                GUILayout.Label("人数", EditorStyles.miniBoldLabel, GUILayout.Width(CapacityWidth));
                GUILayout.Label("操作", EditorStyles.miniBoldLabel, GUILayout.Width(OperationWidth));
            }
        }

        void DrawWorldRow(int index, string worldId)
        {
            var cachedWorld = FindWorld(worldId);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label((index + 1).ToString(), GUILayout.Width(IndexWidth));

                EditorGUI.BeginChangeCheck();
                var editedWorldId = EditorGUILayout.TextField(worldId, GUILayout.MinWidth(180));
                if (EditorGUI.EndChangeCheck())
                {
                    SetWorldId(index, editedWorldId.Trim());
                }

                GUILayout.Label(cachedWorld?.Name ?? "未取得", GUILayout.MinWidth(180));
                GUILayout.Label(cachedWorld?.AuthorName ?? "-", GUILayout.Width(AuthorWidth));
                GUILayout.Label(cachedWorld == null ? "-" : cachedWorld.Capacity.ToString(), GUILayout.Width(CapacityWidth));

                using (new EditorGUI.DisabledScope(index == 0))
                {
                    if (GUILayout.Button("↑", GUILayout.Width(32))) MoveWorld(index, index - 1);
                }
                using (new EditorGUI.DisabledScope(index >= WorldIds.Length - 1))
                {
                    if (GUILayout.Button("↓", GUILayout.Width(32))) MoveWorld(index, index + 1);
                }
                using (new EditorGUI.DisabledScope(isFetchingWorlds || cachedWorld != null || string.IsNullOrEmpty(worldId)))
                {
                    if (GUILayout.Button("取得", GUILayout.Width(48))) GetWorldAsync(worldId).Forget();
                }
                if (GUILayout.Button("削除", GUILayout.Width(48)))
                {
                    RemoveWorldAt(index);
                }
            }
        }

        void DrawFavoriteWorldsGUI()
        {
            showFavorites = EditorGUILayout.Foldout(showFavorites, "お気に入りワールドから追加", true);
            if (!showFavorites) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(isFetchingFavoriteWorldGroups || favoriteWorldGroupNames != null))
                {
                    if (GUILayout.Button(isFetchingFavoriteWorldGroups ? "グループ取得中..." : "グループを取得", GUILayout.Width(150)))
                    {
                        FetchFavoriteWorldGroupNamesAsync().Forget();
                    }
                }

                if (favoriteWorldGroupNames != null)
                {
                    selectedFavoriteWorldGroupIndex = EditorGUILayout.Popup(selectedFavoriteWorldGroupIndex, favoriteWorldGroupNames);
                    using (new EditorGUI.DisabledScope(isFetchingFavoritedWorlds))
                    {
                        if (GUILayout.Button(isFetchingFavoritedWorlds ? "取得中..." : "ワールドを取得", GUILayout.Width(120)))
                        {
                            FetchFavoritedWorldsAsync().Forget();
                        }
                    }
                }
            }

            if (favorites.Count == 0) return;

            favoriteListScroll = EditorGUILayout.BeginScrollView(favoriteListScroll, GUILayout.Height(FavoriteListHeight));
            foreach (var favorite in favorites)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(favorite.Name, EditorStyles.boldLabel, GUILayout.MinWidth(220));
                    GUILayout.Label(favorite.AuthorName, GUILayout.Width(120));
                    GUILayout.Label(favorite.ID, GUILayout.Width(300));

                    var isWorldIdInList = WorldIds.Contains(favorite.ID);
                    using (new EditorGUI.DisabledScope(isWorldIdInList))
                    {
                        if (GUILayout.Button(isWorldIdInList ? "追加済み" : "追加", GUILayout.Width(80)))
                        {
                            AddWorld(favorite.ID);
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        async UniTask GetWorldsAsync()
        {
            isFetchingWorlds = true;
            var progressId = UnityEditor.Progress.Start("ワールド情報を取得中...");
            try
            {
                var worldIds = WorldIds
                    .Where(id => !string.IsNullOrWhiteSpace(id) && FindWorld(id) == null)
                    .Distinct()
                    .ToArray();

                for (var i = 0; i < worldIds.Length; i++)
                {
                    await FetchAndCacheWorld(worldIds[i]);
                    UnityEditor.Progress.Report(progressId, (float)(i + 1) / worldIds.Length, $"ワールド情報を取得中... ({i + 1}/{worldIds.Length})");
                    Repaint();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ワールド情報の取得に失敗しました: {ex.Message}");
            }
            finally
            {
                UnityEditor.Progress.Remove(progressId);
                isFetchingWorlds = false;
                Repaint();
            }
        }

        async UniTask GetWorldAsync(string worldId)
        {
            isFetchingWorlds = true;
            try
            {
                await FetchAndCacheWorld(worldId);
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

        async UniTask FetchFavoriteWorldGroupNamesAsync()
        {
            isFetchingFavoriteWorldGroups = true;
            try
            {
                var favoriteWorldGroups = await WorldsApi.GetFavoriteWorldsList();
                favoriteWorldGroupNames = favoriteWorldGroups.Groups.ConvertAll(g => g.DisplayName).ToArray();
                favoriteWorldGroupApiNames = favoriteWorldGroups.Groups.ConvertAll(g => g.Name).ToArray();
                selectedFavoriteWorldGroupIndex = 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"お気に入りワールドグループの読み込みに失敗しました: {ex.Message}");
                favoriteWorldGroupNames = null;
                favoriteWorldGroupApiNames = null;
            }
            finally
            {
                isFetchingFavoriteWorldGroups = false;
                Repaint();
            }
        }

        async UniTask FetchFavoritedWorldsAsync()
        {
            isFetchingFavoritedWorlds = true;
            try
            {
                if (favoriteWorldGroupApiNames == null || selectedFavoriteWorldGroupIndex < 0 || selectedFavoriteWorldGroupIndex >= favoriteWorldGroupApiNames.Length)
                {
                    Debug.LogError("無効なお気に入りワールドグループが選択されています。");
                    return;
                }

                var favoriteWorlds = await WorldsApi.GetFavoriteWorlds(favoriteWorldGroupApiNames[selectedFavoriteWorldGroupIndex]);
                favorites = favoriteWorlds.Favorites
                    .Where(f => !string.IsNullOrEmpty(f.world.ID))
                    .Select(f => WorldCache.FromWorld(f.world))
                    .ToList();

                foreach (var favorite in favorites)
                {
                    CacheWorld(favorite);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"お気に入りワールドの読み込みに失敗しました: {ex.Message}");
                favorites.Clear();
            }
            finally
            {
                isFetchingFavoritedWorlds = false;
                Repaint();
            }
        }

        async UniTask FetchAndCacheWorld(string worldId)
        {
            var world = await VRCApi.GetWorld(worldId);
            CacheWorld(WorldCache.FromWorld(world));
        }

        void AddWorld(string worldId)
        {
            if (string.IsNullOrWhiteSpace(worldId)) return;

            var worldIds = WorldIds.ToList();
            if (worldIds.Contains(worldId)) return;

            RecordBoard("Add world");
            worldIds.Add(worldId);
            ApplyWorldIds(worldIds);
        }

        void SetWorldId(int index, string worldId)
        {
            if (!IsValidIndex(index)) return;

            var worldIds = WorldIds.ToArray();
            RecordBoard("Edit world ID");
            worldIds[index] = worldId;
            ApplyWorldIds(worldIds);
        }

        void RemoveWorldAt(int index)
        {
            if (!IsValidIndex(index)) return;

            var worldIds = WorldIds.ToList();
            RecordBoard("Remove world");
            worldIds.RemoveAt(index);
            ApplyWorldIds(worldIds);
        }

        void MoveWorld(int fromIndex, int toIndex)
        {
            if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex)) return;

            var worldIds = WorldIds.ToList();
            var worldId = worldIds[fromIndex];
            RecordBoard("Reorder worlds");
            worldIds.RemoveAt(fromIndex);
            worldIds.Insert(toIndex, worldId);
            ApplyWorldIds(worldIds);
        }

        void SetBoard(WorldsBoard board)
        {
            worldsBoard = board;
            SyncCacheWithBoard();
        }

        void ApplyWorldIds(IEnumerable<string> worldIds)
        {
            if (worldsBoard == null) return;

            worldsBoard.worldIds = worldIds.ToArray();
            EditorUtility.SetDirty(worldsBoard);
        }

        void SyncCacheWithBoard()
        {
            if (worlds == null) worlds = new List<WorldCache>();
            if (favorites == null) favorites = new List<WorldCache>();
            worlds.RemoveAll(w => string.IsNullOrEmpty(w.ID));
        }

        void CacheWorld(WorldCache world)
        {
            if (string.IsNullOrEmpty(world.ID)) return;

            var index = worlds.FindIndex(w => w.ID == world.ID);
            if (index >= 0)
            {
                worlds[index] = world;
            }
            else
            {
                worlds.Add(world);
            }
        }

        WorldCache FindWorld(string worldId)
        {
            if (string.IsNullOrEmpty(worldId)) return null;
            return worlds.FirstOrDefault(w => w.ID == worldId);
        }

        bool MatchesFilter(string worldId)
        {
            if (string.IsNullOrWhiteSpace(filterText)) return true;

            var cachedWorld = FindWorld(worldId);
            return Contains(worldId, filterText)
                   || Contains(cachedWorld?.Name, filterText)
                   || Contains(cachedWorld?.AuthorName, filterText);
        }

        static bool Contains(string value, string search)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        bool IsValidIndex(int index)
        {
            return index >= 0 && index < WorldIds.Length;
        }

        void RecordBoard(string operation)
        {
            Undo.RecordObject(worldsBoard, operation);
        }

        string[] WorldIds => worldsBoard?.worldIds ?? new string[0];

        static WorldsBoard GetSelectedBoard()
        {
            return Selection.activeGameObject == null ? null : Selection.activeGameObject.GetComponent<WorldsBoard>();
        }

        [Serializable]
        class WorldCache
        {
            public string ID;
            public string Name;
            public string AuthorName;
            public int Capacity;

            public static WorldCache FromWorld(VRCWorld world)
            {
                return new WorldCache
                {
                    ID = world.ID,
                    Name = world.Name,
                    AuthorName = world.AuthorName,
                    Capacity = world.Capacity
                };
            }
        }
    }
}

