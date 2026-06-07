
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using VRC.SDKBase.Editor.Api;

namespace Nomlas.CompactWorldBoard.Editor.Api
{
    public static class WorldsApi
    {
        [PublicAPI]
        public static async Task<FavoriteGroups> GetFavoriteWorldsList(bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            return await VRCApi.Get<FavoriteGroups>($"favorites/groups/world", forceRefresh: forceRefresh, cancellationToken: cancellationToken);
        }

        [PublicAPI]
        public static async Task<FavoritedWorlds> GetFavoriteWorlds(string name, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            return await VRCApi.Get<FavoritedWorlds>($"favorites/groups/world/{name}", forceRefresh: forceRefresh, cancellationToken: cancellationToken);
        }
    }
}
