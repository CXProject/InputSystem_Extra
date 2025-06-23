using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SimpleCodeGenerator;
using UnityEditor;
using UnityEngine.InputSystem;

namespace InputSystemExtra
{
    public static class InputMethodTemplateGenerator
    {
        class GenInfo
        {
            public string mapName;
            public string filePath;
        }
        
        private static async Task<GenInfo> GetGeneratePath(InputActionAsset asset, string defaultName)
        {
            if(asset == null) return null;
            var selectMap = SelectMapPopup.ShowWindow(asset);
            if(selectMap == null) return null;
            var mapName = await selectMap.WaitWindowClose();
            if(mapName == null) return null;
            
            var filePath = EditorUtility.SaveFilePanel("Save template", "Assets", $"{CodeGenHelper.CapitalizeFirstLetterWithCulture(mapName)}_{defaultName}", "cs");
            return new GenInfo { mapName = mapName, filePath = filePath };
        }

        [MenuItem("Assets/Input System Extra/Generate PlayerInput Template(UnityEvent)")]
        public static void GeneratePlayInputTemplateForUnityEvent()
        {
            GeneratePlayInputTemplateForEvent(true);
        }

        [MenuItem("Assets/Input System Extra/Generate PlayerInput Template(C#Event)")]
        public static void GeneratePlayInputTemplateForCSharpEvent()
        {
            GeneratePlayInputTemplateForEvent(false);
        }
        
        private async static void GeneratePlayInputTemplateForEvent(bool isUnityEvent)
        {
            var asset = Selection.activeObject as InputActionAsset;
            var genInfo = await GetGeneratePath(asset, "PlayerInput_Template");
            if (genInfo == null) return;
            // Create Common Input Template
            foreach (var map in asset.actionMaps)
            {
                if(map.name != genInfo.mapName) continue;
                var gens = CreatePlayerInputTemplateForEvent(asset, map, genInfo.filePath, isUnityEvent);
                if (gens == null) continue;
                gens.WriteToFile();
            }
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Generate PlayerInput Template", "Generate PlayerInput Template Success", "OK");
        }
        
        private static CSFileGenerator CreatePlayerInputTemplateForEvent(InputActionAsset asset, InputActionMap map, string filePath, bool isUnityEvent)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var folderPath = Path.GetDirectoryName(filePath);
            var gen = new CSFileGenerator
            {
                CompanyName = "LAPCAT STUDIOS",
                Name = fileName,
                FolderPath = folderPath,
                Desc = $"Player Input Template for {asset.name}/{map.name}.",
                Details = new[]
                {
                    $"Please used with PlayerInput Component file.",
                },
                NameSpace = "InputSystemExtra"
            };
            
            gen.AddUsingNameSpace("UnityEngine");
            gen.AddUsingNameSpace("UnityEngine.InputSystem");
            //add class
            var classGen = new ClassGen
            {
                ClassName = fileName,
                DeclarationFormat = "public class {0} : MonoBehaviour",
            };
            if (isUnityEvent == false)
            {
                classGen.Attribute = "[RequireComponent(typeof(PlayerInput))]";
            }

            gen.AddClass(classGen);

            if (isUnityEvent == false)
            {
                var playInputProperty = new ClassPropertyGen
                {
                    Declaration = $"private PlayerInput _input;",
                };
                classGen.AddProperty(playInputProperty);
                var startMethod = new ClassMethodGen
                {
                    Declaration = "private void Start()",
                };
                startMethod.AppendBodyLine("_input = GetComponent<PlayerInput>();");
                //add callback
                startMethod.AppendBodyLine("_input.onDeviceLost += OnDeviceLost;");
                startMethod.AppendBodyLine("_input.onDeviceRegained += OnDeviceRegained;");
                startMethod.AppendBodyLine("_input.onControlsChanged += OnControlsChanged;");
                foreach (var action in map.actions)
                {
                    startMethod.AppendBodyLine($"_input.onActionTriggered += On{CodeGenHelper.CapitalizeFirstLetterWithCulture(action.name)};");
                }
                classGen.AddMethod(startMethod);
            }
            GenDeviceCallbackMethod(classGen);
            //add callback method
            GenCallbackMethod(classGen, map, isUnityEvent, false, false);

            if (isUnityEvent == false)
            {
                var destroyMethod = new ClassMethodGen
                {
                    Declaration = "public void OnDestroy()",
                };
                destroyMethod.AppendBodyLine("if(_input == null) return;");
                //remove callback
                destroyMethod.AppendBodyLine("_input.onDeviceLost -= OnDeviceLost;");
                destroyMethod.AppendBodyLine("_input.onDeviceRegained -= OnDeviceRegained;");
                destroyMethod.AppendBodyLine("_input.onControlsChanged -= OnControlsChanged;");
                foreach (var action in map.actions)
                {
                    destroyMethod.AppendBodyLine($"_input.onActionTriggered -= On{CodeGenHelper.CapitalizeFirstLetterWithCulture(action.name)};");
                }
                classGen.AddMethod(destroyMethod);
            }

