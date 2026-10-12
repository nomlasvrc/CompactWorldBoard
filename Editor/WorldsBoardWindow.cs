using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Nomlas.CompactWorldBoard.Editor.Api;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase.Editor.Api;

namespace Nomlas.CompactWorldBoard.Editor
{
    public class WorldsBoardWindow : EditorWindow
    {
        [SerializeField] private WorldsBoard worldsBoard;
        [SerializeField] private List<WorldCache> worlds = new List<WorldCache>();
        [SerializeField] private List<WorldCache> favorites = new List<WorldCache>();
        [SerializeField] private string[] favoriteWorldGroupNames;
        [SerializeField] private string[] favoriteWorldGroupApiNames;
        [SerializeField] private int selectedFavoriteWorldGroupIndex;
        [SerializeField] private string newWorldId;
        [SerializeField] private string filterText;
        [SerializeField] private int selectedTab;
        [SerializeField] private string selectedWorldId;

        private bool isFetchingWorlds;
        private bool isFetchingFavoriteWorldGroups;
        private bool isFetchingFavoritedWorlds;
        private VisualElement boardContent;
        private ScrollView worldList;
        private ScrollView favoriteList;
        private Label listTitle;
        private TextField addField;
        private Button addButton;
        private Button fetchButton;
        private Button groupsButton;
        private Button favoritesButton;
        private PopupField<int> groupPicker;
        private BoardStatusLine status;
        private VisualElement settingsPage;
        private VisualElement[] pages;
        private Button[] tabs;
        private ObjectField boardField;
        private bool isWindowActive;
        private WorldThumbnailCache thumbnailCache;
        private const string WindowTitle = "Worlds Board";
        private static readonly Regex WorldIdPattern = new Regex(@"^wrld_[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", RegexOptions.IgnoreCase);

        [MenuItem("Tools/Compact World Board/" + WindowTitle)]
        public static void OpenFromMenu()
        {
            Open(GetSelectedBoard());
        }

        public static void Open(WorldsBoard board)
        {
            var window = GetWindow<WorldsBoardWindow>(WindowTitle);
            if (board != null) window.SetBoard(board);
            window.minSize = new Vector2(800, 440);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            isWindowActive = true;
            thumbnailCache = new WorldThumbnailCache();
            titleContent = new GUIContent(WindowTitle);
            minSize = new Vector2(800, 440);
            SyncCacheWithBoard();
            Undo.undoRedoPerformed += RefreshUI;
            EditorApplication.hierarchyChanged += RefreshUI;
        }

        private void OnDisable()
        {
            isWindowActive = false;
            thumbnailCache?.Dispose();
            thumbnailCache = null;
            Undo.undoRedoPerformed -= RefreshUI;
            EditorApplication.hierarchyChanged -= RefreshUI;
        }

        private void OnSelectionChange()
        {
            var selectedBoard = GetSelectedBoard();
            if (selectedBoard != null && selectedBoard != worldsBoard) SetBoard(selectedBoard);
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();
            BoardUI.ApplyTheme(root);
            var header = BoardUI.Row();
            header.Add(BoardUI.Icon("world"));
            var title = BoardUI.Text("Worlds Board", "cwb-title");
            title.AddToClassList("cwb-grow");
            header.Add(title);
            header.Add(BoardUI.VersionBadge());
            root.Add(header);
            var targetRow = BoardUI.Row();
            targetRow.AddToClassList("cwb-target-row");
            boardField = new ObjectField("ボード") { objectType = typeof(WorldsBoard), allowSceneObjects = true };
            boardField.AddToClassList("cwb-grow");
            boardField.SetValueWithoutNotify(worldsBoard);
            boardField.RegisterValueChangedCallback(e => SetBoard(e.newValue as WorldsBoard));
            targetRow.Add(boardField);
            var select = BoardUI.Button("", () =>
            {
                if (worldsBoard != null) Selection.activeObject = worldsBoard.gameObject;
            }, "cwb-icon-button", "open");
            select.tooltip = "シーンでボードを選択";
            targetRow.Add(select);
            root.Add(targetRow);
            boardContent = new VisualElement();
            boardContent.AddToClassList("cwb-grow");
            root.Add(boardContent);
            var tabRow = BoardUI.Row();
            tabRow.AddToClassList("cwb-tabs");
            boardContent.Add(tabRow);
            var worldsPage = new VisualElement();
            worldsPage.AddToClassList("cwb-page");
            worldsPage.AddToClassList("cwb-worlds-pane");
            settingsPage = new ScrollView(ScrollViewMode.Vertical);
            settingsPage.AddToClassList("cwb-page");
            var favoritesPage = new VisualElement();
            favoritesPage.AddToClassList("cwb-page");
            favoritesPage.AddToClassList("cwb-favorites-pane");
            var split = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal)
            {
                viewDataKey = "worlds-favorites-split"
            };
            split.AddToClassList("cwb-page");
            split.Add(favoritesPage);
            split.Add(worldsPage);
            pages = new VisualElement[] { split, settingsPage };
            tabs = new Button[2];
            var names = new[] { "ワールド", "インスタンス設定" };
            var icons = new[] { "world", "settings" };
            for (var i = 0; i < pages.Length; i++)
            {
                var index = i;
                tabs[i] = BoardUI.Button(names[i], () => SelectTab(index), "cwb-tab", icons[i]);
                tabRow.Add(tabs[i]);
                boardContent.Add(pages[i]);
            }
            RebuildSettings();

