
using UdonSharp;
using UnityEngine;

namespace Nomlas.CompactWorldBoard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class InstancesBoard : Board
    {
        public override BoardType BoardType => BoardType.Instances;

        [SerializeField] public string worldId;
        [SerializeField] public string[] instanceIds;
        [Space]
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
            foreach (var instanceId in instanceIds)
            {
                GameObject child = GameObject.Instantiate(content, contentParent);
                child.SetActive(true);
                var portalManager = child.GetComponent<VRCPortalMarkerManager>();
                portalManager.SetBoardType(BoardType);
                switch (userOrGroup)
                {
                    case UserOrGroup.Public:
                        portalManager.SetPortal(NewPortal(worldId, instanceId, region));
                        break;
                    case UserOrGroup.User:
                        portalManager.SetPortal(NewPortal(worldId, instanceId, userId, instanceType, region));
                        break;
                    case UserOrGroup.Group:
                        portalManager.SetPortal(NewPortal(worldId, instanceId, groupId, groupType, region));
                        break;
                    default:
                        Debug.LogError("ユーザーまたはグループのタイプが不正です。");
                        break;
                }
            }
        }
    }
}