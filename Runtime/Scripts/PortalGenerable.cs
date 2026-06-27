using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;

namespace Nomlas.CompactWorldBoard
{
    public abstract class PortalGenerable : UdonSharpBehaviour
    {
        public abstract BoardType BoardType { get; }

        public bool canEnter;
        [Space]
        [SerializeField] private GameObject portalMarkerPrefab;
        [SerializeField] private Transform portalParent;
        [SerializeField] private protected GameObject content;
        [SerializeField] private protected Transform contentParent;
        [SerializeField] private int createDelaySecondsMin = 5;
        [SerializeField] private int createDelaySecondsMax = 20;

        private void Start()
        {
            var random = Random.Range(createDelaySecondsMin, createDelaySecondsMax);
            Debug.Log($"[CompactWorldBoard] ポータルを {random} 秒後に作成します。");
            SendCustomEventDelayedSeconds(nameof(Create), random);
        }

        public abstract void Create();

        /// <summary>
        /// WorldIDのみから新しいポータルを作成します。
        /// </summary>
        private protected GameObject NewPortal(string worldId)
        {
            return GenerateNewPortal(worldId);
        }

        /// <summary>
        /// IDやリージョンなどを指定して新しいポータルを作成します。
        /// Publicインスタンスを作成します。
        /// </summary>
        private protected GameObject NewPortal(string worldId, string instanceId, Region region)
        {
            return GenerateNewPortal($"{Utils.FString("", worldId)}{Utils.FString(":", instanceId)}~{Utils.GetRegion(region)}");
        }

        /// <summary>
        /// IDやリージョンなどを指定して新しいポータルを作成します。
        /// Friend+からInviteインスタンスを作成します。
        /// </summary>
        private protected GameObject NewPortal(string worldId, string instanceId, string userId, InstanceType instanceType, Region region)
        {
            return GenerateNewPortal($"{Utils.FString("", worldId)}{Utils.FString(":", instanceId)}{Utils.GetInstanceTypeString(instanceType, userId)}~{Utils.GetRegion(region)}");
        }

        /// <summary>
        /// IDやリージョンなどを指定して新しいポータルを作成します。
        /// グループのインスタンスタイプとIDを指定します。
        /// </summary>
        private protected GameObject NewPortal(string worldId, string instanceId, string groupId, GroupType groupType, Region region)
        {
            return GenerateNewPortal($"{Utils.FString("", worldId)}{Utils.FString(":", instanceId)}{Utils.GetGroupTypeString(groupType, groupId)}~{Utils.GetRegion(region)}");
        }

        /// <summary>
        /// Room IDから新しいポータルを作成します。
        /// </summary>
        private protected GameObject GenerateNewPortal(string id)
        {
            VRCPortalMarker portal = portalMarkerPrefab.GetComponent<VRCPortalMarker>();
            portal.roomId = id;
            GameObject newPortal = GameObject.Instantiate(portalMarkerPrefab, portalParent);
            newPortal.GetComponent<VRCPortalMarker>().enabled = true;
            newPortal.SetActive(true);

            if (!canEnter)
            {
                Destroy(newPortal.GetComponent<BoxCollider>());
                var portalInternal = newPortal.transform.Find(VRCPortalMarkerManager.PortalInternalPath);
                if (portalInternal != null) Destroy(portalInternal.GetComponent<BoxCollider>());
            }
            return newPortal;
        }
    }
}