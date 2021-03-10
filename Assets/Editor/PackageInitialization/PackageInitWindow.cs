using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static Editor.PackageInitialization.HardcodedTemplates;

namespace Editor.PackageInitialization
{
    public class PackageInitWindow : EditorWindow
    {
        private const string PackageRegex = @"^[A-Z][a-zA-Z0-9]+$";
        private const int WindowWidth = 300;
        private const string GUIControlPackageName = "PackageName";
        private string _failedPackageName;
        private bool _generateEditorAssembly;
        private bool _nukePackageInitScripts = true;

        private string _projectName;
        private bool _validationFailed;
        private bool _wasInit;
        private static string PackageJsonPath => Path.Combine(Application.dataPath, "package.json");
        private static string EditorFolderPath => Path.Combine(Application.dataPath, "Editor");
        private static string ImagesFolderPath => Path.Combine(Application.dataPath, "../Images");
        private static string ReadMeFilePath => Path.Combine(Application.dataPath, "../README.md");
        private static string PackageInitialzationPath => Path.Combine(EditorFolderPath, "PackageInitialization");

        private void Awake()
        {
            _projectName = Directory.GetParent(Application.dataPath).Name;
        }

        private void OnGUI()
        {
            GUILayout.Space(20f);
            GUI.SetNextControlName(GUIControlPackageName);
            _projectName = EditorGUILayout.TextField("Package name", _projectName);
            _generateEditorAssembly = EditorGUILayout.Toggle("Generate Editor Assembly", _generateEditorAssembly);
            _nukePackageInitScripts = EditorGUILayout.Toggle("Self Destruct Initization", _nukePackageInitScripts);

            if (_validationFailed)
            {
                EditorGUILayout.HelpBox($"{_failedPackageName} is not a valid package name!" +
                                        $"\n\nPackages must begin with an uppercase letter, and contain only alphanumeric characters.'",
                    MessageType.Error);
            }

            if (!_wasInit)
            {
                _wasInit = true;
                GUI.FocusControl(GUIControlPackageName);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Init (Package.json, .asmdefs)"))
            {
                _validationFailed = !ValidatePackageName(_projectName);
                if (_validationFailed)
                {
                    _failedPackageName = _projectName;
                    return;
                }

                var info = new PackageBuilderInfo(DefaultConfiguration.CompanyName, _projectName);
                InitPackage(info, _generateEditorAssembly);

                if (_nukePackageInitScripts)
                {
                    NukePackageInitScripts();
                }

                Close();
            }

            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }

        [MenuItem("PackageTools/InitPackageProject")]
        public static void InitPackageProject()
        {
            var window = CreateInstance<PackageInitWindow>();

            var height = EditorGUIUtility.singleLineHeight * 12;
            window.position = new Rect(Screen.width / 2f, Screen.height / 2f, WindowWidth, height);
            window.titleContent.text = "Package Initializer";
            window.ShowAuxWindow();
        }

        private void NukePackageInitScripts()
        {
            Directory.Delete(ImagesFolderPath, true);
            File.Delete(ReadMeFilePath);
            Directory.Delete(_generateEditorAssembly ? PackageInitialzationPath : EditorFolderPath, true);
            Debug.Log("Nuked Package Initialization Scripts!");
        }

        private static void InitPackage(PackageBuilderInfo info, bool generateEditorAssembly)
        {
            var unityVersion = GetUnityVersion();

            Debug.Log($"Initializing package: name: {PackageJsonPath}. Unity Version: {unityVersion}");

            // Replace values
            GeneratePackageJson(info, PackageJsonTemplate, unityVersion);
            GenerateAsmdefs(info, generateEditorAssembly, overwriteOk: false);

            // Set names
            PlayerSettings.companyName = info.CompanyName;
            PlayerSettings.productName = info.ProjectName;
            AssetDatabase.Refresh();

            Debug.Log("Done Initializing!");
        }

        private static void GeneratePackageJson(PackageBuilderInfo info, string packageJsonText, string unityVersion)
        {
            Debug.Log($"Replacing package.json values. Raw Text: {packageJsonText}");

            packageJsonText = packageJsonText.Replace(PackageAssemblyName, info.ProjectName);
            packageJsonText = packageJsonText.Replace(PackageIdTag, info.PackageID);
            packageJsonText = packageJsonText.Replace(UnityVersionTag, unityVersion);


            File.WriteAllText(PackageJsonPath, packageJsonText);
            Debug.Log("Done replacing package.json!");
        }

        private static void GenerateAsmdefs(PackageBuilderInfo info, bool generateEditorAssembly, bool overwriteOk)
        {
            var runtimeFolder = Path.Combine(Application.dataPath, "Runtime");
            var editorFolder = Path.Combine(Application.dataPath, "Editor");

            CreateDirectoryIfNeeded(runtimeFolder);
            CreateDirectoryIfNeeded(editorFolder);

            var mainAsmdefPath = Path.Combine(runtimeFolder, $"{info.RuntimeAssemblyName}.asmdef");
            var editorAsmdefPath = Path.Combine(editorFolder, $"{info.EditorAssemblyName}.asmdef");

            // Create main assembly definition file
            new AssemblyDefinitionFileBuilder(info.RuntimeAssemblyID)
                .SetFilePath(mainAsmdefPath)
                .SetIsEditorAssembly(false)
                .CreateFile(overwriteOk);

            // Wait for AssetDatabase to create a meta (guid) for it
            AssetDatabase.Refresh();
            var mainAssetPath = FileUtil.GetProjectRelativePath(mainAsmdefPath);
            var mainGuid = AssetDatabase.AssetPathToGUID(mainAssetPath);

            if (generateEditorAssembly)
            {
                // Create editor asmdef + link with main meta
                new AssemblyDefinitionFileBuilder(info.EditorAssemblyID)
                    .SetFilePath(editorAsmdefPath)
                    .SetIsEditorAssembly(true)
                    .AddReferences(mainGuid) // Link with main 
                    .CreateFile(overwriteOk);
            }

            AssetDatabase.Refresh();
        }

        private static void CreateDirectoryIfNeeded(string dir)
        {
            if (Directory.Exists(dir)) return;
            if (string.IsNullOrEmpty(dir) || File.Exists(dir))
            {
                Debug.LogError($"Something went wrong! dir path: {dir}");
                return;
            }

            Directory.CreateDirectory(dir);
        }

        private static bool ValidatePackageName(string packageName) => Regex.Match(packageName, PackageRegex).Success;

        private static string GetUnityVersion()
        {
            var rawVersion = Application.unityVersion;
            return Regex.Match(rawVersion, @"\d+\.\d+").Value;
        }
    }
}