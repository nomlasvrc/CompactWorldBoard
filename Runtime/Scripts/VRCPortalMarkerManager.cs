
using System.Text.RegularExpressions;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Nomlas.CompactWorldBoard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VRCPortalMarkerManager : UdonSharpBehaviour
    {
        public string WorldText => worldText;
        public string OwnerText => ownerText;
        public string AccessText => accessText;
        public string GroupText => groupText;
        public string AgeGateText => ageGateText;
        public int PlayerCount => playerCount;
        public int MaxPlayerCount => maxPlayerCount;
        public string Timer => timer;

        [Header("ボタンテキスト")][SerializeField] private TMP_Text buttonText;
        [Header("ポータルセレクターの親")][SerializeField] private RectTransform selectorParent;
        [Header("ポータルセレクターのメッシュと置き換えるCubeメッシュ")][SerializeField] private Mesh cubeMesh;
        [Header("ワールドサムネイルを表示するRawImage")][SerializeField] private RawImage image;

        private Transform portalInternal;

        public const string PortalInternalPath = "PortalInternal(Clone)";
        public const string PortalSelectorPath = "PortalSelector";
        public const string PortalPlanePath = "Portal Cosmetics Handler/Portal(Clone)/Base Graphics/Portal Plane";
        public const string WorldTex = "_WorldTex";

        private const string Prefix = "[<color=green>VRCPortalMarkerManager</color>] ";
        private const string accessPattern = @"^(\S)\s(#\S+)\s{2}(.+)$";

        public void SetPortal(GameObject newPortal)
        {
            portalInternal = newPortal.transform.Find(PortalInternalPath);
            if (portalInternal == null)
            {
                Debug.LogError($"{Prefix}{PortalInternalPath}が見つかりませんでした。");
                return;
            }

            SendCustomEventDelayedSeconds(nameof(ModifyPortalSelector), 1);
        }

        public void ModifyPortalSelector()
        {
            if (!Utilities.IsValid(portalInternal)) return;

            var selector = portalInternal.Find(PortalSelectorPath);
            if (selector == null)
            {
                Debug.LogWarning($"{Prefix}{PortalSelectorPath}が見つかりませんでした。");
            }
            else
            {
                selector.SetPositionAndRotation(selectorParent.position, selectorParent.rotation);
                selector.localScale = selectorParent.lossyScale;
                selector.GetComponent<MeshFilter>().mesh = cubeMesh;
                selector.GetComponent<BoxCollider>().size = Vector3.one;
                selector.GetComponent<BoxCollider>().center = Vector3.zero;
            }

            tryCount = 0;
            UpdatePortalData();
        }

        private string worldText;
        private string ownerText;
        private string accessText;
        private string groupText;
        private string ageGateText;
        private string playerCountText;
        private int playerCount;
        private int maxPlayerCount;
        private string timer;
        public void UpdatePortalData()
        {
            if (!Utilities.IsValid(portalInternal)) return;

            tryCount++;
            if (!TryUpdatePortalData())
            {
                if (tryCount >= 10)
                {
                    Debug.LogWarning($"{Prefix}ポータルデータの更新に失敗しました。");
                    return;
                }
                SendCustomEventDelayedSeconds(nameof(UpdatePortalData), 1);
            }
        }
        private int tryCount = 0;

        private bool TryUpdatePortalData()
        {
            var canvasRoot = portalInternal.Find("Canvas");
            if (canvasRoot != null)
            {
                worldText = canvasRoot.FindText("WorldText", "[world]");
                ownerText = canvasRoot.FindText("OwnerText", "[owner]");
                accessText = canvasRoot.FindText("AccessText", "[access]");
                groupText = canvasRoot.FindText("GroupText", "[group]");
                ageGateText = canvasRoot.FindText("AgeGateText", "[ageGate]");
                playerCountText = canvasRoot.FindText("PlayerCount", "[playerCount]");
                timer = canvasRoot.FindText("Timer");

                if (string.IsNullOrWhiteSpace(worldText)) return false;

                // --- 整形 ---
                string instance = null;
                if (!string.IsNullOrWhiteSpace(accessText))
                {
                    Match m = Regex.Match(accessText,accessPattern);
                    if (m.Success && m.Groups.Count == 4)
                    {
                        //string region = m.Groups[1].Value;
                        instance = m.Groups[2].Value;
                        //string type = m.Groups[3].Value;
                    }
                }
                if (!string.IsNullOrWhiteSpace(playerCountText))
                {
                    var parts = playerCountText.Split(" / ");
                    if (parts.Length == 2)
                    {
                        int.TryParse(parts[0].Trim(), out playerCount);
                        int.TryParse(parts[1].Trim(), out maxPlayerCount);
                    }
                }

                switch (boardType)
                {
                    case BoardType.Worlds:
                        buttonText.text = worldText;
                        break;
                    case BoardType.Instances:
                        buttonText.text = string.IsNullOrEmpty(instance) ? accessText : instance;
                        /*
                        if (playerCount == 0)
                        {
                            buttonText.text = $"<color=#ff5e5e>{instance}</color> {playerCount}/{maxPlayerCount}({playerCountText})";
                        }
                        else
                        {
                            buttonText.text = $"<color=#a4e38f>{instance}</color> {playerCount}/{maxPlayerCount}";
                        }
                        */
                        break;
                }
            }

            var portalPlane = portalInternal.Find(PortalPlanePath);
            if (portalPlane == null) return false;

            var meshRenderer = portalPlane.GetComponent<MeshRenderer>();
            if (meshRenderer == null) return false;

            var sharedMaterial = meshRenderer.sharedMaterial;
            if (sharedMaterial == null) return false;

            var worldTex = sharedMaterial.GetTexture(WorldTex);
            if (worldTex == null) return false;

            image.texture = worldTex;
            return true;
        }

        private BoardType boardType;
        public void SetBoardType(BoardType boardType) => this.boardType = boardType;
    }
}