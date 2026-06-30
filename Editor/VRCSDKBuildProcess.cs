
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Nomlas.CompactWorldBoard.Editor
{
    public class VRCSDKBuildProcess : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => int.MinValue;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            var result = VPMVersionChecker.CheckForUpdateAsync(Application.version).GetAwaiter().GetResult();

            if (result.HasUpdate)
            {
                UnityEditor.EditorUtility.DisplayDialog("アップデートが利用可能です", $"CompactWorldBoardのアップデートが利用可能です\n{result.CurrentVersion} -> {result.LatestVersion}\n\n「OK」をクリックしてビルドを終了します", "OK");
                return false; // Cancel the build
            }
            else
            {
                Debug.Log("Already up to date.");
                return true; // Proceed with the build
            }
        }
    }

    public static class VPMVersionChecker
    {
        private const string VPMJSONURL = "https://nomlasvrc.github.io/CompactWorldBoard/index.json";
        private const string PackageID = "com.nomlas.compactworldboard";
        public static async UniTask<UpdateCheckResult> CheckForUpdateAsync(string currentVersion)
        {
            using var req = UnityWebRequest.Get(VPMJSONURL);
            await req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"Failed to download VPM index: {req.error}");
            }

            var repository = JsonConvert.DeserializeObject<VPMRepository>(req.downloadHandler.text);

            if (!repository.Packages.TryGetValue(PackageID, out var package))
            {
                throw new Exception($"Package '{PackageID}' not found.");
            }

            var latest = package.Versions.Values
                .Where(v => !v.Version.Contains('-')) // Stable only
                .OrderByDescending(v => Version.Parse(v.Version))
                .First();

            return new UpdateCheckResult
            {
                CurrentVersion = currentVersion,
                LatestVersion = latest.Version,
                HasUpdate = Version.Parse(latest.Version) > Version.Parse(currentVersion)
            };
        }
    }

    public class UpdateCheckResult
    {
        public bool HasUpdate;
        public string CurrentVersion;
        public string LatestVersion;
    }

    public class VPMRepository
    {
        [JsonProperty("packages")]
        public Dictionary<string, VpmPackage> Packages;
    }

    public class VpmPackage
    {
        [JsonProperty("versions")]
        public Dictionary<string, VpmVersion> Versions;
    }

    public class VpmVersion
    {
        [JsonProperty("version")]
        public string Version;
    }
}