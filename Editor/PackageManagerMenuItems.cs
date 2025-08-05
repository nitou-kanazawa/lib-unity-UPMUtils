#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UPMUtils.Editor
{
    /// <summary>
    /// Unity Package Manager用のMenuItemを提供
    /// </summary>
    internal static class PackageManagerMenuItems
    {
        private const string MENU_ROOT = "Tools/Package Manager/";
        private const string MENU_PACKAGES = MENU_ROOT + "Install/";
        private const string MENU_SETS = MENU_ROOT + "Package Sets/";
        private const string MENU_UTILS = MENU_ROOT + "Utilities/";

        #region Individual Package Installation

        [MenuItem(MENU_PACKAGES + "UniRx")]
        private static void InstallUniRx() => InstallPackage("UniRx");

        [MenuItem(MENU_PACKAGES + "UniRx", true)]
        private static bool ValidateInstallUniRx() => ValidatePackageInstall("UniRx");

        [MenuItem(MENU_PACKAGES + "UniTask")]
        private static void InstallUniTask() => InstallPackage("UniTask");

        [MenuItem(MENU_PACKAGES + "UniTask", true)]
        private static bool ValidateInstallUniTask() => ValidatePackageInstall("UniTask");

        [MenuItem(MENU_PACKAGES + "Input System")]
        private static void InstallInputSystem() => InstallPackage("Input System");

        [MenuItem(MENU_PACKAGES + "Input System", true)]
        private static bool ValidateInstallInputSystem() => ValidatePackageInstall("Input System");

        [MenuItem(MENU_PACKAGES + "Addressables")]
        private static void InstallAddressables() => InstallPackage("Addressables");

        [MenuItem(MENU_PACKAGES + "Addressables", true)]
        private static bool ValidateInstallAddressables() => ValidatePackageInstall("Addressables");

        [MenuItem(MENU_PACKAGES + "Cinemachine")]
        private static void InstallCinemachine() => InstallPackage("Cinemachine");

        [MenuItem(MENU_PACKAGES + "Cinemachine", true)]
        private static bool ValidateInstallCinemachine() => ValidatePackageInstall("Cinemachine");

        [MenuItem(MENU_PACKAGES + "Universal Render Pipeline")]
        private static void InstallURP() => InstallPackage("URP");

        [MenuItem(MENU_PACKAGES + "Universal Render Pipeline", true)]
        private static bool ValidateInstallURP() => ValidatePackageInstall("URP");

        [MenuItem(MENU_PACKAGES + "High Definition Render Pipeline")]
        private static void InstallHDRP() => InstallPackage("HDRP");

        [MenuItem(MENU_PACKAGES + "High Definition Render Pipeline", true)]
        private static bool ValidateInstallHDRP() => ValidatePackageInstall("HDRP");

        [MenuItem(MENU_PACKAGES + "UI Toolkit")]
        private static void InstallUIToolkit() => InstallPackage("UI Toolkit");

        [MenuItem(MENU_PACKAGES + "UI Toolkit", true)]
        private static bool ValidateInstallUIToolkit() => ValidatePackageInstall("UI Toolkit");

        [MenuItem(MENU_PACKAGES + "Test Framework")]
        private static void InstallTestFramework() => InstallPackage("Test Framework");

        [MenuItem(MENU_PACKAGES + "Test Framework", true)]
        private static bool ValidateInstallTestFramework() => ValidatePackageInstall("Test Framework");

        #endregion

        #region Package Sets Installation

        [MenuItem(MENU_SETS + "Recommended Packages")]
        private static void InstallRecommendedPackages()
        {
            var packages = PackageDatabase.GetRecommendedPackages();
            InstallPackageSet(packages, "Recommended Packages");
        }

        [MenuItem(MENU_SETS + "Recommended Packages", true)]
        private static bool ValidateInstallRecommendedPackages() => !PackageInstaller.Instance.IsProcessing;

        [MenuItem(MENU_SETS + "3D Game Development")]
        private static void Install3DGamePackages()
        {
            var packages = PackageDatabase.Get3DGamePackages();
            InstallPackageSet(packages, "3D Game Development");
        }

        [MenuItem(MENU_SETS + "3D Game Development", true)]
        private static bool ValidateInstall3DGamePackages() => !PackageInstaller.Instance.IsProcessing;

        [MenuItem(MENU_SETS + "2D Game Development")]
        private static void Install2DGamePackages()
        {
            var packages = PackageDatabase.Get2DGamePackages();
            InstallPackageSet(packages, "2D Game Development");
        }

        [MenuItem(MENU_SETS + "2D Game Development", true)]
        private static bool ValidateInstall2DGamePackages() => !PackageInstaller.Instance.IsProcessing;

        [MenuItem(MENU_SETS + "Core Unity Packages")]
        private static void InstallCorePackages()
        {
            var packages = PackageDatabase.GetPackagesByCategory(PackageCategory.Core);
            InstallPackageSet(packages, "Core Unity Packages");
        }

        [MenuItem(MENU_SETS + "Core Unity Packages", true)]
        private static bool ValidateInstallCorePackages() => !PackageInstaller.Instance.IsProcessing;

        [MenuItem(MENU_SETS + "Rendering Packages")]
        private static void InstallRenderingPackages()
        {
            var packages = PackageDatabase.GetPackagesByCategory(PackageCategory.Rendering);
            InstallPackageSet(packages, "Rendering Packages");
        }

        [MenuItem(MENU_SETS + "Rendering Packages", true)]
        private static bool ValidateInstallRenderingPackages() => !PackageInstaller.Instance.IsProcessing;

        #endregion

        #region Utilities

        [MenuItem(MENU_UTILS + "Show Installation Status")]
        private static void ShowInstallationStatus()
        {
            var (queueCount, isProcessing, currentPackage) = PackageInstaller.Instance.GetStatus();

            var message = $"Package Manager Status:\n" +
                         $"Queue: {queueCount} packages\n" +
                         $"Processing: {(isProcessing ? "Yes" : "No")}\n" +
                         $"Current: {currentPackage}";

            EditorUtility.DisplayDialog("Package Manager Status", message, "OK");
        }

        [MenuItem(MENU_UTILS + "Clear Installation Queue")]
        private static void ClearInstallationQueue()
        {
            if (EditorUtility.DisplayDialog("Clear Queue",
                "Are you sure you want to clear the installation queue?",
                "Yes", "No"))
            {
                PackageInstaller.Instance.ClearQueue();
            }
        }

        [MenuItem(MENU_UTILS + "Clear Installation Queue", true)]
        private static bool ValidateClearInstallationQueue() =>
            PackageInstaller.Instance.QueueCount > 0 && !PackageInstaller.Instance.IsProcessing;

        [MenuItem(MENU_UTILS + "Refresh Package Cache")]
        private static void RefreshPackageCache()
        {
            PackageValidator.RefreshPackageCache();
            Debug.Log("Package cache refreshed");
        }

        [MenuItem(MENU_UTILS + "Show Installed Packages")]
        private static void ShowInstalledPackages()
        {
            var installedPackages = PackageValidator.GetProjectPackages();
            var message = "Installed Packages:\n\n" +
                         string.Join("\n", installedPackages.Select(p => $"• {p.displayName} ({p.version})"));

            EditorUtility.DisplayDialog("Installed Packages", message, "OK");
        }

        [MenuItem(MENU_UTILS + "Show Available Packages")]
        private static void ShowAvailablePackages()
        {
            var availablePackages = PackageValidator.GetInstallablePackages();
            var message = "Available Packages:\n\n" +
                        string.Join("\n", availablePackages.Select(p => $"• {p.DisplayName}"));

            EditorUtility.DisplayDialog("Available Packages", message, "OK");
        }

        [MenuItem(MENU_UTILS + "Open Package Manager Window")]
        private static void OpenPackageManagerWindow()
        {
            PackageManagerWindow.ShowWindow();
        }

        #endregion

        #region Private Helper Methods

        private static void InstallPackage(string packageName)
        {
            var packageInfo = PackageDatabase.GetPackage(packageName);
            if (packageInfo == null)
            {
                Debug.LogError($"Package '{packageName}' not found in database");
                return;
            }

            PackageInstaller.Instance.QueuePackage(packageInfo.DisplayName, packageInfo.Url);
        }

        private static bool ValidatePackageInstall(string packageName)
        {
            if (PackageInstaller.Instance.IsProcessing)
                return false;

            var packageInfo = PackageDatabase.GetPackage(packageName);
            if (packageInfo == null)
                return false;

            return !PackageValidator.IsPackageInstalled(packageInfo.PackageId);
        }

        private static void InstallPackageSet(System.Collections.Generic.IEnumerable<PackageInfo> packages, string setName)
        {
            var packageList = packages.ToList();
            var notInstalled = packageList.Where(p => !PackageValidator.IsPackageInstalled(p.PackageId)).ToList();

            if (notInstalled.Count == 0)
            {
                EditorUtility.DisplayDialog("Package Set",
                    $"All packages in '{setName}' are already installed.", "OK");
                return;
            }

            var message = $"Install {notInstalled.Count} packages from '{setName}'?\n\n" +
                        string.Join("\n", notInstalled.Select(p => $"• {p.DisplayName}"));

            if (EditorUtility.DisplayDialog("Install Package Set", message, "Install", "Cancel"))
            {
                var packagesToInstall = notInstalled.Select(p => (p.DisplayName, p.Url));
                PackageInstaller.Instance.QueuePackages(packagesToInstall);
            }
        }

        #endregion
    }
}
#endif
