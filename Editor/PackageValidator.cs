#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using System.Linq;
using System.Collections.Generic;

namespace UPMUtils.Editor
{
    /// <summary>
    /// パッケージのインストール状態を検証するユーティリティ
    /// </summary>
    internal static class PackageValidator
    {
        private static UnityEditor.PackageManager.PackageInfo[] cachedPackages;
        private static float lastCacheTime;
        private const float CACHE_DURATION = 5f; // 5秒間キャッシュ

        #region Public Methods

        /// <summary>
        /// 指定したパッケージがインストールされているかチェック
        /// </summary>
        public static bool IsPackageInstalled(string packageId)
        {
            if (string.IsNullOrEmpty(packageId))
                return false;

            var installedPackages = GetInstalledPackages();
            return installedPackages.Any(p => p.name.Equals(packageId, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 複数パッケージのインストール状態をチェック
        /// </summary>
        public static Dictionary<string, bool> CheckPackagesInstallation(IEnumerable<string> packageIds)
        {
            var result = new Dictionary<string, bool>();
            var installedPackages = GetInstalledPackages();
            var installedPackageNames = new HashSet<string>(
                installedPackages.Select(p => p.name),
                System.StringComparer.OrdinalIgnoreCase
            );

            foreach (var packageId in packageIds)
            {
                result[packageId] = installedPackageNames.Contains(packageId);
            }

            return result;
        }

        /// <summary>
        /// インストール済みパッケージの情報を取得
        /// </summary>
        public static UnityEditor.PackageManager.PackageInfo GetInstalledPackageInfo(string packageId)
        {
            if (string.IsNullOrEmpty(packageId))
                return null;

            var installedPackages = GetInstalledPackages();
            return installedPackages.FirstOrDefault(p =>
                p.name.Equals(packageId, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// すべてのインストール済みパッケージを取得
        /// </summary>
        public static UnityEditor.PackageManager.PackageInfo[] GetInstalledPackages()
        {
            // キャッシュチェック
            if (cachedPackages != null && (EditorApplication.timeSinceStartup - lastCacheTime) < CACHE_DURATION)
            {
                return cachedPackages;
            }

            // キャッシュ更新
            cachedPackages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
            lastCacheTime = (float)EditorApplication.timeSinceStartup;

            return cachedPackages;
        }

        /// <summary>
        /// プロジェクト依存のパッケージのみを取得
        /// </summary>
        public static UnityEditor.PackageManager.PackageInfo[] GetProjectPackages()
        {
            return GetInstalledPackages()
                .Where(p => p.source == PackageSource.Registry ||
                            p.source == PackageSource.Git ||
                            p.source == PackageSource.Local ||
                            p.source == PackageSource.Embedded)
                .ToArray();
        }

        /// <summary>
        /// Built-inパッケージを除外したパッケージを取得
        /// </summary>
        public static UnityEditor.PackageManager.PackageInfo[] GetNonBuiltInPackages()
        {
            return GetInstalledPackages()
                .Where(p => p.source != PackageSource.BuiltIn)
                .ToArray();
        }

        /// <summary>
        /// パッケージの依存関係をチェック
        /// </summary>
        public static bool HasDependencyConflicts(string packageId)
        {
            var packageInfo = GetInstalledPackageInfo(packageId);
            if (packageInfo == null)
                return false;

            // 簡易的な依存関係チェック（実際の実装はより複雑）
            return packageInfo.dependencies.Length > 0 &&
                    packageInfo.dependencies.Any(dep => !IsPackageInstalled(dep.name));
        }

        /// <summary>
        /// パッケージのバージョン情報を取得
        /// </summary>
        public static string GetPackageVersion(string packageId)
        {
            var packageInfo = GetInstalledPackageInfo(packageId);
            return packageInfo?.version ?? "Not Installed";
        }

        /// <summary>
        /// 開発用パッケージかどうかをチェック
        /// </summary>
        public static bool IsDevelopmentPackage(string packageId)
        {
            var packageInfo = GetInstalledPackageInfo(packageId);
            if (packageInfo == null)
                return false;

            return packageInfo.source == PackageSource.Local ||
                    packageInfo.source == PackageSource.Embedded ||
                    packageInfo.source == PackageSource.Git;
        }

        /// <summary>
        /// パッケージキャッシュを強制更新
        /// </summary>
        public static void RefreshPackageCache()
        {
            cachedPackages = null;
            lastCacheTime = 0f;
            GetInstalledPackages(); // キャッシュを再構築
        }

        /// <summary>
        /// MenuItemの有効性をチェック
        /// </summary>
        public static bool ValidatePackageInstallation(string packageId)
        {
            return !IsPackageInstalled(packageId) && !PackageInstaller.Instance.IsProcessing;
        }

        /// <summary>
        /// インストール可能なパッケージのリストを取得
        /// </summary>
        public static IEnumerable<PackageInfo> GetInstallablePackages()
        {
            var installedPackageIds = GetInstalledPackages().Select(p => p.name).ToHashSet();

            return PackageDatabase.GetAllPackages()
                .Where(p => !installedPackageIds.Contains(p.PackageId));
        }

        /// <summary>
        /// パッケージの詳細情報を表示
        /// </summary>
        public static void LogPackageInfo(string packageId)
        {
            var packageInfo = GetInstalledPackageInfo(packageId);
            if (packageInfo == null)
            {
                Debug.Log($"Package '{packageId}' is not installed.");
                return;
            }

            Debug.Log($"Package Info:\n" +
                    $"  Name: {packageInfo.displayName}\n" +
                    $"  ID: {packageInfo.name}\n" +
                    $"  Version: {packageInfo.version}\n" +
                    $"  Source: {packageInfo.source}\n" +
                    $"  Dependencies: {packageInfo.dependencies.Length}\n" +
                    $"  Description: {packageInfo.description}");
        }

        #endregion
    }
}
#endif
