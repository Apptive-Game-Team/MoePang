using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Editor
{
    public class CustomShaderCreator : EndNameEditAction
    {
        private const string CustomURPUnlitTemplatePath =
            "Assets/06.Effects/Shader/ShaderTemplate/CustomURPUnlitShader.shader.txt";

        [MenuItem("Assets/Create/Shader/CustomURPUnlit", false, 83)]
        private static void CreateCustomUnlitShader()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                CreateInstance<CustomShaderCreator>(),
                "NewCustomUnlit.shader",
                GetShaderIcon(),
                GetTemplate()
            );
        }

        private static string GetTemplate()
        {
            string fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                CustomURPUnlitTemplatePath
            );

            if (!File.Exists(fullPath))
            {
                Debug.LogError(
                    $"Custom Unlit Shader Template을 찾을 수 없습니다.\n" +
                    $"경로: {CustomURPUnlitTemplatePath}"
                );

                return string.Empty;
            }

            return File.ReadAllText(fullPath);
        }

        private static Texture2D GetShaderIcon()
        {
            return EditorGUIUtility
                .IconContent("Shader Icon")
                .image as Texture2D;
        }

        public override void Action(
            int instanceId,
            string pathName,
            string resourceFile)
        {
            // 파일 이름에서 확장자 제거
            string shaderName =
                Path.GetFileNameWithoutExtension(pathName);

            // 템플릿의 Shader 이름 변경
            resourceFile = resourceFile.Replace(
                "#SHADER_NAME#",
                shaderName
            );

            // Shader 파일 생성
            File.WriteAllText(
                pathName,
                resourceFile
            );

            // AssetDatabase 갱신
            AssetDatabase.ImportAsset(pathName);

            // 생성된 Shader 선택
            Object shader =
                AssetDatabase.LoadAssetAtPath<Object>(pathName);

            Selection.activeObject = shader;
            EditorGUIUtility.PingObject(shader);
        }
    }
}