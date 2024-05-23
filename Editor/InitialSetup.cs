using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Omnilatent.InAppPurchase.EditorNS
{
    public class InitialSetup : EditorWindow
    {
        private static InitialSetup _instance;
        private const string PackageName = "IAP Helper";
        
        #if !OMNILATENT_IAP_HELPER
        [UnityEditor.Callbacks.DidReloadScripts]
        #endif
        private static void ShowInstallWindowWhenReady()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += ShowInstallWindowWhenReady;
                return;
            }
 
            EditorApplication.delayCall += ShowInstallWindow;
        }
        
        [MenuItem("Tools/Omnilatent/IAP Helper/Import essential files")]
        public static void ShowInstallWindow()
        {
            if (_instance == null)
            {
                _instance = GetWindow<InitialSetup>();
                _instance.maxSize = new Vector2(670f, 280f);
                _instance.minSize = new Vector2(500f, 280f);
                _instance.titleContent = new GUIContent($"{PackageName} Initial Setup");
            }
            else
            {
                _instance.Focus();
            }
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            Label label = new Label($"Click on the following button to import files needed for {PackageName} to work properly.");
            label.style.marginTop = label.style.marginBottom = 20;
            label.style.marginLeft = label.style.marginRight = 20;
            label.style.alignSelf = new StyleEnum<Align>(Align.Center);
            label.style.whiteSpace = WhiteSpace.Normal;
            root.Add(label);

            Button button = new Button();
            button.style.height = 80;
            button.style.marginTop = new StyleLength(StyleKeyword.Auto);
            button.style.marginBottom = 10;
            button.name = "button";
            button.text = $"Import {PackageName}'s essential files";
            button.clicked += OnInstall;
            root.Add(button);
        }

        private void OnInstall()
        {
            ExtraPackageImporter.ImportExtraPackage();
            InitScriptingDefineSymbol();
        }
        
        const string SYMBOL = "OMNILATENT_IAP_HELPER";

        public static void InitScriptingDefineSymbol()
        {
#if !OMNILATENT_IAP_HELPER
            // Get current defines
            string defineSymbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            // Split at ;
            List<string> symbols = defineSymbolString.Split(';').ToList();
            // check if defines already exist given define
            if (!symbols.Contains(SYMBOL))
            {
                // if not add it at the end with a leading ; separator
                defineSymbolString += $";{SYMBOL}";

                // write the new defines back to the PlayerSettings
                // This will cause a recompilation of your scripts
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbolString);

                Debug.Log($"Scripting Define Symbol '{SYMBOL}' was added.");
            }
#endif
        }
    }
}