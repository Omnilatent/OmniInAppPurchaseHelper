using UnityEditor;
using UnityEngine;

namespace Omnilatent.InAppPurchase.EditorNS
{
    public class ObfuscatorWindowServiceless : EditorWindow
    {
        string googlePlayKey = "";
        string resultMessage = "";

        [MenuItem(MenuItemEditor.MenuItemPath + "Obfuscation Generator")]
        public static void ShowWindow()
        {
            GetWindow<ObfuscatorWindowServiceless>("Obfuscator Serviceless");
        }

        void OnGUI()
        {
            GUILayout.Label("Google Play Key Obfuscator", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Google Play Key");
            googlePlayKey = EditorGUILayout.TextArea(googlePlayKey, GUILayout.MinHeight(60));
            
            if (GUILayout.Button("Obfuscate Google Secrets"))
            {
                if (!string.IsNullOrEmpty(googlePlayKey))
                {
                    resultMessage = ObfuscationGenerator.ObfuscateGoogleSecrets(googlePlayKey);
                    if (string.IsNullOrEmpty(resultMessage))
                        resultMessage = "Obfuscation completed successfully.";
                }
                else
                {
                    resultMessage = "Please enter a Google Play Key.";
                }
            }

            if (!string.IsNullOrEmpty(resultMessage))
            {
                EditorGUILayout.HelpBox(resultMessage, MessageType.Info);
            }
        }
    }
}