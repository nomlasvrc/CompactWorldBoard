
using System;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;

namespace Nomlas.CompactWorldBoard.Editor
{
    public class VRCSDKBuildProcess : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => int.MinValue;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            try
            {
                if (!VPMUpdateCheckCache.IsCompleted)
                {
                    throw new Exception("Update check has not completed yet. Please wait a moment and retry build.");
                }

                if (VPMUpdateCheckCache.LastError != null)
                {
                    throw new Exception($"Startup update check failed: {VPMUpdateCheckCache.LastError.Message}", VPMUpdateCheckCache.LastError);
                }

                var result = VPMUpdateCheckCache.Result;
                if (result == null)
                {
                    throw new Exception("Update check cache is empty.");
                }

                if (result.HasUpdate)
                {
                    EditorUtility.DisplayDialog("アップデートが利用可能です", $"CompactWorldBoardのアップデートが利用可能です\n{result.CurrentVersion} -> {result.LatestVersion}\n\n「OK」をクリックしてビルドを終了します", "OK");
                    return false; // Cancel the build
                }

                Debug.Log("Already up to date.");
                return true; // Proceed with the build
            }
            catch (Exception ex)
            {
                Debug.LogError($"Update check failed. Build is canceled. Reason: {ex}");
                EditorUtility.DisplayDialog("更新チェックエラー", $"更新チェック中にエラーが発生したためビルドを中止しました。\n\n{ex.Message}", "OK");
                return false; // Fail the build on any error
            }
        }
    }
}