            return gen;
        }

        [MenuItem("Assets/Input System Extra/Generate PlayerInput Template(Send Message)")]
        public async static void GeneratePlayInputTemplate()
        {
            var asset = Selection.activeObject as InputActionAsset;
            var genInfo = await GetGeneratePath(asset, "PlayerInput_Template");
            if (genInfo == null) return;
            // Create Common Input Template
            foreach (var map in asset.actionMaps)
            {
                if(map.name != genInfo.mapName) continue;
                var gens = CreatePlayerInputTemplate(asset, map, genInfo.filePath);
                if (gens == null) continue;
                gens.WriteToFile();
            }
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Generate PlayerInput Template", "Generate PlayerInput Template Success", "OK");
        }

        private static CSFileGenerator CreatePlayerInputTemplate(InputActionAsset asset, InputActionMap map, string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var folderPath = Path.GetDirectoryName(filePath);
            var gen = new CSFileGenerator
            {
                CompanyName = "LAPCAT STUDIOS",
                Name = fileName,
                FolderPath = folderPath,
                Desc = $"Player Input Template for {asset.name}/{map.name}.",
                Details = new[]
                {
                    $"Please used with PlayerInput Component file.",
                },
                NameSpace = "InputSystemExtra"
            };
            
            gen.AddUsingNameSpace("UnityEngine");
            gen.AddUsingNameSpace("UnityEngine.InputSystem");
            //add class
            var classGen = new ClassGen
            {
                ClassName = fileName,
                DeclarationFormat = "public class {0} : MonoBehaviour",
            };
            gen.AddClass(classGen);

            GenDeviceCallbackMethod(classGen);
            //add callback method
            GenPlayerInputCallbackMethod(classGen, map);

            return gen;
        }

        private static void GenDeviceCallbackMethod(ClassGen classGen)
        {
            var deviceLostMethod = new ClassMethodGen
            {
                Declaration = "public void OnDeviceLost(PlayerInput input)",
            };
            deviceLostMethod.AppendBodyLine($"Debug.Log(\"Device Lost\");");
            classGen.AddMethod(deviceLostMethod);
            var deviceRegainedMethod = new ClassMethodGen
            {
                Declaration = "public void OnDeviceRegained(PlayerInput input)",
            };
            deviceRegainedMethod.AppendBodyLine($"Debug.Log(\"Device Regained\");");
            classGen.AddMethod(deviceRegainedMethod);
            var controlChangedMethod = new ClassMethodGen
            {
                Declaration = "public void OnControlsChanged(PlayerInput input)",
            };
            controlChangedMethod.AppendBodyLine($"Debug.Log(\"Controls Changed\");");
            classGen.AddMethod(controlChangedMethod);
        }

        private static void GenPlayerInputCallbackMethod(ClassGen gen, InputActionMap map)
        {
            foreach (var action in map.actions)
            {
                var parameter = action.type == InputActionType.Button ? "" : "InputValue value";
                var performMethod = new ClassMethodGen
                {
                    Declaration = $"public void On{CodeGenHelper.CapitalizeFirstLetterWithCulture(action.name)}({parameter})",
                };
                performMethod.AppendBodyLine($"Debug.Log(\"{action.name} is performed\");");
                //read value
                switch (action.type)
                {
                    case InputActionType.PassThrough:
                    case InputActionType.Value:
                        var valName = string.Empty;
                        switch (action.expectedControlType)
                        {
                            case "Vector2":
                                valName = "Vector2";
                                break;
                            case "Vector3":
                                valName = "Vector3";
                                break;
                            case "Axis":
                                valName = "Vector2";
                                break;
                        }
                        performMethod.AppendBodyLine($@"Debug.Log($""On{CodeGenHelper.CapitalizeFirstLetterWithCulture(action.name)} : {{value.Get<{valName}>()}}"");");
                        break;
                }
                gen.AddMethod(performMethod);
            }
        }

        [MenuItem("Assets/Input System Extra/Generate Common Input Template")]
        public async static void GenerateCommonInputTemplate()
        {
            var asset = Selection.activeObject as InputActionAsset;
            var genInfo = await GetGeneratePath(asset, "Template");
            if (genInfo == null) return;
            // Create Common Input Template
            foreach (var map in asset.actionMaps)
            {
                if(map.name != genInfo.mapName) continue;
                var gens = CreateCommonInputTemplate(asset.name, map, genInfo.filePath);
                if (gens == null) continue;
                gens.WriteToFile();
            }
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Generate Template", "Generate Template Success", "OK");
        }

