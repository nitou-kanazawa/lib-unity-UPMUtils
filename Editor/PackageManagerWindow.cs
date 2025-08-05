#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace UPMUtils.Editor
{
    /// <summary>
    /// Package Manager用のカスタムエディターウィンドウ
    /// </summary>
    internal sealed class PackageManagerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private string searchFilter = "";
        private PackageCategory selectedCategory = PackageCategory.Core;
        private bool showOnlyInstallable = true;
        private int selectedTab = 0;
        private readonly string[] tabNames = { "Browse", "Installed", "Queue", "Settings" };

        [MenuItem("Tools/Package Manager/Package Manager Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<PackageManagerWindow>("Package Manager");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            // イベント購読
            PackageInstaller.OnPackageInstalled += OnPackageInstalled;
            PackageInstaller.OnPackageInstallFailed += OnPackageInstallFailed;
            PackageInstaller.OnPackageQueued += OnPackageQueued;
            PackageInstaller.OnQueueCompleted += OnQueueCompleted;
        }

        private void OnDisable()
        {
            // イベント購読解除
            PackageInstaller.OnPackageInstalled -= OnPackageInstalled;
            PackageInstaller.OnPackageInstallFailed -= OnPackageInstallFailed;
            PackageInstaller.OnPackageQueued -= OnPackageQueued;
            PackageInstaller.OnQueueCompleted -= OnQueueCompleted;
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawTabs();

            EditorGUILayout.Space();

            switch (selectedTab)
            {
                case 0: DrawBrowseTab(); break;
                case 1: DrawInstalledTab(); break;
                case 2: DrawQueueTab(); break;
                case 3: DrawSettingsTab(); break;
            }
        }

        #region GUI Drawing Methods

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("Unity Package Manager Utility", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            // ステータス表示
            var (queueCount, isProcessing, _) = PackageInstaller.Instance.GetStatus();
            var statusText = isProcessing ? "🔄 Installing..." : $"📦 Queue: {queueCount}";
            GUILayout.Label(statusText, EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabs()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        }

        private void DrawBrowseTab()
        {
            EditorGUILayout.BeginHorizontal();

            // 検索フィルター
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            searchFilter = EditorGUILayout.TextField(searchFilter);

            // カテゴリフィルター
            EditorGUILayout.LabelField("Category:", GUILayout.Width(60));
            selectedCategory = (PackageCategory)EditorGUILayout.EnumPopup(selectedCategory, GUILayout.Width(100));

            // インストール可能のみ表示
            showOnlyInstallable = EditorGUILayout.Toggle("Installable Only", showOnlyInstallable);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // パッケージリスト
            var packages = GetFilteredPackages();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var package in packages)
            {
                DrawPackageItem(package);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawInstalledTab()
        {
            var installedPackages = PackageValidator.GetProjectPackages();

            EditorGUILayout.LabelField($"Installed Packages ({installedPackages.Length})", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var package in installedPackages.OrderBy(p => p.displayName))
            {
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(package.displayName, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Version: {package.version}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Source: {package.source}", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Info", GUILayout.Width(50)))
                {
                    PackageValidator.LogPackageInfo(package.name);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawQueueTab()
        {
            var (queueCount, isProcessing, currentPackage) = PackageInstaller.Instance.GetStatus();

            EditorGUILayout.LabelField("Installation Queue", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"Queue Count: {queueCount}");
            EditorGUILayout.LabelField($"Processing: {(isProcessing ? "Yes" : "No")}");
            EditorGUILayout.EndHorizontal();

            if (isProcessing)
            {
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField("Current:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(currentPackage);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = queueCount > 0 && !isProcessing;
            if (GUILayout.Button("Clear Queue"))
            {
                if (EditorUtility.DisplayDialog("Clear Queue",
                    "Are you sure you want to clear the installation queue?",
                    "Yes", "No"))
                {
                    PackageInstaller.Instance.ClearQueue();
                }
            }
            GUI.enabled = true;

            if (GUILayout.Button("Refresh"))
            {
                PackageValidator.RefreshPackageCache();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSettingsTab()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Package Database", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Manage custom packages and package sets.", MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh Package Cache"))
            {
                PackageValidator.RefreshPackageCache();
                ShowNotification(new GUIContent("Package cache refreshed"));
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Install Recommended"))
            {
                var packages = PackageDatabase.GetRecommendedPackages();
                var packagesToInstall = packages
                    .Where(p => !PackageValidator.IsPackageInstalled(p.PackageId))
                    .Select(p => (p.DisplayName, p.Url));

                PackageInstaller.Instance.QueuePackages(packagesToInstall);
            }

            if (GUILayout.Button("Install 3D Game Set"))
            {
                var packages = PackageDatabase.Get3DGamePackages();
                var packagesToInstall = packages
                    .Where(p => !PackageValidator.IsPackageInstalled(p.PackageId))
                    .Select(p => (p.DisplayName, p.Url));

                PackageInstaller.Instance.QueuePackages(packagesToInstall);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPackageItem(PackageInfo package)
        {
            bool isInstalled = PackageValidator.IsPackageInstalled(package.PackageId);

            EditorGUILayout.BeginHorizontal("box");

            // パッケージ情報
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(package.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(package.Description, EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField($"Category: {package.Category}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // ステータスと操作
            EditorGUILayout.BeginVertical(GUILayout.Width(80));

            if (isInstalled)
            {
                var version = PackageValidator.GetPackageVersion(package.PackageId);
                EditorGUILayout.LabelField("✅ Installed", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(version, EditorStyles.miniLabel);
            }
            else
            {
                GUI.enabled = !PackageInstaller.Instance.IsProcessing;
                if (GUILayout.Button("Install", GUILayout.Width(60)))
                {
                    PackageInstaller.Instance.QueuePackage(package.DisplayName, package.Url);
                }
                GUI.enabled = true;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
        }

        #endregion

        #region Helper Methods

        private IEnumerable<PackageInfo> GetFilteredPackages()
        {
            var packages = PackageDatabase.GetAllPackages();

            // カテゴリフィルター
            packages = packages.Where(p => p.Category == selectedCategory);

            // 検索フィルター
            if (!string.IsNullOrEmpty(searchFilter))
            {
                packages = packages.Where(p =>
                    p.DisplayName.ToLower().Contains(searchFilter.ToLower()) ||
                    p.Description.ToLower().Contains(searchFilter.ToLower()));
            }

            // インストール可能のみ
            if (showOnlyInstallable)
            {
                packages = packages.Where(p => !PackageValidator.IsPackageInstalled(p.PackageId));
            }

            return packages.OrderBy(p => p.DisplayName);
        }

        #endregion

        #region Event Handlers

        private void OnPackageInstalled(string packageName)
        {
            ShowNotification(new GUIContent($"✅ {packageName} installed"));
            Repaint();
        }

        private void OnPackageInstallFailed(string packageName, string error)
        {
            ShowNotification(new GUIContent($"❌ Failed: {packageName}"));
            Repaint();
        }

        private void OnPackageQueued(string packageName)
        {
            ShowNotification(new GUIContent($"📦 Queued: {packageName}"));
            Repaint();
        }

        private void OnQueueCompleted()
        {
            ShowNotification(new GUIContent("✅ All packages processed"));
            Repaint();
        }

        #endregion
    }
}
#endif
