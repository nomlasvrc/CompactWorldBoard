
using UdonSharp;
using UnityEngine;

namespace Nomlas.CompactWorldBoard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class WorldsBoard : Board
    {
        public override BoardType BoardType => BoardType.Worlds;

        [SerializeField] public string[] worldIds;
        [SerializeField] public string instanceId;
        [SerializeField] public UserOrGroup userOrGroup;
        [Space]
        [SerializeField] public InstanceType instanceType;
        [SerializeField] public string userId;
        [Space]
        [SerializeField] public GroupType groupType;
        [SerializeField] public string groupId;
        [Space]
        [SerializeField] public Region region = Region.jp;

        public override void Create()
        {
            foreach (var worldId in worldIds)
            {
                GameObject child = GameObject.Instantiate(content, contentParent);
                child.SetActive(true);
                var portalManager = child.GetComponent<VRCPortalMarkerManager>();
                portalManager.SetBoardType(BoardType);
                var newPortal = _NewPortal(worldId);
                portalManager.SetPortal(newPortal);
            }
        }

        private GameObject _NewPortal(string worldId)
        {
            GameObject newPortal;
            if (userOrGroup == UserOrGroup.Public)
            {
                newPortal = NewPortal(worldId, instanceId, region);
            }
            else if (userOrGroup == UserOrGroup.User)
            {
                newPortal = NewPortal(worldId, instanceId, userId, instanceType, region);
            }
            else if (userOrGroup == UserOrGroup.Group)
            {
                newPortal = NewPortal(worldId, instanceId, groupId, groupType, region);
            }
            else
            {
                Debug.LogError("ユーザーまたはグループのタイプが不正です。");
                return null;
            }
            return newPortal;
        }
    }
}