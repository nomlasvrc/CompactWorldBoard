
using UdonSharp;
using UnityEngine;

namespace Nomlas.CompactWorldBoard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class InstancesBoard : Board
    {
        public override BoardType BoardType => BoardType.Instances;

        [SerializeField] public string worldId;
        [SerializeField] public string instanceIdPrefix;
        [SerializeField] public string instanceIdSuffix;
        [SerializeField] public int instanceCount = 8;
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
            for (int i = 0; i < instanceCount; i++)
            {
                GameObject child = GameObject.Instantiate(content, contentParent);
                child.SetActive(true);
                var portalManager = child.GetComponent<VRCPortalMarkerManager>();
                portalManager.SetBoardType(BoardType);
                var newPortal = _NewPortal(i, worldId);
                portalManager.SetPortal(newPortal);
            }
        }

        private GameObject _NewPortal(int instanceIndex, string worldId)
        {
            GameObject newPortal;
            char v = (char)('A' + instanceIndex);
            string instanceId = $"{instanceIdPrefix}{v}{instanceIdSuffix}";
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