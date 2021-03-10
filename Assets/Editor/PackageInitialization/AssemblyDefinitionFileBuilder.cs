using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Editor.PackageInitialization.HardcodedTemplates;

namespace Editor.PackageInitialization
{
    public class AssemblyDefinitionFileBuilder
    {
        private readonly string _assemblyId;
        private readonly List<string> _referencedGuids = new List<string>();
        private bool _isEditor;
        private string _path;

        public AssemblyDefinitionFileBuilder(string assemblyId)
        {
            _assemblyId = assemblyId;
        }

        public string PlatformsString => _isEditor ? "\"Editor\"" : "";

        private string ReferencesString => string.Join(", ", _referencedGuids);

        public AssemblyDefinitionFileBuilder SetFilePath(string filePath)
        {
            _path = filePath;
            return this;
        }

        public AssemblyDefinitionFileBuilder AddReferences(params string[] guids)
        {
            var wrappedGuids = guids.Select(guid => $"\"GUID:{guid}\""); // Wrap guids in quotes + prefix
            _referencedGuids.AddRange(wrappedGuids);
            return this;
        }

        public AssemblyDefinitionFileBuilder SetIsEditorAssembly(bool isEditor)
        {
            _isEditor = isEditor;
            return this;
        }

        public void CreateFile(bool overwriteOk)
        {
            if (string.IsNullOrEmpty(_path)) return;
            if (File.Exists(_path))
            {
                if (overwriteOk)
                {
                    File.Delete(_path);
                }
                else
                {
                    Debug.Log($"File already exists at path {_path}!");
                    return;
                }
            }

            using (var file = File.CreateText(_path))
            {
                var text = GetFileText();
                file.Write(text);
            }
        }

        private string GetFileText()
        {
            return AssemblyDefinitionTemplate
                .Replace(AssemblyReferencesTag, ReferencesString)
                .Replace(AssemblyPlatformsTag, PlatformsString)
                .Replace(PackageAssemblyName, _assemblyId);
        }
    }
}