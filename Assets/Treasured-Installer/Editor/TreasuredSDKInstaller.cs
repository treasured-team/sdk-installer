using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class TreasuredSDKInstaller : MonoBehaviour
{
    struct PackageInfo
    {
        public string PackageName;
        public string PackageSource;

        public PackageInfo(string packageName, string packageSource)
        {
            PackageName = packageName;
            PackageSource = packageSource;
        }
    }

    private static List<PackageInfo> _packageList = new List<PackageInfo>();

    private const string TreasuredLogPrefix = "<b>[Treasured Installer]</b>: ";
    private static readonly bool ShouldAutoDeleteInstaller = true;
    private static bool _isInitialized = false;

    [MenuItem("Treasured/AddPackages")]
#if !TREASURED_DEV_ENV
    [InitializeOnLoadMethod]
#endif
    private static async void AddAllPackages()
    {
        if (_isInitialized)
        {
            TreasuredLog("Waiting for installation..");
            return;
        }

        await InitializeInstaller();
    }

    private static async Task InitializeInstaller()
    {
        _isInitialized = true;
        EditorApplication.LockReloadAssemblies();
        TreasuredLog("Launcher started..");

        ChangeApiCompabilityLevel();

        _packageList.Add(new PackageInfo("com.unity.nuget.newtonsoft-json", "com.unity.nuget.newtonsoft-json"));
        _packageList.Add(new PackageInfo("com.whinarn.unitymeshsimplifier",
            "https://github.com/Whinarn/UnityMeshSimplifier.git"));
        _packageList.Add(new PackageInfo("com.treasured.unitysdk",
            "https://github.com/TB-Terence/treasured-sdk-for-unity.git#upm"));


        foreach (var package in _packageList)
        {
            await InstallPackage(package.PackageSource);
        }

        EditorApplication.UnlockReloadAssemblies();

        //  Check if all the required packages are installed
        await ValidatePackages();

        TreasuredLog("Installation Complete successfully.");

#if !TREASURED_DEV_ENV
        await DeleteInstaller();
#endif

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Treasured Installer", "Treasured SDK installed successfully!", "OK");
    }

    private static async Task InstallPackage(string packageName)
    {
        EditorUtility.DisplayProgressBar("Treasured Installer", "Installing package " + packageName, 0);
        var addRequest = Client.Add(packageName);
        await Task.Delay(1000);
        while (!addRequest.IsCompleted)
        {
            await Task.Delay(100);
        }

        await Task.Delay(1000);

        if (addRequest.Status == StatusCode.Success)
        {
            TreasuredLog("Installed " + addRequest.Result.packageId);
        }

        EditorUtility.ClearProgressBar();
    }

    private static void ChangeApiCompabilityLevel()
    {
        TreasuredLog("Checking if Project API is Compatible..");
        if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) != ApiCompatibilityLevel.NET_4_6)
        {
            TreasuredLog("Changed Project API Compability level to 4.x  ");
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);
        }
    }

    //  Check if all the required packages are installed
    private static async Task ValidatePackages()
    {
        var packageList = Client.List();
        while (!packageList.IsCompleted)
        {
            await Task.Delay(100);
        }

        foreach (var package in _packageList)
        {
            EditorUtility.DisplayProgressBar("Treasured Installer", "Validating package " + package.PackageName, 0);

            var containsPackage =
                packageList.Result.FirstOrDefault(packageInfo => packageInfo.name == package.PackageName);

            if (containsPackage == null || containsPackage.status != PackageStatus.Available)
            {
                TreasuredLog($"Issue validating {package.PackageName}. Re-installing..!");

                //  Try re-installing the package
                await InstallPackage(package.PackageSource);
            }

            EditorUtility.ClearProgressBar();
        }
    }

    //  Delete installer script from the project
    private static async Task DeleteInstaller()
    {
        var installerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Treasured-Installer");

        if (!string.IsNullOrEmpty(installerPath) && Directory.Exists(installerPath))
        {
            while (EditorApplication.isUpdating || EditorApplication.isCompiling)
            {
                await Task.Delay(500);
            }

            Directory.Delete(installerPath, true);
            var meta = installerPath + ".meta";
            if (File.Exists(meta))
            {
                File.Delete(meta);
            }

            TreasuredLog("Installer deleted");
            AssetDatabase.Refresh();
        }
    }

    private static void TreasuredLog(string message)
    {
#if TREASURED_DEV_ENV
        Debug.Log(TreasuredLogPrefix + message);
#else
        Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, TreasuredLogPrefix + message);
#endif
    }
}