            var addRow = BoardUI.Row();
            addRow.AddToClassList("cwb-toolbar");
            addField = new TextField { value = newWorldId ?? "" };
            addField.tooltip = "ワールドID、またはVRChatのワールドURLを貼り付け（Enterで追加）";
            addField.AddToClassList("cwb-grow");
            addField.RegisterValueChangedCallback(e => { newWorldId = e.newValue; UpdateButtons(); });
            addField.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode != KeyCode.Return && e.keyCode != KeyCode.KeypadEnter) return;
                SubmitWorld();
                e.StopPropagation();
            });
            addRow.Add(BoardUI.Icon("add"));
            addRow.Add(addField);
            addButton = BoardUI.Button("追加", SubmitWorld);
            addRow.Add(addButton);
            worldsPage.Add(addRow);
            worldsPage.Add(BoardUI.Text("ワールドID / VRChat URL · Enterで追加", "cwb-input-hint"));

            var toolbar = BoardUI.Row();
            toolbar.AddToClassList("cwb-toolbar");
            listTitle = BoardUI.Text("ワールド", "cwb-meta");
            listTitle.AddToClassList("cwb-grow");
            toolbar.Add(listTitle);
            var search = new ToolbarSearchField { value = filterText ?? "", tooltip = "名前・作者・ワールドIDで検索" };
            search.AddToClassList("cwb-search");
            search.RegisterValueChangedCallback(e => { filterText = e.newValue; RefreshWorldList(); });
            toolbar.Add(search);
            fetchButton = BoardUI.Button("情報を取得", () => GetWorldsAsync().Forget());
            toolbar.Add(fetchButton);
            worldsPage.Add(toolbar);
            worldList = new ScrollView(ScrollViewMode.Vertical) { viewDataKey = "worlds-scroll" };
            worldList.AddToClassList("cwb-list");
            worldsPage.Add(worldList);

            var favoriteHeader = BoardUI.Row();
            favoriteHeader.AddToClassList("cwb-toolbar");
            favoriteHeader.Add(BoardUI.Icon("bookmark"));
            var favoriteTitle = BoardUI.Text("お気に入り", "cwb-section-title");
            favoriteTitle.AddToClassList("cwb-grow");
            favoriteHeader.Add(favoriteTitle);
            groupsButton = BoardUI.Button("", () => FetchFavoriteWorldGroupNamesAsync().Forget(), "cwb-icon-button", "refresh");
            groupsButton.AddToClassList("cwb-outlined");
            groupsButton.tooltip = "お気に入りグループを取得・更新";
            favoritesPage.Add(favoriteHeader);
            var favoriteHint = BoardUI.Text("お気に入りから追加（VRChat SDKへのログインが必要です）", "cwb-input-hint");
            favoriteHint.AddToClassList("cwb-favorite-hint");
            favoritesPage.Add(favoriteHint);
            var favoriteToolbar = BoardUI.Row();
            favoriteToolbar.AddToClassList("cwb-toolbar");
            groupPicker = new PopupField<int>(new List<int>(), -1, FavoriteGroupLabel, FavoriteGroupLabel);
            groupPicker.AddToClassList("cwb-favorite-group-picker");
            groupPicker.RegisterValueChangedCallback(e =>
            {
                if (selectedFavoriteWorldGroupIndex == e.newValue) return;
                selectedFavoriteWorldGroupIndex = e.newValue;
                favorites.Clear();
                RefreshFavorites();
            });
            favoriteToolbar.Add(groupPicker);
            favoriteToolbar.Add(groupsButton);
            favoritesPage.Add(favoriteToolbar);
            var favoriteFetchRow = BoardUI.Row();
            favoriteFetchRow.AddToClassList("cwb-toolbar");
            favoritesButton = BoardUI.Button("ワールドを取得", () => FetchFavoritedWorldsAsync().Forget());
            favoritesButton.AddToClassList("cwb-favorite-fetch");
            favoritesButton.AddToClassList("cwb-outlined");
            favoritesButton.tooltip = "選択したグループのワールドを取得";
            favoriteFetchRow.Add(favoritesButton);
            favoritesPage.Add(favoriteFetchRow);
            favoriteList = new ScrollView(ScrollViewMode.Vertical) { viewDataKey = "favorites-scroll" };
            favoriteList.AddToClassList("cwb-list");
            favoritesPage.Add(favoriteList);
            status = new BoardStatusLine();
            status.style.display = DisplayStyle.None;
            root.Add(status);
            SelectTab(selectedTab == 1 ? 1 : 0);
            RefreshUI();
        }

        private void SelectTab(int index)
        {
            selectedTab = index;
            for (var i = 0; i < pages.Length; i++)
            {
                pages[i].style.display = i == index ? DisplayStyle.Flex : DisplayStyle.None;
                tabs[i].EnableInClassList("cwb-selected", i == index);
            }
        }

        private void RebuildSettings()
        {
            if (settingsPage == null) return;
            settingsPage.Unbind();
            settingsPage.Clear();
            settingsPage.Add(BoardSettingsUI.Build(worldsBoard));
        }
        private void SubmitWorld()
        {
            var id = NormalizeWorldId(newWorldId);
            if (!WorldIdPattern.IsMatch(id))
            {
                ShowStatus("有効なワールドID、またはVRChatのワールドURLを入力してください。", BoardMessageKind.Warning);
                return;
            }
            if (WorldIds.Contains(id))
            {
                ShowStatus("このワールドは追加済みです。", BoardMessageKind.Info);
                return;
            }
            AddWorld(id);
            newWorldId = "";
            addField.SetValueWithoutNotify("");
            UpdateButtons();
            ShowStatus("ワールドを追加しました。名前や作者は「情報を取得」で読み込めます。", BoardMessageKind.Info);
        }

        private static string NormalizeWorldId(string value)
        {
            value = (value ?? "").Trim();
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && (uri.Scheme == "https" || uri.Scheme == "http")
                && (uri.Host == "vrchat.com" || uri.Host == "www.vrchat.com"))
            {
                var path = uri.AbsolutePath.Split('/');
                value = path.FirstOrDefault(part => WorldIdPattern.IsMatch(part)) ?? value;
            }
            return WorldIdPattern.IsMatch(value) ? value.ToLowerInvariant() : value;
        }

        private void RefreshUI()
        {
            if (!isWindowActive || worldList == null) return;
            boardField.SetValueWithoutNotify(worldsBoard);
            boardContent.SetEnabled(worldsBoard != null);
            if (worldsBoard == null) ShowStatus("シーンのWorldsBoardを選択するか、上の欄に指定してください。", BoardMessageKind.Info);
            RefreshWorldList();
            RefreshFavorites();
            UpdateButtons();
        }

        private void RefreshWorldList()
        {
            if (worldList == null) return;
            worldList.Clear();
            var ids = WorldIds;
            var visible = 0;
            for (var i = 0; i < ids.Length; i++)
            {
                if (!MatchesFilter(ids[i])) continue;
                visible++;
                var index = i;
                var id = ids[i];
                var cached = FindWorld(id);
                var row = BoardUI.Row();
                row.AddToClassList("cwb-world-row");
                row.userData = id;
                row.EnableInClassList("cwb-selected", selectedWorldId == id);
                void SelectRow()
                {
                    selectedWorldId = id;
                    foreach (var child in worldList.Children()) child.EnableInClassList("cwb-selected", Equals(child.userData, id));
                }
                row.RegisterCallback<PointerDownEvent>(_ => SelectRow());
                row.RegisterCallback<FocusInEvent>(_ => SelectRow());
                row.Add(BoardUI.Text((i + 1).ToString("00"), "cwb-index"));
                row.Add(CreateThumbnail(cached));
                var details = new VisualElement();
                details.AddToClassList("cwb-grow");
                details.AddToClassList("cwb-world-details");
                details.Add(BoardUI.Text(cached?.Name ?? "ワールド情報 未取得", "cwb-world-name"));
                details.Add(BoardUI.Text(!WorldIdPattern.IsMatch(id ?? "") ? "ワールドIDの形式を確認してください" : cached == null ? "-" : $"{cached.AuthorName}  ·  定員 {cached.Capacity} 人", "cwb-meta"));
                var field = new TextField { value = id ?? "", isDelayed = true, tooltip = "ワールドIDを編集（Enterまたはフォーカス移動で確定）" };
                field.RegisterValueChangedCallback(e =>
                {
                    var edited = NormalizeWorldId(e.newValue);
                    if (!WorldIdPattern.IsMatch(edited) || ids.Where((_, n) => n != index).Contains(edited))
                    {
                        field.SetValueWithoutNotify(id ?? "");
                        ShowStatus("有効で、他の行と重複しないワールドIDを入力してください。", BoardMessageKind.Warning);
                        return;
                    }
                    SetWorldId(index, edited);
                });
                details.Add(field);
                row.Add(details);
                var actions = new VisualElement();
                actions.AddToClassList("cwb-actions");
                var up = BoardUI.Button("↑", () => MoveWorld(index, index - 1));
                up.tooltip = "上へ移動";
                up.SetEnabled(index > 0);
                actions.Add(up);
                var down = BoardUI.Button("↓", () => MoveWorld(index, index + 1));
                down.tooltip = "下へ移動";
                down.SetEnabled(index < ids.Length - 1);
                actions.Add(down);
                var fetch = BoardUI.Button("", () => GetWorldAsync(id).Forget(), "cwb-icon-button", "refresh");
                fetch.tooltip = "ワールド情報を取得";
                fetch.SetEnabled(!isFetchingWorlds && NeedsWorldInfo(id) && WorldIdPattern.IsMatch(id ?? ""));
                actions.Add(fetch);
                var remove = BoardUI.Button("", () => RemoveWorldAt(index), "cwb-icon-button", "delete");
                remove.AddToClassList("cwb-danger");
                remove.tooltip = "ワールドを削除";
                actions.Add(remove);
                row.Add(actions);
                worldList.Add(row);
            }
            listTitle.text = string.IsNullOrWhiteSpace(filterText) ? $"ワールド一覧 · {ids.Length} 件" : $"ワールド一覧 · {visible} / {ids.Length} 件";
            if (visible == 0) worldList.Add(BoardUI.Text(ids.Length == 0 ? "ワールドを追加して、ボードを作りましょう。" : "検索に一致するワールドがありません。", "cwb-empty"));
        }

        private void RefreshFavorites()
        {
            favoriteList.Clear();
            groupPicker.choices = Enumerable.Range(0, favoriteWorldGroupNames?.Length ?? 0).ToList();
            if (groupPicker.choices.Count > 0)
            {
                selectedFavoriteWorldGroupIndex = Mathf.Clamp(selectedFavoriteWorldGroupIndex, 0, groupPicker.choices.Count - 1);
                groupPicker.SetValueWithoutNotify(selectedFavoriteWorldGroupIndex);
            }
            else groupPicker.SetValueWithoutNotify(-1);
            foreach (var favorite in favorites)
            {
                var row = BoardUI.Row();
                row.AddToClassList("cwb-world-row");
                row.Add(CreateThumbnail(favorite));
                var details = new VisualElement();
                details.AddToClassList("cwb-grow");
                details.AddToClassList("cwb-world-details");
                details.Add(BoardUI.Text(favorite.Name, "cwb-world-name"));
                details.Add(BoardUI.Text(favorite.AuthorName, "cwb-meta"));
                details.tooltip = favorite.ID;
                row.Add(details);
                var added = WorldIds.Contains(favorite.ID);
                var button = BoardUI.Button(added ? "追加済み" : "追加", () => AddWorld(favorite.ID));
                button.SetEnabled(!added);
                row.Add(button);
                favoriteList.Add(row);
            }
            if (favorites.Count == 0) favoriteList.Add(BoardUI.Text("グループを選んで、お気に入りを読み込んでください。", "cwb-empty"));
        }

        private VisualElement CreateThumbnail(WorldCache world)
        {
            var image = new Image { scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
            image.style.display = DisplayStyle.None;
            var placeholder = BoardUI.Icon("world");
            var retry = false;
            var button = new Button { focusable = false };
            button.AddToClassList("cwb-thumbnail");
            button.Add(placeholder);
            button.Add(image);
            var url = world?.ThumbnailImageUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                button.tooltip = "サムネイル未取得。「情報を取得」でワールド情報を読み込めます。";
                return button;
            }
            var cache = thumbnailCache;
            async UniTask LoadThumbnailAsync(bool forceRetry)
            {
                if (cache == null) return;
                retry = false;
                button.focusable = false;
                button.tooltip = "サムネイルを読み込み中…";
                try
                {
                    var texture = await cache.GetAsync(url, forceRetry);
                    if (!isWindowActive || cache != thumbnailCache || texture == null) return;
                    image.image = texture;
                    image.style.display = DisplayStyle.Flex;
                    placeholder.style.display = DisplayStyle.None;
                    button.tooltip = world.Name;
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    if (!isWindowActive || cache != thumbnailCache) return;
                    retry = true;
                    button.focusable = true;
                    button.tooltip = "サムネイルを取得できませんでした。クリックして再試行。\n" + ex.Message;
                }
            }
            button.clicked += () =>
            {
                if (retry) LoadThumbnailAsync(true).Forget();
            };
            LoadThumbnailAsync(false).Forget();
            return button;
        }

        private string FavoriteGroupLabel(int index)
        {
            return favoriteWorldGroupNames != null && index >= 0 && index < favoriteWorldGroupNames.Length
                ? favoriteWorldGroupNames[index] : "グループを選択";
        }

        private void UpdateButtons()
        {
            if (addButton == null) return;
            addButton.SetEnabled(!string.IsNullOrWhiteSpace(newWorldId));
            BoardUI.SetButtonText(fetchButton, isFetchingWorlds ? "取得中…" : "情報を取得");
            fetchButton.SetEnabled(!isFetchingWorlds && WorldIds.Any(id => WorldIdPattern.IsMatch(id ?? "") && NeedsWorldInfo(id)));
            groupsButton.tooltip = isFetchingFavoriteWorldGroups ? "グループ取得中…" : "お気に入りグループを取得・更新";
            groupsButton.SetEnabled(!isFetchingFavoriteWorldGroups && !isFetchingFavoritedWorlds);
            groupPicker.SetEnabled(!isFetchingFavoriteWorldGroups && !isFetchingFavoritedWorlds && groupPicker.choices.Count > 0);
            BoardUI.SetButtonText(favoritesButton, isFetchingFavoritedWorlds ? "取得中…" : "ワールドを取得");
            favoritesButton.SetEnabled(!isFetchingFavoriteWorldGroups && !isFetchingFavoritedWorlds && groupPicker.choices.Count > 0);
        }

        private void ShowStatus(string message, BoardMessageKind type)
        {
            if (!isWindowActive || status == null) return;
            status.text = message;
            status.messageType = type;
            status.style.display = DisplayStyle.Flex;
        }
        private async UniTask GetWorldsAsync()
        {
            if (isFetchingWorlds || !isWindowActive) return;
            isFetchingWorlds = true;
            RefreshUI();
            ShowStatus("ワールド情報を取得しています…", BoardMessageKind.Info);
            var progressId = UnityEditor.Progress.Start("ワールド情報を取得中...");
            try
            {
                var worldIds = WorldIds
                    .Where(id => WorldIdPattern.IsMatch(id ?? "") && NeedsWorldInfo(id))
                    .Distinct()
                    .ToArray();

                var failures = 0;
                for (var i = 0; i < worldIds.Length; i++)
                {
                    if (!isWindowActive) break;
                    try { await FetchAndCacheWorld(worldIds[i]); }
                    catch (Exception ex)
                    {
                        failures++;
                        Debug.LogError($"{worldIds[i]} の取得に失敗しました: {ex.Message}");
                    }
                    UnityEditor.Progress.Report(progressId, (float)(i + 1) / worldIds.Length, $"ワールド情報を取得中... ({i + 1}/{worldIds.Length})");
                    RefreshUI();
                }
                ShowStatus(failures == 0 ? "ワールド情報を取得しました。" : $"{failures} 件の取得に失敗しました。SDKのログイン状態を確認し、再度取得してください。詳細はConsoleで確認できます。",
                    failures == 0 ? BoardMessageKind.Info : BoardMessageKind.Error);
            }
            catch (Exception ex)
            {
                ShowStatus($"ワールド情報の取得に失敗しました: {ex.Message}", BoardMessageKind.Error); Debug.LogException(ex);
            }
            finally
            {
                UnityEditor.Progress.Remove(progressId);
                isFetchingWorlds = false;
                RefreshUI();
            }
        }

        private async UniTask GetWorldAsync(string worldId)
        {
            if (isFetchingWorlds || !isWindowActive) return;
            isFetchingWorlds = true;
            RefreshUI();
            try
            {
                await FetchAndCacheWorld(worldId);
                ShowStatus("ワールド情報を取得しました。", BoardMessageKind.Info);
            }
            catch (Exception ex)
            {
                ShowStatus($"ワールド情報の取得に失敗しました: {ex.Message}", BoardMessageKind.Error); Debug.LogException(ex);
            }
            finally
            {
                isFetchingWorlds = false;
                RefreshUI();
            }
        }

        private async UniTask FetchFavoriteWorldGroupNamesAsync()
        {
            if (isFetchingFavoriteWorldGroups || isFetchingFavoritedWorlds || !isWindowActive) return;
            isFetchingFavoriteWorldGroups = true;
            UpdateButtons();
            try
            {
                var favoriteWorldGroups = await WorldsApi.GetFavoriteWorldsList(forceRefresh: true);
                favoriteWorldGroupNames = (favoriteWorldGroups.Groups ?? new List<FavoriteGroup>()).ConvertAll(g => string.IsNullOrWhiteSpace(g.DisplayName) ? g.Name : g.DisplayName).ToArray();
                favoriteWorldGroupApiNames = (favoriteWorldGroups.Groups ?? new List<FavoriteGroup>()).ConvertAll(g => g.Name).ToArray();
                selectedFavoriteWorldGroupIndex = 0;
                favorites.Clear();
                ShowStatus(favoriteWorldGroupNames.Length == 0 ? "お気に入りグループがありません。" : "グループを選んで「ワールドを取得」を押してください。", BoardMessageKind.Info);
            }
            catch (Exception ex)
            {
                ShowStatus($"お気に入りグループを取得できませんでした: {ex.Message}", BoardMessageKind.Error); Debug.LogException(ex);
                favoriteWorldGroupNames = null;
                favoriteWorldGroupApiNames = null;
            }
            finally
            {
                isFetchingFavoriteWorldGroups = false;
                RefreshUI();
            }
        }

        private async UniTask FetchFavoritedWorldsAsync()
        {
            if (isFetchingFavoriteWorldGroups || isFetchingFavoritedWorlds || !isWindowActive) return;
            isFetchingFavoritedWorlds = true;
            UpdateButtons();
            try
            {
                if (favoriteWorldGroupApiNames == null || selectedFavoriteWorldGroupIndex < 0 || selectedFavoriteWorldGroupIndex >= favoriteWorldGroupApiNames.Length)
                {
                    Debug.LogError("無効なお気に入りワールドグループが選択されています。");
                    return;
                }

                var favoriteWorlds = await WorldsApi.GetFavoriteWorlds(favoriteWorldGroupApiNames[selectedFavoriteWorldGroupIndex], forceRefresh: true);
                favorites = (favoriteWorlds.Favorites ?? new List<FavoritedWorld>())
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
                ShowStatus($"お気に入りを取得できませんでした: {ex.Message}", BoardMessageKind.Error); Debug.LogException(ex);
                favorites.Clear();
            }
            finally
            {
                isFetchingFavoritedWorlds = false;
                RefreshUI();
            }
        }

        private async UniTask FetchAndCacheWorld(string worldId)
        {
            var world = await VRCApi.GetWorld(worldId);
            if (isWindowActive) CacheWorld(WorldCache.FromWorld(world));
        }

        private void AddWorld(string worldId)
        {
            if (string.IsNullOrWhiteSpace(worldId)) return;

            var worldIds = WorldIds.ToList();
            if (worldIds.Contains(worldId)) return;

            RecordBoard("Add world");
            worldIds.Add(worldId);
            ApplyWorldIds(worldIds);
        }

        private void SetWorldId(int index, string worldId)
        {
            if (!IsValidIndex(index)) return;

            var worldIds = WorldIds.ToArray();
            RecordBoard("Edit world ID");
            worldIds[index] = worldId;
            ApplyWorldIds(worldIds);
        }

        private void RemoveWorldAt(int index)
        {
            if (!IsValidIndex(index)) return;

            var worldIds = WorldIds.ToList();
            RecordBoard("Remove world");
            worldIds.RemoveAt(index);
            ApplyWorldIds(worldIds);
        }

        private void MoveWorld(int fromIndex, int toIndex)
        {
            if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex)) return;

            var worldIds = WorldIds.ToList();
            var worldId = worldIds[fromIndex];
            RecordBoard("Reorder worlds");
            worldIds.RemoveAt(fromIndex);
            worldIds.Insert(toIndex, worldId);
            ApplyWorldIds(worldIds);
        }

        private void SetBoard(WorldsBoard board)
        {
            worldsBoard = board;
            RebuildSettings();
            if (status != null) status.style.display = DisplayStyle.None;
            SyncCacheWithBoard();
            RefreshUI();
        }

        private void ApplyWorldIds(IEnumerable<string> worldIds)
        {
            if (worldsBoard == null) return;

            worldsBoard.worldIds = worldIds.ToArray();
            PrefabUtility.RecordPrefabInstancePropertyModifications(worldsBoard);
            EditorUtility.SetDirty(worldsBoard);
            RefreshUI();
        }

        private void SyncCacheWithBoard()
        {
            if (worlds == null) worlds = new List<WorldCache>();
            if (favorites == null) favorites = new List<WorldCache>();
            worlds.RemoveAll(w => w == null || string.IsNullOrEmpty(w.ID));
        }

        private void CacheWorld(WorldCache world)
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

        private WorldCache FindWorld(string worldId)
        {
            if (string.IsNullOrEmpty(worldId)) return null;
            return worlds.FirstOrDefault(w => w.ID == worldId);
        }

        private bool NeedsWorldInfo(string worldId)
        {
            var cached = FindWorld(worldId);
            return cached == null || string.IsNullOrEmpty(cached.ThumbnailImageUrl);
        }

        private bool MatchesFilter(string worldId)
        {
            if (string.IsNullOrWhiteSpace(filterText)) return true;

            var cachedWorld = FindWorld(worldId);
            return Contains(worldId, filterText)
                   || Contains(cachedWorld?.Name, filterText)
                   || Contains(cachedWorld?.AuthorName, filterText);
        }

        private static bool Contains(string value, string search)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < WorldIds.Length;
        }

        private void RecordBoard(string operation)
        {
            Undo.RecordObject(worldsBoard, operation);
        }

        private string[] WorldIds => worldsBoard?.worldIds ?? new string[0];

        private static WorldsBoard GetSelectedBoard()
        {
            return Selection.activeGameObject == null ? null : Selection.activeGameObject.GetComponent<WorldsBoard>();
        }

        [Serializable]
        private class WorldCache
        {
            public string ID;
            public string Name;
            public string AuthorName;
            public int Capacity;
            public string ThumbnailImageUrl;

            public static WorldCache FromWorld(VRCWorld world)
            {
                return new WorldCache
                {
                    ID = world.ID,
                    Name = world.Name,
                    AuthorName = world.AuthorName,
                    Capacity = world.Capacity,
                    ThumbnailImageUrl = string.IsNullOrWhiteSpace(world.ThumbnailImageUrl) ? world.ImageUrl : world.ThumbnailImageUrl
                };
            }
        }
    }
}

