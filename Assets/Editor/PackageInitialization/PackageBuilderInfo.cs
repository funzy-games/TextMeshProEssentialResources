namespace Editor.PackageInitialization
{
    public class PackageBuilderInfo
    {
        public readonly string CompanyName;
        public readonly string ProjectName;

        public PackageBuilderInfo(string companyName, string projectName)
        {
            CompanyName = companyName;
            ProjectName = projectName;
        }

        public string PackageID => $"com.{CompanyName.ToLower()}.{ProjectName.ToLower()}";

        public string RuntimeAssemblyName => $"{CompanyName}.{ProjectName}";
        public string RuntimeAssemblyID => RuntimeAssemblyName.ToLower();

        public string EditorAssemblyName => $"{RuntimeAssemblyName}.Editor";
        public string EditorAssemblyID => EditorAssemblyName.ToLower();
    }
}