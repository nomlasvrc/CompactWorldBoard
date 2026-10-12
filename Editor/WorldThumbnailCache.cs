using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VRC.SDKBase.Editor.Api;

namespace Nomlas.CompactWorldBoard.Editor
{
    // Called on Unity's editor thread. SDK caches bytes; this cache owns the textures.
    internal sealed class WorldThumbnailCache : IDisposable
    {
        private sealed class Entry
        {
            internal Task<Texture2D> Request;
            internal Texture2D Texture;
        }

        private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>();
        private readonly SemaphoreSlim downloads = new SemaphoreSlim(4);
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        private readonly Func<string, CancellationToken, Task<Texture2D>> loadImage;
        private bool disposed;
        private bool cancellationCompleted;
        private int pendingRequests;

        internal WorldThumbnailCache(Func<string, CancellationToken, Task<Texture2D>> loadImage = null)
        {
            this.loadImage = loadImage ?? ((url, token) => VRCApi.GetImage(url, cancellationToken: token));
        }

        internal Task<Texture2D> GetAsync(string url, bool retry = false)
        {
            if (disposed || string.IsNullOrWhiteSpace(url)) return Task.FromResult<Texture2D>(null);
            if (entries.TryGetValue(url, out var existing))
            {
                if (!retry || !existing.Request.IsCompleted || existing.Texture != null) return existing.Request;
                entries.Remove(url);
            }
            var entry = new Entry();
            entries.Add(url, entry);
            pendingRequests++;
            entry.Request = DownloadAsync(url, entry, cancellation.Token);
            return entry.Request;
        }

        private async Task<Texture2D> DownloadAsync(string url, Entry entry, CancellationToken token)
        {
            var acquired = false;
            try
            {
                await downloads.WaitAsync(token);
                acquired = true;
                var texture = await loadImage(url, token);
                if (disposed || token.IsCancellationRequested)
                {
                    if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                    return null;
                }
                if (texture == null) throw new InvalidOperationException("サムネイル画像が返されませんでした。");
                entry.Texture = texture;
                return texture;
            }
            finally
            {
                if (acquired) downloads.Release();
                pendingRequests--;
                DisposeResourcesIfIdle();
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            cancellation.Cancel();
            cancellationCompleted = true;
            foreach (var entry in entries.Values)
            {
                if (entry.Texture != null) UnityEngine.Object.DestroyImmediate(entry.Texture);
            }
            entries.Clear();
            DisposeResourcesIfIdle();
        }

        private void DisposeResourcesIfIdle()
        {
            // A queued cancellation or late download must finish before its resources are released.
            if (!cancellationCompleted || pendingRequests != 0) return;
            downloads.Dispose();
            cancellation.Dispose();
        }
    }
}
