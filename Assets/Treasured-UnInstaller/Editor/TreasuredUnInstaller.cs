using System.Collections.Generic;
using System.Threading.Tasks;
using Treasured.UnitySdk.Installer;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Treasured.UnitySdk.UnInstaller
{
    public class TreasuredUnInstaller : MonoBehaviour
    {
        private static List<string> _packageList = new List<string>();

        [MenuItem("Treasured/Remove Packages")]
        private static async void RemoveAllPackages()
        {
            EditorApplication.LockReloadAssemblies();
            TreasuredLog("Launcher started..");

            ChangeApiCompabilityLevel();

            _packageList = new List<string>
            {
                TreasuredConstants.TreasuredUnitySdkPackageName,
                TreasuredConstants.UnityMeshSimplifierPackageName,
                TreasuredConstants.NewtonsoftJsonPackageName,
            };

            Client.Remove(_packageList[0]);
            await Task.Delay(1000);
            TreasuredLog($"Removed {_packageList[0]}");

            Client.Remove(_packageList[1]);
            await Task.Delay(1000);
            TreasuredLog($"Removed {_packageList[1]}");

            Client.Remove(_packageList[2]);
            await Task.Delay(1000);
            TreasuredLog($"Removed {_packageList[2]}");

            EditorApplication.UnlockReloadAssemblies();
            await Task.Delay(1000);
            Client.Resolve();
            
            EditorUtility.DisplayDialog("Treasured UnInstaller", "Treasured SDK uninstalled successfully!", "OK");
        }

        private static void ChangeApiCompabilityLevel()
        {
            TreasuredLog("Checking if API is Compatible..");
            if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) == ApiCompatibilityLevel.NET_4_6)
            {
                TreasuredLog("Changed API Compability level to 2.x  ");
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone,
                    ApiCompatibilityLevel.NET_Standard_2_0);
            }
        }

        private static void TreasuredLog(string message)
        {
#if TREASURED_DEV_ENV
            Debug.Log(TreasuredConstants.TreasuredUnInstallerLogPrefix + message);
#else
        Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, TreasuredConstants.TreasuredUnInstallerLogPrefix + message);
#endif
        }
    }
}
