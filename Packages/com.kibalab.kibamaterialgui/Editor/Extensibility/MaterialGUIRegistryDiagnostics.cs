#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace KIBA_.KIBAMaterialGUI.Editor.Extensibility
{
    internal static class MaterialGUIRegistryDiagnostics
    {
        private static readonly HashSet<string> s_Keys = new();

        internal static void ReportCallbackFailure(object extension, string callback, System.Exception exception)
        {
            var type = extension.GetType().FullName;
            WarnOnce($"callback:{type}:{callback}:{exception.GetType().FullName}",
                $"Extension '{type}' failed in {callback}: {exception}");
        }

        public static void WarnOnce(string key, string message)
        {
            if (string.IsNullOrEmpty(key)) key = message;
            if (!s_Keys.Add(key)) return;
            Debug.LogWarning("[KIBAMaterialGUI] " + message);
        }

#if UNITY_INCLUDE_TESTS
        internal static void ResetForTests()
        {
            s_Keys.Clear();
        }
#endif
    }
}


