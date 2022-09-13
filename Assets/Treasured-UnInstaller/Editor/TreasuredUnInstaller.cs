using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class TreasuredUnInstaller : MonoBehaviour
{
    private static List<string> _packageList = new List<string>();
    private static int _packageIndex = 0;
    private static RemoveRequest _removeRequest;

    private const string TreasuredUnInstallerName = "Treasured Un-Installer";
    private const string TreasuredLogPrefix = "<b>[Treasured Un-Installer]</b>";
    private static readonly bool ShouldAutoDeleteUnInstaller = true;

    [MenuItem("Treasured/Remove Packages")]
    // [InitializeOnLoadMethod]
    private static async void RemoveAllPackages()
    {
        EditorApplication.LockReloadAssemblies();
        Debug.Log("<b>[Treasured Un-Installer]</b>: Launcher started..");
        _packageIndex = 0;

        ChangeApiCompabilityLevel();

        _packageList = new List<string>
        {
            "com.treasured.unitysdk",
            "com.whinarn.unitymeshsimplifier",
            "com.unity.nuget.newtonsoft-json",
        };
        
        Client.Remove(_packageList[0]);
        await Task.Delay(1000);
        Debug.Log($"Removed {_packageList[0]}");
        
        Client.Remove(_packageList[1]);
        await Task.Delay(1000);
        Debug.Log($"Removed {_packageList[1]}");
        
        Client.Remove(_packageList[2]);
        await Task.Delay(1000);
        Debug.Log($"Removed {_packageList[2]}");
        
        EditorApplication.UnlockReloadAssemblies();
        await Task.Delay(1000);
        Client.Resolve();
        /*_removeRequest = Client.Remove(_packageList[_packageIndex]);
        await Task.Delay(1000);
        // AssetDatabase.Refresh();
        EditorApplication.update += Progress;*/
    }

    private static void Progress()
    {
        if (_removeRequest.IsCompleted)
        {
            if (_removeRequest.Status == StatusCode.Success)
            {
                Debug.Log("<b>[Treasured Un-Installer]</b> Installed : " + _removeRequest.PackageIdOrName);

                _packageIndex++;
                if (_packageIndex < _packageList.Count)
                {
                    _removeRequest = Client.Remove(_packageList[_packageIndex]);
                }
                else
                {
                    Debug.Log("<b>[Treasured Un-Installer]</b> Installation Complete successfully.");
                    EditorApplication.update -= Progress;
                    DeleteUnInstaller();
                    EditorApplication.UnlockReloadAssemblies();
                    Client.Resolve();
                    // AssetDatabase.Refresh();
                }
            }
            else
            {
                if (_removeRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log(_removeRequest.Error.message);
                }
            }
        }
    }

    private static void ChangeApiCompabilityLevel()
    {
        Debug.Log("<b>[Treasured Un-Installer]</b>: Checking if API is Compatible..");
        if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) == ApiCompatibilityLevel.NET_4_6)
        {
            Debug.Log("<b>[Treasured Un-Installer]</b>: Changed API Compability level to 2.x  ");
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_Standard_2_0);
            // AssetDatabase.Refresh();
        }
    }

    static void DeleteUnInstaller()
    {
        //  TODO: Delete Un-Installer script from the project
    }
}
