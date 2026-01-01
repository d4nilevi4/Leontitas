#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Leontitas.Editor
{
    [InitializeOnLoad]
    public static class LeontitasDependencyInstaller
    {
        private const string LeoEcsLitePackageName = "com.leopotam.ecslite";
        private const string LeoEcsLiteGitUrl = "https://github.com/Leopotam/ecslite.git";
        private const string PrefsKey = "Leontitas.DependencyCheckDone";

        static LeontitasDependencyInstaller()
        {
            // Only check once per session to avoid spam
            if (SessionState.GetBool(PrefsKey, false))
                return;

            SessionState.SetBool(PrefsKey, true);
            CheckForLeoEcsLite();
        }

        private static void CheckForLeoEcsLite()
        {
            var listRequest = Client.List(true, false);
            EditorApplication.update += () => OnListCompleted(listRequest);
        }

        private static void OnListCompleted(ListRequest request)
        {
            if (!request.IsCompleted)
                return;

            EditorApplication.update -= () => OnListCompleted(request);

            if (request.Status != StatusCode.Success)
                return;

            bool hasLeoEcsLite = request.Result.Any(package => package.name == LeoEcsLitePackageName);

            if (!hasLeoEcsLite)
            {
                ShowInstallDialog();
            }
        }

        private static void ShowInstallDialog()
        {
            bool shouldInstall = EditorUtility.DisplayDialog(
                "Leontitas: Missing Dependency",
                $"Leontitas requires the LeoEcsLite package ({LeoEcsLitePackageName}).\n\n" +
                "Would you like to install it automatically?",
                "Install Now",
                "Cancel"
            );

            if (shouldInstall)
            {
                var addRequest = Client.Add(LeoEcsLiteGitUrl);
                EditorApplication.update += () => OnAddCompleted(addRequest);
            }
            else
            {
                UnityEngine.Debug.LogWarning(
                    "[Leontitas] LeoEcsLite is required. Install it manually:\n" +
                    $"Package Manager > Add package from git URL > {LeoEcsLiteGitUrl}"
                );
            }
        }

        private static void OnAddCompleted(AddRequest request)
        {
            if (!request.IsCompleted)
                return;

            EditorApplication.update -= () => OnAddCompleted(request);

            if (request.Status == StatusCode.Success)
            {
                UnityEngine.Debug.Log($"[Leontitas] Successfully installed {LeoEcsLitePackageName}");
            }
            else
            {
                UnityEngine.Debug.LogError(
                    $"[Leontitas] Failed to install {LeoEcsLitePackageName}: {request.Error.message}"
                );
            }
        }
    }
}
#endif
