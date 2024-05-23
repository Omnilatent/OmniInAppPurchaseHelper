using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Omnilatent.InAppPurchase.EditorNS
{
    public class ExtraPackageImporter
    {
        // [MenuItem("Tools/Omnilatent/IAP Helper/Import Extra Package")]
        public static void ImportExtraPackage()
        {
            string path = GetPackagePath("Assets/Omnilatent/OmniInAppPurchaseHelper/OmniIAPExtra.unitypackage", "OmniIAPExtra");
            AssetDatabase.importPackageCompleted += ImportTangleAsmdefRef;
            AssetDatabase.importPackageCancelled += ImportTangleAsmdefRef;
            AssetDatabase.ImportPackage(path, true);
        }

        static void ImportTangleAsmdefRef(string lastPackageName)
        {
            AssetDatabase.importPackageCompleted -= ImportTangleAsmdefRef;
            AssetDatabase.importPackageCancelled -= ImportTangleAsmdefRef;
            Debug.Log("Importing OmniIAPExtra_TangleAsmdefRef");
            string path = GetPackagePath("Assets/Omnilatent/OmniInAppPurchaseHelper/OmniIAPExtra_TangleAsmdefRef.unitypackage", "OmniIAPExtra_TangleAsmdefRef");
            AssetDatabase.ImportPackage(path, true);
        }

        static string GetPackagePath(string path, string filename)
        {
            if (!File.Exists($"{Application.dataPath}/../{path}"))
            {
                Debug.Log($"{filename} not found at {path}, attempting to search whole project for {filename}");
                string[] guids = AssetDatabase.FindAssets($"{filename} l:package");
                if (guids.Length > 0)
                {
                    path = AssetDatabase.GUIDToAssetPath(guids[0]);
                }
                else
                {
                    Debug.LogError($"{filename} not found at {Application.dataPath}/../{path}");
                    return null;
                }
            }
            return path;
        }
    }
}