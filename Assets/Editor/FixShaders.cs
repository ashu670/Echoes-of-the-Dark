using UnityEngine;
using UnityEditor;

public class FixShaders : EditorWindow
{
    [MenuItem("Tools/Fix Non-HDRP Shaders")]
    static void FixAllShaders()
    {
        string[] guids = AssetDatabase.FindAssets("t:material");

        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null) continue;

            if (mat.shader == null) continue;

            string shaderName = mat.shader.name;

            // Skip valid HDRP shaders
            if (shaderName.Contains("HDRP")) continue;

            // Replace everything else
            Shader hdrpLit = Shader.Find("HDRP/Lit");

            if (hdrpLit != null)
            {
                mat.shader = hdrpLit;
                EditorUtility.SetDirty(mat);
                fixedCount++;
                Debug.Log("Replaced shader on: " + path + " (" + shaderName + ")");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Shader fix complete. Total replaced: " + fixedCount);
    }
}