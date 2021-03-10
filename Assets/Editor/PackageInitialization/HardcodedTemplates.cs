namespace Editor.PackageInitialization
{
    public static class HardcodedTemplates
    {
        public const string PackageAssemblyName = "<PACKAGE-NAME>";
        public const string PackageIdTag = "<PACKAGE-ID>";
        public const string AssemblyReferencesTag = "<REFERENCES>";
        public const string AssemblyPlatformsTag = "<PLATFORMS>";
        public const string UnityVersionTag = "<UNITY-VERSION>";

        #region PackageJson

        public const string PackageJsonTemplate = @"{
  ""name"": """ + PackageIdTag + @""",
  ""displayName"": """ + PackageAssemblyName + @""",
  ""unity"": """ + UnityVersionTag + @""",
  ""description"": """ + DefaultConfiguration.CompanyName + " " + PackageAssemblyName + @" package"",
  ""version"": ""1.0.0"",
    ""dependencies"": {
    }
}";

        #endregion

        #region Asmdef

        public const string AssemblyDefinitionTemplate = @"{
    ""name"": """ + PackageAssemblyName + @""",
    ""references"": [" + AssemblyReferencesTag + @"],
    ""includePlatforms"": [" + AssemblyPlatformsTag + @"],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": []
}";

        #endregion
    }
}