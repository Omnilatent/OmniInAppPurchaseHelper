using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;

namespace Omnilatent.InAppPurchase.EditorNS
{
    public class InitialSetup : EditorWindow
    {
        private static InitialSetup _instance;
        private const string PackageName = "IAP Helper";
        private Toggle _toggleAddInstanceToFirstScene;
        private Toggle _toggleUseOmniSceneManager;
        private Toggle _toggleDetectLoadingFunction;
        [SerializeField] private bool _waitingPostCompile;

        #if !OMNILATENT_IAP_HELPER
        [UnityEditor.Callbacks.DidReloadScripts]
        #endif
        private static void ShowInstallWindowWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += ShowInstallWindowWhenReady;
                return;
            }

            EditorApplication.delayCall += ShowInstallWindow;
        }

        #region GUI
        [MenuItem("Tools/Omnilatent/IAP Helper/Install...")]
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
            VisualElement root1 = rootVisualElement;
            VisualElement root = new VisualElement();

            root.style.marginTop = root.style.marginLeft = root.style.marginRight = 20;
            Label label = new Label($"Click on the following button to import files needed for {PackageName} to work properly.");
            label.style.alignSelf = new StyleEnum<Align>(Align.Center);
            label.style.whiteSpace = WhiteSpace.Normal;
            root.Add(label);

            // Horizontal line separator
            VisualElement line = new VisualElement();
            line.style.height = 1;
            line.style.marginTop = line.style.marginBottom = 12;
            line.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);
            root.Add(line);

            // Add checkboxes
            _toggleAddInstanceToFirstScene = new Toggle("Add instance to first scene.")
            {
                value = true
            };
            _toggleUseOmniSceneManager = new Toggle("Use Omni Scene Manager to show message.")
            {
                value = true
            };
            StyleToggle(_toggleAddInstanceToFirstScene);
            StyleToggle(_toggleUseOmniSceneManager);
            root.Add(_toggleAddInstanceToFirstScene);
            root.Add(_toggleUseOmniSceneManager);
            
            _toggleDetectLoadingFunction = new Toggle("Detect and implement loading screen function.")
            {
                value = true
            };
            StyleToggle(_toggleDetectLoadingFunction);
            root.Add(_toggleDetectLoadingFunction);

            Button button = new Button();
            button.style.height = 80;
            button.style.marginTop = 30;
            button.style.marginBottom = 10;
            button.name = "button";
            button.text = $"Import {PackageName}'s essential files";
            button.clicked += OnInstall;
            root.Add(button);
            root1.Add(root);
        }

        private void StyleToggle(Toggle toggle)
        {
            toggle.style.marginTop = 4;
            toggle.style.marginBottom = 4;
            toggle.style.maxWidth = 400;

            // Ensure toggle has enough height for one text line
            toggle.style.minHeight = EditorGUIUtility.singleLineHeight + 4;

            // Find the internal elements
            VisualElement input = toggle.Q(className: "unity-toggle__input");
            Label labelElement = toggle.Q<Label>();

            if (input != null)
            {
                // Position the checkbox absolutely inside the toggle
                // input.style.position = Position.Absolute;
                input.style.justifyContent = new StyleEnum<Justify>(Justify.FlexEnd);
            }

            if (labelElement != null)
            {
                // Shift label to the right of checkbox
                // labelElement.style.marginLeft = 22;
            }
        }
        #endregion
        
        private void OnInstall()
        {
            AssetDatabase.importPackageCompleted += OnPackageImported;
            ExtraPackageImporter.ImportExtraPackage();
        }

        private void OnPackageImported(string packageName)
        {
            AssetDatabase.importPackageCompleted -= OnPackageImported;
            Debug.Log($"Package import completed: {packageName}");

            InitScriptingDefineSymbol();
            // Wait until Unity finishes compiling before proceeding
            _waitingPostCompile = true;
            EditorApplication.update += WaitForCompilationToEnd;
        }

        private void WaitForCompilationToEnd()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !_waitingPostCompile)
                return;

            EditorApplication.update -= WaitForCompilationToEnd;

            DoPostInstallActions();
        }

        private void DoPostInstallActions()
        {
            _waitingPostCompile = false;
            EditorApplication.delayCall -= DoPostInstallActions;
            if (_toggleAddInstanceToFirstScene != null && _toggleAddInstanceToFirstScene.value)
                AddInstanceToFirstScene();

            if (_toggleUseOmniSceneManager != null && _toggleUseOmniSceneManager.value)
                UseOmniSceneManagerMessage();
            
            if (_toggleDetectLoadingFunction != null && _toggleDetectLoadingFunction.value)
                DetectAndImplementLoadingFunction();
            Debug.Log("Post-install actions completed.");
        }

        #region Auto Installation
        private void AddInstanceToFirstScene()
        {
            if (EditorBuildSettings.scenes.Length == 0)
            {
                Debug.LogWarning("No scenes found in Build Settings. Cannot add InAppPurchaseHelper.");
                return;
            }

            string firstScenePath = EditorBuildSettings.scenes[0].path;
            if (string.IsNullOrEmpty(firstScenePath))
            {
                Debug.LogWarning("First scene path invalid.");
                return;
            }

            Debug.Log($"Opening first scene: {firstScenePath}");
            var scene = EditorSceneManager.OpenScene(firstScenePath, OpenSceneMode.Single);

            // Check if already exists
            InAppPurchaseHelper existing = Object.FindObjectOfType<InAppPurchaseHelper>(true);
            if (existing != null)
            {
                Debug.Log("InAppPurchaseHelper already exists in the scene. Skipping creation.");
                return;
            }

            // Find prefab in assets
            string[] guids = AssetDatabase.FindAssets("InAppPurchaseHelper t:prefab");
            if (guids.Length == 0)
            {
                Debug.LogError("Could not find prefab 'InAppPurchaseHelper' in project.");
                return;
            }

            string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Failed to load prefab at path: {prefabPath}");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Add InAppPurchaseHelper to scene");

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("InAppPurchaseHelper prefab added to first scene.");
        }

        private void UseOmniSceneManagerMessage()
        {
            const string typeName = "PopupController";
            const string targetScript = "HandleIAPEvent";
            const string targetMarker = "#if false //OMNILATENT_SCENEMANAGER_POPUP";
            const string replacement = "#if true //OMNILATENT_SCENEMANAGER_POPUP";

            // 1. If PopupController doesn't exist, prompt user to open HandleIAPEvent
            System.Type popupType = System.Type.GetType(typeName);
            if (popupType == null)
            {
                PromptOpenScriptIfTypeMissing(typeName, targetScript, "ShowErrorPopup");
                return;
            }

            // 2. If PopupController exists, patch the file
            ReplaceMarkerInFile(targetScript, targetMarker, replacement);
        }

        private void DetectAndImplementLoadingFunction()
        {
            const string typeName = "LoadingAnywhere";
            const string targetScript = "HandleIAPEvent";
            const string targetMarker = "#if false //OMNILATENT_SCENEMANAGER_LOADING";
            const string replacement = "#if true //OMNILATENT_SCENEMANAGER_LOADING";
            const string manualFunction = "OnToggleLoading";

            System.Type loadingType = System.Type.GetType(typeName);
            if (loadingType == null)
            {
                PromptOpenScriptIfTypeMissing(typeName, targetScript, manualFunction);
                return;
            }

            ReplaceMarkerInFile(targetScript, targetMarker, replacement);
        }
        #endregion
        
        #region Helper functions
        private bool ReplaceMarkerInFile(string assetSearchName, string targetMarker, string replacement)
        {
            string[] guids = AssetDatabase.FindAssets(assetSearchName + " t:script");
            if (guids == null || guids.Length == 0)
            {
                Debug.LogError($"Could not find script '{assetSearchName}' in project.");
                return false;
            }

            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            string scriptText = File.ReadAllText(scriptPath);

            if (!scriptText.Contains(targetMarker))
            {
                Debug.LogWarning($"Target marker not found in {scriptPath}: {targetMarker}");
                return false;
            }

            scriptText = scriptText.Replace(targetMarker, replacement);
            File.WriteAllText(scriptPath, scriptText);
            AssetDatabase.ImportAsset(scriptPath);
            Debug.Log($"Patched {Path.GetFileName(scriptPath)}: replaced '{targetMarker}' with '{replacement}'.");
            return true;
        }

        private void PromptOpenScriptIfTypeMissing(string typeName, string assetSearchName, string functionName = null)
        {
            System.Type type = System.Type.GetType(typeName);
            string[] guids = AssetDatabase.FindAssets(assetSearchName + " t:script");
            if (guids == null || guids.Length == 0)
            {
                Debug.LogError($"Could not find script '{assetSearchName}' in project.");
                return;
            }

            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            Object scriptAsset = AssetDatabase.LoadAssetAtPath<Object>(scriptPath);

            if (type != null)
                return; // Type exists, no prompt needed

            bool openScript = EditorUtility.DisplayDialog(
                "Manual Edit Required",
                $"{typeName} class not found.\n\nYou need to manually implement the {functionName ?? "required function"} in {assetSearchName}.\n\nDo you want to open the script now?",
                "Open Script",
                "Cancel"
            );

            if (openScript && scriptAsset != null)
            {
                int targetLine = string.IsNullOrEmpty(functionName) ? -1 : FindFunctionLine(scriptPath, functionName);
                if (targetLine > 0)
                {
                    AssetDatabase.OpenAsset(scriptAsset, targetLine);
                    Debug.Log($"Opened {scriptPath} at line {targetLine} ({functionName}).");
                }
                else
                {
                    AssetDatabase.OpenAsset(scriptAsset);
                    Debug.Log($"Opened {scriptPath} (function '{functionName}' not found).");
                }
            }
        }

        // Helper to find the line where a function is declared
        private int FindFunctionLine(string filePath, string functionName)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.Contains(functionName) && line.Contains("("))
                    {
                        // Rough heuristic to detect a function definition line
                        if (line.Contains("void") || line.Contains("public") || line.Contains("private") || line.Contains("protected"))
                            return i + 1; // +1 because Unity line numbers are 1-based
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error scanning {filePath}: {ex.Message}");
            }

            return -1;
        }
        #endregion

        // Called whenever scripts reload (EditorWindow auto-deserializes)
        private void OnEnable()
        {
            if (_waitingPostCompile && !EditorApplication.isCompiling)
            {
                EditorApplication.delayCall += DoPostInstallActions;
            }
        }

        #region Symbol
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
        #endregion
    }
}