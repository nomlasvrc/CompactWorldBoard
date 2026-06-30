
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;

namespace Nomlas.CompactWorldBoard.Editor
{
    [InitializeOnLoad]
    public static class VPMUpdateCheckCache
    {
        public static bool IsCompleted { get; private set; }
        public static UpdateCheckResult Result { get; private set; }
        public static Exception LastError { get; private set; }

        static VPMUpdateCheckCache()
        {
            RefreshAsync().Forget();
        }

        private static async UniTaskVoid RefreshAsync()
        {
            try
            {
                Result = null;
                LastError = null;

                var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(VPMVersionChecker.PackagePath);
                if (package == null || string.IsNullOrWhiteSpace(package.version))
                {
                    throw new Exception($"Package info not found for '{VPMVersionChecker.PackagePath}'.");
                }

                Result = await VPMVersionChecker.CheckForUpdateAsync(package.version);
            }
            catch (Exception ex)
            {
                LastError = ex;
                Debug.LogError($"Startup update check failed: {ex}");
            }
            finally
            {
                IsCompleted = true;
            }
        }
    }

    public static class VPMVersionChecker
    {
        private const string VPMJSONURL = "https://nomlasvrc.github.io/CompactWorldBoard/index.json";
        public const string PackageID = "com.nomlas.compactworldboard";
        public const string PackagePath = "Packages/" + PackageID;

        public static async UniTask<UpdateCheckResult> CheckForUpdateAsync(string currentVersion)
        {
            using var req = UnityWebRequest.Get(VPMJSONURL);
            req.timeout = 15;
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
        public Dictionary<string, VPMPackage> Packages;
    }

    public class VPMPackage
    {
        [JsonProperty("versions")]
        public Dictionary<string, VPMVersion> Versions;
    }

    public class VPMVersion
    {
        [JsonProperty("version")]
        public string Version;
    }
}