        private static CSFileGenerator CreateCommonInputTemplate(string assetName, InputActionMap map, string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var folderPath = Path.GetDirectoryName(filePath);
            var wrapperName = $"{assetName}_{map.name}_Wrapper";
            var gen = new CSFileGenerator
            {
                CompanyName = "LAPCAT STUDIOS",
                Name = fileName,
                FolderPath = folderPath,
                Desc = $"Common Input Template for {assetName}/{map.name}.",
                Details = new[]
                {
                    $"Please used with the {assetName}_{map.name}_Wrapper.cs file.",
                },
                NameSpace = "InputSystemExtra"
            };
            
            gen.AddUsingNameSpace("UnityEngine");
            gen.AddUsingNameSpace("UnityEngine.InputSystem");
            
            //add class
            var classGen = new ClassGen
            {
                ClassName = fileName,
                DeclarationFormat = "public class {0} : MonoBehaviour",
            };
            gen.AddClass(classGen);

            var startMethod = new ClassMethodGen
            {
                Declaration = "private void Start()",
            };
            
            startMethod.AddIFSrcope($"CommonInputManager.Instance.TryGetActionMap<{wrapperName}>({wrapperName}.MAP_NAME, out var wrapper)",CreateRegisterEventScript(map, true));
            classGen.AddMethod(startMethod);
            
            //add callback method
            GenCallbackMethod(classGen, map,false, true, true);
            
            
            var destroyMethod = new ClassMethodGen
            {
                Declaration = "private void OnDestroy()",
            };
            destroyMethod.AppendBodyLine($"if(CommonInputManager.IsExist == false ) return;");
            destroyMethod.AddIFSrcope($"CommonInputManager.Instance.TryGetActionMap<{wrapperName}>({wrapperName}.MAP_NAME, out var wrapper)",CreateRegisterEventScript(map, false));
            classGen.AddMethod(destroyMethod);
            
            return gen;
        }

        private static string[] CreateRegisterEventScript(InputActionMap map, bool isRegister)
        {
            List<string> methodBody = new List<string>();
            var actionSingal = isRegister? "+" : "-";
            foreach (var action in map.actions)
            {
                methodBody.Add($"wrapper.{action.name}Action.started {actionSingal}= On{CodeGenHelper.CapitalizeFirstLetterWithCulture(action.name)}Start;");
                methodBody.Add($"wrapper.{action.name}Action.performed {actionSingal}= On{CodeGenHelper.CapitalizeFirstLetterWithCulture(action.name)};");
                methodBody.Add($"wrapper.{action.name}Action.canceled {actionSingal}= On{CodeGenHelper.CapitalizeFirstLetterWithCulture(action.name)}Cancel;");
            }
            return methodBody.ToArray();
        }

        private static void GenCallbackMethod(ClassGen gen, InputActionMap map, bool isPublic, bool genStartMethod, bool genCancelMethod)
        {
            var modifer = isPublic ? "public" : "private";
            foreach (var action in map.actions)
            {
                //add start method
                var startMethod = new ClassMethodGen
                {
                    Declaration = $"{modifer} void On{CodeGenHelper.CapitalizeFirstLetterWithCulture(action.name)}Start(InputAction.CallbackContext context)"
                };
                startMethod.AppendBodyLine($"Debug.Log($\"{action.name} is {{context.phase}}\");");
                gen.AddMethod(startMethod);
                
                //add perform Method
                var performMethod = new ClassMethodGen
                {
                    Declaration = $"{modifer} void On{CodeGenHelper.CapitalizeFirstLetterWithCulture(action.name)}(InputAction.CallbackContext context)",
                };
                performMethod.AppendBodyLine($"Debug.Log($\"{action.name} is {{context.phase}}\");");
                //read value
                switch (action.type)
                {
                    case InputActionType.PassThrough:
                    case InputActionType.Value:
                        var valName = string.Empty;
                        switch (action.expectedControlType)
                        {
                            case "Vector2":
                                valName = "Vector2";
                                break;
                            case "Vector3":
                                valName = "Vector3";
                                break;
                            case "Axis":
                                valName = "Vector2";
                                break;
                        }
                        performMethod.AppendBodyLine($@"Debug.Log($""On{CodeGenHelper.CapitalizeFirstLetterWithCulture(action.name)} : {{context.ReadValue<{valName}>()}}"");");
                        break;
                }
                gen.AddMethod(performMethod);

                //add cancel method
                var cancelMethod = new ClassMethodGen
                {
                    Declaration = $"{modifer} void On{CodeGenHelper.CapitalizeFirstLetterWithCulture(action.name)}Cancel(InputAction.CallbackContext context)"
                };
                cancelMethod.AppendBodyLine($"Debug.Log($\"{action.name} is {{context.phase}}\");");
                gen.AddMethod(cancelMethod);


            }
        }


    }
}