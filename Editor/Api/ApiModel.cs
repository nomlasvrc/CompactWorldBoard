using System.Collections.Generic;
using Newtonsoft.Json;
using VRC.SDKBase.Editor.Api;

namespace Nomlas.CompactWorldBoard.Editor.Api
{
    public struct FavoriteGroups
    {
        [JsonProperty("favoriteGroups")]
        public List<FavoriteGroup> Groups { get; set; }
        public int MaxFavoriteGroups { get; set; }
        public int MaxFavoritesPerGroup { get; set; }
    }
    public struct FavoriteGroup : IVRCContent
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string OwnerId { get; set; }
        public string OwnerDisplayName { get; set; }
        public List<string> Tags { get; set; }
        public string Type { get; set; }
        public string Visibility { get; set; }
    }

    public struct FavoritedWorlds
    {
        public List<FavoritedWorld> Favorites { get; set; }
        public int TotalCount { get; set; }
    }

    public struct FavoritedWorld
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        public string FavoriteId { get; set; }
        public List<string> Tags { get; set; }
        public string Type { get; set; }
        public VRCWorld world { get; set; }
    }
}
