using System.IO;
using SimpleCodeGenerator;
using UnityEditor;
using UnityEngine.InputSystem;

namespace InputSystemExtra
{
    public static class ActionMapWrapperGenerator
    {
        private const string k_WrapperFolder = "InputSystemExtra";
        
        [MenuItem("Assets/Input System Extra/Generate ActionMapWrapper")]
        private static void GenerateWrapper()
        {
            var asset = Selection.activeObject as InputActionAsset;
            if(asset == null) return;
            
            //check folder exists
            var path = AssetDatabase.GetAssetPath(asset);
            var rootFolder = Path.GetDirectoryName(path);
            var folderPath = Path.Combine(rootFolder, k_WrapperFolder);
            
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            //createWrapperFile
            foreach (var map in asset.actionMaps)
            {
                var gens = CreateWrapperFile(asset.name, map, folderPath);
                if (gens == null) continue;
                gens.WriteToFile();
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Generate ActionMapWrapper", "Generate ActionMapWrapper Success", "OK");
        }

        private static CSFileGenerator CreateWrapperFile(string assetName, InputActionMap map, string folderPath)
        {
            var fileName = $"{assetName}_{map.name}_Wrapper";
            var gen = new CSFileGenerator
            {
                CompanyName = "LAPCAT STUDIOS",
                Name = fileName,
                FolderPath = folderPath,
                Desc = $"Wrapper for {map.name}ActionMap.",
                Details = new[]
                {
                    "Please add this component above the CommonInputManager.",
                    "PLEASE DON'T EDIT IT!!!"
                },
                NameSpace = "InputSystemExtra"
            };
            gen.AddUsingNameSpace("UnityEngine");
            gen.AddUsingNameSpace("UnityEngine.InputSystem");

            //add class
            var classGen = new ClassGen
            {
                ClassName = fileName,
                DeclarationFormat = "public class {0} : MonoBehaviour , IActionMapWrapper",
            };
            gen.AddClass(classGen);

            classGen.AddProperty(new ClassPropertyGen
            {
                Declaration = $@"public static string MAP_NAME = ""{map.name}"";",
            });
            classGen.AddProperty(new ClassPropertyGen
            {
                Declaration = "public string mapName => MAP_NAME;",
            });
            classGen.AddProperty(new ClassPropertyGen
            {
                Declaration = "public bool enabledOnStart = true;",
            });

            foreach (var action in map.actions)
            {
                classGen.AddProperty(new ClassPropertyGen
                {
                    Desc = $"{action.type} type : {action.expectedControlType}",
                    Declaration = $"public InputAction {action.name}Action {{ get; private set; }}",
                });
            }

            classGen.AddProperty(new ClassPropertyGen
            {
                Declaration = "private InputActionMap _map;",
            });

            var enableMethod = new ClassMethodGen()
            {
                Declaration = $"public bool mapEnabled"
            };
            enableMethod.AppendBodyLine("get => _map.enabled;");
            enableMethod.AppendBodyLine("set {");
            enableMethod.AddIFSrcope("value", new[]
            {
                "_map.Enable();"
            }, new[]
            {
                "_map.Disable();"
            });
            enableMethod.AppendBodyLine("}");
            classGen.AddMethod(enableMethod);
            
            //InitializeMap
            var initMethod = new ClassMethodGen()
            {
                Declaration = "public void InitializeMap(InputActionMap map)"
            };
            initMethod.AppendBodyLine("_map = map;");
            foreach (var action in map.actions)
            {
                initMethod.AppendBodyLine($"{action.name}Action = _map.FindAction(\"{action.name}\");");
            }

            initMethod.AddIFSrcope("enabledOnStart", new[]
            {
                "_map.Enable();"
            }, new[]
            {
                "_map.Disable();"
            });
            classGen.AddMethod(initMethod);


        return gen;
        }
    }
}