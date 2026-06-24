
using UnityEngine;

namespace Nomlas.CompactWorldBoard
{
    public abstract class Board : PortalGenerable
    {
        [SerializeField] public UserOrGroup userOrGroup;
        [SerializeField] public InstanceType instanceType;
        [SerializeField] public string userId;
        [SerializeField] public GroupType groupType;
        [SerializeField] public string groupId;
        [SerializeField] public Region region = Region.jp;

        private protected void CreateAndSetPortal(GameObject newPortal)
        {
            GameObject child = GameObject.Instantiate(content, contentParent);
            child.SetActive(true);
            var portalManager = child.GetComponent<VRCPortalMarkerManager>();
            portalManager.SetBoardType(BoardType);
            portalManager.SetPortal(newPortal);
        }
    }
}