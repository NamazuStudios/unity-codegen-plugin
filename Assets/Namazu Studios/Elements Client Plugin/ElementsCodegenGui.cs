#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Elements.Codegen
{
    public class ElementsCodegenGui : EditorWindow
    {
        private static CodegenUtils codegen = new CodegenUtils();

        private GUIStyle wordWrapStyle;
        private GUIStyle buttonStyle;
        private GUIStyle warningTextStyle;

        private string userId;
        private string password;

        private bool isLoading;

#region Editor GUI

        [MenuItem("Window/Elements/Elements Codegen")]
        public static void ShowWindow()
        {
            GetWindow(typeof(ElementsCodegenGui), false, "Elements Codegen", false);
        }

        private void OnGUI()
        {
            codegen.CheckConfiguration();

            CheckGuiStyles();

            GUILayout.Label("Elements Codegen Utility", EditorStyles.boldLabel);

            DrawUILine(Color.gray, 5);

            EditorGUILayout.BeginVertical();

            codegen.UserId = userId = EditorGUILayout.TextField("Admin (SUPERUSER) user id", userId);
            codegen.Password = password = EditorGUILayout.PasswordField("Password", password);

            DrawUILine(Color.gray, 1);

            EditorGUILayout.LabelField("This will be the root namespace of the generated code, with .Api and .Model appended where appropriate.", wordWrapStyle);
            codegen.Config.packageName = EditorGUILayout.TextField("Package Name", codegen.Config.packageName);

            DrawUILine(Color.gray, 1);

            EditorGUILayout.LabelField("The version of the generated code.", wordWrapStyle);
            codegen.Config.packageVersion = EditorGUILayout.TextField("Version", codegen.Config.packageVersion);

            DrawUILine(Color.gray, 1);

            EditorGUILayout.LabelField("The output folder, relative to the Assets folder in your project.", wordWrapStyle);
            codegen.Config.relativeDirectory = EditorGUILayout.TextField("Output Directory", codegen.Config.relativeDirectory);

            DrawUILine(Color.gray, 1);

            EditorGUILayout.LabelField("The root url to download the Swagger JSON from.", wordWrapStyle);
            codegen.Config.urlRoot = EditorGUILayout.TextField("Root URL", codegen.Config.urlRoot);

            DrawUILine(Color.gray, 1);

            EditorGUILayout.LabelField("The name of the application as it is set in the admin panel.", wordWrapStyle);
            codegen.Config.applicationName = EditorGUILayout.TextField("Application Name", codegen.Config.applicationName);

            DrawUILine(Color.gray, 1);

            if (GUILayout.Button(isLoading ? "Generating code..." : "Generate Core Elements Code", buttonStyle))
            {
                if (!isLoading)
                {
                    isLoading = true;

                    codegen.GenerateCode(null);

                    isLoading = false;
                }
            }

            if (!isLoading && GUILayout.Button("Generate Custom Element Code", buttonStyle))
            {
                codegen.GenerateCode(codegen.Config.applicationName);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a UI Line. Can only be used in OnGUI
        /// </summary>
        /// <param name="color"></param>
        /// <param name="thickness"></param>
        /// <param name="padding"></param>
        private void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }


        /// <summary>
        /// If the haven't been created already, creates the GUI styles to be used in this window.
        /// </summary>
        private void CheckGuiStyles()
        {
            if (wordWrapStyle == null)
            {
                wordWrapStyle = GUI.skin.GetStyle("Label");
                wordWrapStyle.wordWrap = true;
                wordWrapStyle.fontStyle = FontStyle.Italic;
                wordWrapStyle.padding = new RectOffset { bottom = 5, top = 15, left = 5, right = 5 };
            }

            if (warningTextStyle == null)
            {
                warningTextStyle = GUI.skin.GetStyle("Label");
                warningTextStyle.wordWrap = true;
                warningTextStyle.fontStyle = FontStyle.Bold;
                warningTextStyle.padding = new RectOffset { bottom = 5, top = 15, left = 5, right = 5 };
            }

            if (buttonStyle == null)
            {
                buttonStyle = GUI.skin.GetStyle("Button");
                buttonStyle.margin = new RectOffset { bottom = 5, top = 15, left = 5, right = 5 };
            }
        }

#endregion

    }

}
#endif