
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

        public override void Create()
        {
            foreach (var worldId in worldIds)
            {
                var newPortal = _NewPortal(worldId);
                CreateAndSetPortal(newPortal);
            }
        }

        private GameObject _NewPortal(string worldId)
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