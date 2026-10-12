using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nomlas.CompactWorldBoard.Editor
{
    internal enum BoardMessageKind { Info, Warning, Error }

    internal sealed class BoardStatusLine : VisualElement
    {
        private readonly Label label;
        internal string text { get => label.text; set => label.text = value; }
        internal BoardMessageKind messageType
        {
            set
            {
                EnableInClassList("cwb-warning", value == BoardMessageKind.Warning);
                EnableInClassList("cwb-error", value == BoardMessageKind.Error);
            }
        }
        internal BoardStatusLine(string message = "")
        {
            AddToClassList("cwb-status");
            Add(BoardUI.Icon("info"));
            label = BoardUI.Text(message, "cwb-status-text");
            Add(label);
        }
    }

    internal static class BoardUI
    {
        internal static void ApplyTheme(VisualElement root)
        {
            root.AddToClassList("cwb");
            root.EnableInClassList("cwb-dark", EditorGUIUtility.isProSkin);
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(VPMVersionChecker.PackagePath + "/Editor/BoardUI.uss");
            if (sheet != null && !root.styleSheets.Contains(sheet)) root.styleSheets.Add(sheet);
        }

        internal static VisualElement Row()
        {
            var row = new VisualElement();
            row.AddToClassList("cwb-row");
            return row;
        }

        internal static VisualElement Card(VisualElement parent, string title, string description = null)
        {
            var section = new VisualElement();
            section.AddToClassList("cwb-section");
            section.Add(Text(title, "cwb-section-title"));
            if (description != null) section.Add(Text(description, "cwb-muted"));
            parent.Add(section);
            return section;
        }

        internal static Label Text(string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }

        internal static Button Button(string text, Action action, string className = null, string icon = null)
        {
            var button = new Button(action) { tooltip = text };
            button.AddToClassList("cwb-button");
            if (className != null) button.AddToClassList(className);
            if (icon == null)
            {
                switch (text)
                {
                    case "↑": icon = "up"; break;
                    case "↓": icon = "down"; break;
                    case "削除": icon = "delete"; break;
                    case "追加": icon = "add"; break;
                    case "追加済み": icon = "check"; break;
                    default: icon = text.Contains("取得") ? "refresh" : "open"; break;
                }
            }
            button.Add(Icon(icon));
            var label = new Label(text) { name = "cwb-button-label", pickingMode = PickingMode.Ignore };
            if (text == "↑" || text == "↓" || text.Length == 0)
            {
                label.style.display = DisplayStyle.None;
                button.AddToClassList("cwb-icon-button");
            }
            button.Add(label);
            return button;
        }

        internal static void SetButtonText(Button button, string text)
        {
            button.Q<Label>("cwb-button-label").text = text;
        }

        internal static VisualElement Icon(string kind)
        {
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("cwb-icon");
            icon.generateVisualContent += context =>
            {
                var p = context.painter2D;
                p.lineWidth = 1.3f;
                p.strokeColor = icon.resolvedStyle.color;
                p.BeginPath();
                void Line(params float[] xy)
                {
                    p.MoveTo(new Vector2(xy[0], xy[1]));
                    for (var i = 2; i < xy.Length; i += 2) p.LineTo(new Vector2(xy[i], xy[i + 1]));
                }
                void Circle(float x, float y, float rx, float ry)
                {
                    p.MoveTo(new Vector2(x + rx, y));
                    for (var i = 1; i <= 24; i++)
                    {
                        var a = i * Mathf.PI * 2 / 24;
                        p.LineTo(new Vector2(x + Mathf.Cos(a) * rx, y + Mathf.Sin(a) * ry));
                    }
                }
                switch (kind)
                {
                    case "add": Line(8,3,8,13); Line(3,8,13,8); break;
                    case "up": Line(4,9,8,5,12,9); break;
                    case "down": Line(4,6,8,10,12,6); break;
                    case "delete": Line(3,4,13,4); Line(6,4,6,2,10,2,10,4); Line(4,5,5,14,11,14,12,5); Line(7,6,7,12); Line(9,6,9,12); break;
                    case "open": Line(9,2,14,2,14,7); Line(14,2,7,9); Line(7,3,3,3,3,13,13,13,13,9); break;
                    case "refresh": Line(13,7,13,4,10,2,6,2,3,5,3,10,6,13,10,13,13,10); Line(10,7,13,7,15,4); break;
                    case "bookmark": Line(4,2,12,2,12,14,8,11,4,14,4,2); break;
                    case "check": Line(3,8,6,11,13,4); break;
                    case "settings": Circle(8,8,3,3); Circle(8,8,6,6); Line(8,0,8,3); Line(8,13,8,16); Line(0,8,3,8); Line(13,8,16,8); break;
                    case "world": Circle(8,8,6,6); Circle(8,8,3,6); Line(2,8,14,8); break;
                    case "list": Line(5,4,14,4); Line(5,8,14,8); Line(5,12,14,12); Line(2,4,3,4); Line(2,8,3,8); Line(2,12,3,12); break;
                    default: Circle(8,8,6,6); Line(8,7,8,11); Line(8,4,8,5); break;
                }
                p.Stroke();
            };
            return icon;
        }

        internal static VisualElement VersionBadge()
        {
            var badge = Row();
            badge.AddToClassList("cwb-version");
            var icon = Icon("info");
            var label = Text("", "cwb-version-text");
            badge.Add(icon);
            badge.Add(label);
            var version = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(VPMVersionChecker.PackagePath)?.version ?? "?";
            void Update()
            {
                var result = VPMUpdateCheckCache.Result;
                label.text = $"v{version}";
                badge.focusable = result != null && result.HasUpdate;
                badge.EnableInClassList("cwb-update-available", result != null && result.HasUpdate);
                if (!VPMUpdateCheckCache.IsCompleted) badge.tooltip = "更新を確認しています";
                else if (VPMUpdateCheckCache.LastError != null) badge.tooltip = "更新を確認できませんでした。編集は引き続き行えます。\n" + VPMUpdateCheckCache.LastError.Message;
                else if (result != null && result.HasUpdate)
                {
                    label.text += $" · v{result.LatestVersion} 利用可能";
                    badge.tooltip = "パッケージ一覧を開く";
                }
                else badge.tooltip = "最新バージョンです";
            }
            Update();
            badge.RegisterCallback<ClickEvent>(_ =>
            {
                if (VPMUpdateCheckCache.Result?.HasUpdate == true) Application.OpenURL("https://nomlasvrc.github.io/nomlas-package-listing/");
            });
            badge.RegisterCallback<KeyDownEvent>(e =>
            {
                if (badge.focusable && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Space))
                {
                    Application.OpenURL("https://nomlasvrc.github.io/nomlas-package-listing/");
                    e.StopPropagation();
                }
            });
            IVisualElementScheduledItem poll = null;
            poll = badge.schedule.Execute(() => { Update(); if (VPMUpdateCheckCache.IsCompleted) poll.Pause(); }).Every(500);
            return badge;
        }
    }
}
