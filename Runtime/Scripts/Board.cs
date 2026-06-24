
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
    }
}