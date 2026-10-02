using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Unity.Formats.USD
{
    public class UsdBuildPostProcess
    {
        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            var source = "";
            var destination = "";
            if (target == BuildTarget.StandaloneLinux64)
            {
                source = Path.GetFullPath("Packages/com.unity.usd.core/Runtime/Plugins/x86_64/Linux/lib/usd");
                destination = pathToBuiltProject.Replace(".x86_64", "_Data/Plugins");
            }
            else if (target == BuildTarget.StandaloneOSX)
            {
                source = Path.GetFullPath("Packages/com.unity.usd.core/Runtime/Plugins/x86_64/MacOS/lib/usd");
                destination = pathToBuiltProject + "/Contents/Plugins";
            }
            else if (target == BuildTarget.StandaloneWindows64)
            {
                source = Path.GetFullPath("Packages/com.unity.usd.core/Runtime/Plugins/x86_64/Windows/lib/usd");
                destination = pathToBuiltProject.Replace(".exe", "_Data/Plugins/x86_64");
            }
            else
            {
                Debug.LogWarning("The USD package is not supported in non desktop builds. The USD plugins directory will not be included in the build.");
                return;
            }

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }
            else
            {
                var attrs = File.GetAttributes(destination);
                attrs &= ~FileAttributes.ReadOnly;
                File.SetAttributes(destination, attrs);
            }

            // We need to copy the whole share folder if not already in place
            // Ideally we should also check that all files/folders are present and match the source
            if (!Directory.Exists(destination + "/usd"))
            {
                FileUtil.CopyFileOrDirectory(source, destination + "/usd");
            }
        }

        static string GetCurrentDir([CallerFilePath] string filePath = "")
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.DirectoryName;
        }
    }
}
