
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

        public override void Create()
        {
            foreach (var instanceId in instanceIds)
            {
                GameObject child = GameObject.Instantiate(content, contentParent);
                child.SetActive(true);
                var portalManager = child.GetComponent<VRCPortalMarkerManager>();
                portalManager.SetBoardType(BoardType);
                portalManager.SetPortal(_NewPortal(instanceId));
            }
        }

        private GameObject _NewPortal(string instanceId)
        {
            switch (userOrGroup)
            {
                case UserOrGroup.Public:
                    return NewPortal(worldId, instanceId, region);
                case UserOrGroup.User:
                    return NewPortal(worldId, instanceId, userId, instanceType, region);
                case UserOrGroup.Group:
                    return NewPortal(worldId, instanceId, groupId, groupType, region);
                default:
                    Debug.LogError("ユーザーまたはグループのタイプが不正です。");
                    return null;
            }
        }
    }
}