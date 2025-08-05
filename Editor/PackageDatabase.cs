#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

namespace UPMUtils.Editor
{
    /// <summary>
    /// よく使用されるUnityパッケージのデータベース
    /// </summary>
    internal static class PackageDatabase
    {
        private static readonly Dictionary<string, PackageInfo> packages = new()
        {
            // Reactive Extensions
            {
                "UniRx",
                new PackageInfo(
                    "UniRx",
                    "https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts",
                    "Reactive Extensions for Unity",
                    PackageCategory.Utility,
                    "com.neuecc.unirx"
                )
            },

            // Async/Await
            {
                "UniTask",
                new PackageInfo(
                    "UniTask",
                    "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
                    "Efficient async/await integration for Unity",
                    PackageCategory.Utility,
                    "com.cysharp.unitask"
                )
            },

            // Unity Official Packages
            {
                "Input System",
                new PackageInfo(
                    "Input System",
                    "com.unity.inputsystem",
                    "Unity's new Input System",
                    PackageCategory.Core,
                    "com.unity.inputsystem"
                )
            },

            {
                "Addressables",
                new PackageInfo(
                    "Addressables",
                    "com.unity.addressables",
                    "Unity Addressables Asset System",
                    PackageCategory.Core,
                    "com.unity.addressables"
                )
            },

            {
                "Cinemachine",
                new PackageInfo(
                    "Cinemachine",
                    "com.unity.cinemachine",
                    "Smart camera system",
                    PackageCategory.Rendering,
                    "com.unity.cinemachine"
                )
            },

            {
                "URP",
                new PackageInfo(
                    "Universal Render Pipeline",
                    "com.unity.render-pipelines.universal",
                    "Universal Render Pipeline",
                    PackageCategory.Rendering,
                    "com.unity.render-pipelines.universal"
                )
            },

            {
                "HDRP",
                new PackageInfo(
                    "High Definition Render Pipeline",
                    "com.unity.render-pipelines.high-definition",
                    "High Definition Render Pipeline",
                    PackageCategory.Rendering,
                    "com.unity.render-pipelines.high-definition"
                )
            },

            // UI
            {
                "UI Toolkit",
                new PackageInfo(
                    "UI Toolkit",
                    "com.unity.ui",
                    "Unity's new UI system",
                    PackageCategory.UI,
                    "com.unity.ui"
                )
            },

            // Networking
            {
                "Netcode for GameObjects",
                new PackageInfo(
                    "Netcode for GameObjects",
                    "com.unity.netcode.gameobjects",
                    "Unity's networking solution",
                    PackageCategory.Networking,
                    "com.unity.netcode.gameobjects"
                )
            },

            // Audio
            {
                "Unity Audio",
                new PackageInfo(
                    "Unity Audio",
                    "com.unity.modules.audio",
                    "Unity Audio modules",
                    PackageCategory.Audio,
                    "com.unity.modules.audio"
                )
            },

            // Analytics
            {
                "Unity Analytics",
                new PackageInfo(
                    "Unity Analytics",
                    "com.unity.analytics",
                    "Unity Analytics services",
                    PackageCategory.Services,
                    "com.unity.analytics"
                )
            },

            // Testing
            {
                "Test Framework",
                new PackageInfo(
                    "Test Framework",
                    "com.unity.test-framework",
                    "Unity Test Framework",
                    PackageCategory.Testing,
                    "com.unity.test-framework"
                )
            }
        };


        #region Public Methods

        /// <summary>
        /// 指定した名前のパッケージ情報を取得
        /// </summary>
        public static PackageInfo GetPackage(string packageName)
        {
            return packages.TryGetValue(packageName, out var package) ? package : null;
        }

        /// <summary>
        /// すべてのパッケージを取得
        /// </summary>
        public static IEnumerable<PackageInfo> GetAllPackages()
        {
            return packages.Values;
        }

        /// <summary>
        /// カテゴリ別にパッケージを取得
        /// </summary>
        public static IEnumerable<PackageInfo> GetPackagesByCategory(PackageCategory category)
        {
            return packages.Values.Where(p => p.Category == category);
        }

        /// <summary>
        /// パッケージ名の一覧を取得
        /// </summary>
        public static IEnumerable<string> GetPackageNames()
        {
            return packages.Keys;
        }

        /// <summary>
        /// パッケージが存在するかチェック
        /// </summary>
        public static bool HasPackage(string packageName)
        {
            return packages.ContainsKey(packageName);
        }

        /// <summary>
        /// カスタムパッケージを追加
        /// </summary>
        public static void AddCustomPackage(string key, PackageInfo packageInfo)
        {
            packages[key] = packageInfo;
        }

        /// <summary>
        /// 推奨パッケージセットを取得
        /// </summary>
        public static IEnumerable<PackageInfo> GetRecommendedPackages()
        {
            var recommended = new[]
            {
                "Input System",
                "Addressables",
                "Cinemachine",
                "URP"
            };

            return recommended.Select(GetPackage).Where(p => p != null);
        }

        /// <summary>
        /// 3Dゲーム開発用パッケージセット
        /// </summary>
        public static IEnumerable<PackageInfo> Get3DGamePackages()
        {
            var packages3D = new[]
            {
                "Input System",
                "Addressables",
                "Cinemachine",
                "URP",
                "UniTask"
            };

            return packages3D.Select(GetPackage).Where(p => p != null);
        }

        /// <summary>
        /// 2Dゲーム開発用パッケージセット
        /// </summary>
        public static IEnumerable<PackageInfo> Get2DGamePackages()
        {
            var packages2D = new[]
            {
                "Input System",
                "Addressables",
                "UniTask"
            };

            return packages2D.Select(GetPackage).Where(p => p != null);
        }

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// パッケージ情報
    /// </summary>
    [Serializable]
    public class PackageInfo
    {
        public string DisplayName { get; }
        public string Url { get; }
        public string Description { get; }
        public PackageCategory Category { get; }
        public string PackageId { get; }

        public PackageInfo(string displayName, string url, string description,
                          PackageCategory category, string packageId)
        {
            DisplayName = displayName;
            Url = url;
            Description = description;
            Category = category;
            PackageId = packageId;
        }
    }

    /// <summary>
    /// パッケージカテゴリ
    /// </summary>
    public enum PackageCategory
    {
        Core,
        Rendering,
        UI,
        Audio,
        Networking,
        Services,
        Testing,
        Utility
    }

    #endregion
}
#endif
