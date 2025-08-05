#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System;
using System.Collections.Generic;

namespace UPMUtils.Editor
{
    /// <summary>
    /// Unity Package Manager用のパッケージインストーラー
    /// 順次実行によりパッケージの安全なインストールを提供
    /// </summary>
    internal sealed class PackageInstaller
    {
        #region Events

        public static event Action<string> OnPackageInstalled;
        public static event Action<string, string> OnPackageInstallFailed;
        public static event Action<string> OnPackageQueued;
        public static event Action OnQueueCompleted;
        #endregion

        #region Private Fields

        private static PackageInstaller instance;
        private readonly Queue<PackageInstallRequest> installQueue;
        private AddRequest currentRequest;
        private bool isProcessing;
        #endregion

        #region Properties

        public static PackageInstaller Instance
        {
            get
            {
                if (instance == null)
                    instance = new PackageInstaller();
                return instance;
            }
        }

        public bool IsProcessing => isProcessing;
        public int QueueCount => installQueue.Count;
        #endregion

        private PackageInstaller()
        {
            installQueue = new Queue<PackageInstallRequest>();
        }

        #region Public Methods

        /// <summary>
        /// パッケージをインストールキューに追加
        /// </summary>
        public void QueuePackage(string packageName, string packageUrl)
        {
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(packageUrl))
            {
                Debug.LogError("Package name and URL cannot be null or empty");
                return;
            }

            var request = new PackageInstallRequest(packageName, packageUrl);
            installQueue.Enqueue(request);

            OnPackageQueued?.Invoke(packageName);
            Debug.Log($"📦 Queued package: {packageName}");

            if (!isProcessing)
            {
                ProcessNextPackage();
            }
        }

        /// <summary>
        /// 複数パッケージを一括でキューに追加
        /// </summary>
        public void QueuePackages(IEnumerable<(string name, string url)> packages)
        {
            foreach (var (name, url) in packages)
            {
                QueuePackage(name, url);
            }
        }

        /// <summary>
        /// インストールキューをクリア
        /// </summary>
        public void ClearQueue()
        {
            if (isProcessing)
            {
                Debug.LogWarning("Cannot clear queue while processing. Wait for current installation to complete.");
                return;
            }

            installQueue.Clear();
            Debug.Log("📦 Package queue cleared");
        }

        /// <summary>
        /// 現在のキュー状態を取得
        /// </summary>
        public (int queueCount, bool isProcessing, string currentPackage) GetStatus()
        {
            string currentPackage = currentRequest != null && !currentRequest.IsCompleted
                ? "Installing..."
                : "Idle";

            return (installQueue.Count, isProcessing, currentPackage);
        }

        #endregion

        #region Private Methods

        private void ProcessNextPackage()
        {
            if (installQueue.Count == 0)
            {
                isProcessing = false;
                OnQueueCompleted?.Invoke();
                Debug.Log("✅ All packages processed");
                return;
            }

            if (isProcessing)
            {
                Debug.LogWarning("Package installation already in progress");
                return;
            }

            isProcessing = true;
            var packageRequest = installQueue.Dequeue();

            Debug.Log($"🔄 Installing package: {packageRequest.Name}");

            try
            {
                currentRequest = Client.Add(packageRequest.Url);
                EditorApplication.update += ProgressCallback;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to start installation for {packageRequest.Name}: {ex.Message}");
                OnPackageInstallFailed?.Invoke(packageRequest.Name, ex.Message);
                CompleteCurrentInstallation();
            }
        }

        private void ProgressCallback()
        {
            if (currentRequest == null || !currentRequest.IsCompleted)
                return;

            try
            {
                if (currentRequest.Status == StatusCode.Success)
                {
                    var result = currentRequest.Result;
                    Debug.Log($"✅ Successfully installed: {result.displayName} ({result.packageId})");
                    OnPackageInstalled?.Invoke(result.displayName);
                }
                else if (currentRequest.Status >= StatusCode.Failure)
                {
                    var errorMessage = currentRequest.Error?.message ?? "Unknown error";
                    Debug.LogError($"❌ Failed to install package: {errorMessage}");
                    OnPackageInstallFailed?.Invoke("Unknown", errorMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error processing installation result: {ex.Message}");
            }
            finally
            {
                CompleteCurrentInstallation();
            }
        }

        private void CompleteCurrentInstallation()
        {
            EditorApplication.update -= ProgressCallback;
            currentRequest = null;
            isProcessing = false;

            // 次のパッケージを処理（delayCallで安全に実行）
            EditorApplication.delayCall += ProcessNextPackage;
        }

        #endregion

        #region Nested Types

        private struct PackageInstallRequest
        {
            public readonly string Name;
            public readonly string Url;

            public PackageInstallRequest(string name, string url)
            {
                Name = name;
                Url = url;
            }
        }

        #endregion
    }
}
#endif
