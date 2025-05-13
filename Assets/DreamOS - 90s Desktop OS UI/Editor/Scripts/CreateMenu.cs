#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Michsky.DreamOS
{
    public class CreateMenu : Editor
    {
        static string objectPath;

        #region Methods & Helpers
        static void GetObjectPath()
        {
            objectPath = AssetDatabase.GetAssetPath(Resources.Load("UI Manager/DreamOS UI Manager"));
            objectPath = objectPath.Replace("Resources/UI Manager/DreamOS UI Manager.asset", "").Trim();
            objectPath = objectPath + "Prefabs/";
        }

        static void MakeSceneDirty(GameObject source, string sourceName)
        {
            if (Application.isPlaying == false)
            {
                Undo.RegisterCreatedObjectUndo(source, sourceName);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        static void SelectErrorDialog()
        {
            EditorUtility.DisplayDialog("DreamOS", "Cannot create the object due to missing manager file. " +
                    "Make sure you have a valid 'DreamOS UI Manager' file in DreamOS > Resources > UI Manager folder.", "Okay");
        }

        static void UpdateCustomEditorPath()
        {
            string darkPath = AssetDatabase.GetAssetPath(Resources.Load("DreamOS-EditorDark"));
            string lightPath = AssetDatabase.GetAssetPath(Resources.Load("DreamOS-EditorLight"));

            EditorPrefs.SetString("DreamOS.CustomEditorDark", darkPath);
            EditorPrefs.SetString("DreamOS.CustomEditorLight", lightPath);
        }
        #endregion

        #region Object Creating
        static void CreateObject(string resourcePath)
        {
            try
            {
                GetObjectPath();
                UpdateCustomEditorPath();

                GameObject clone = Instantiate(AssetDatabase.LoadAssetAtPath(objectPath + resourcePath + ".prefab", typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;

                try
                {
                    if (Selection.activeGameObject == null)
                    {
                        var canvas = (Canvas)GameObject.FindObjectsByType(typeof(Canvas), FindObjectsSortMode.None)[0];
                        clone.transform.SetParent(canvas.transform, false);
                    }

                    else { clone.transform.SetParent(Selection.activeGameObject.transform, false); }

                    clone.name = clone.name.Replace("(Clone)", "").Trim();
                    MakeSceneDirty(clone, clone.name);
                }

                catch
                {
                    CreateCanvas();
                    var canvas = (Canvas)GameObject.FindObjectsByType(typeof(Canvas), FindObjectsSortMode.None)[0];
                    clone.transform.SetParent(canvas.transform, false);
                    clone.name = clone.name.Replace("(Clone)", "").Trim();
                    MakeSceneDirty(clone, clone.name);
                }

                Selection.activeObject = clone;
            }

            catch { SelectErrorDialog(); }
        }

        static void CreateCanvas()
        {
            try
            {
                GetObjectPath();
                UpdateCustomEditorPath();

                GameObject clone = Instantiate(AssetDatabase.LoadAssetAtPath(objectPath + "UI Elements/Other/Canvas.prefab", typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
                clone.name = clone.name.Replace("(Clone)", "").Trim();
                Selection.activeObject = clone;

                MakeSceneDirty(clone, clone.name);
            }

            catch { SelectErrorDialog(); }
        }
        #endregion

        #region Button
        [MenuItem("GameObject/DreamOS/Button/Default Button")]
        static void DefaultButton()
        {
            CreateObject("UI Elements/Button/Default Button");
        }

        [MenuItem("GameObject/DreamOS/Button/Desktop Button")]
        static void DesktopButton()
        {
            CreateObject("UI Elements/Button/Desktop Button");
        }

        [MenuItem("GameObject/DreamOS/Button/Navbar Button")]
        static void NavbarButton()
        {
            CreateObject("UI Elements/Button/Navbar Button");
        }

        [MenuItem("GameObject/DreamOS/Button/QC Button")]
        static void QuickCenterButton()
        {
            CreateObject("UI Elements/Button/Quick Center Button");
        }

        [MenuItem("GameObject/DreamOS/Button/QC Button (Sub)")]
        static void QuickCenterSubButton()
        {
            CreateObject("UI Elements/Button/Quick Center Sub Button");
        }

        [MenuItem("GameObject/DreamOS/Button/QC Category")]
        static void QuickCenterCatButton()
        {
            CreateObject("UI Elements/Button/Quick Center Category");
        }

        [MenuItem("GameObject/DreamOS/Button/Taskbar Button")]
        static void TaskbarButton()
        {
            CreateObject("UI Elements/Button/Taskbar Button");
        }
        #endregion

        #region Input Field
        [MenuItem("GameObject/DreamOS/Input Field/Standard")]
        static void StandardInputField()
        {
            CreateObject("UI Elements/Input Field/Input Field");
        }
        #endregion

        #region Modal Window
        [MenuItem("GameObject/DreamOS/Modal Window/Standard")]
        static void ModalWindow()
        {
            CreateObject("UI Elements/Modal Window/Standard Modal Window");
        }
        #endregion

        #region Scrollbar
        [MenuItem("GameObject/DreamOS/Scrollbar/Horizontal")]
        static void ScrollbarHorizontal()
        {
            CreateObject("UI Elements/Scrollbar/Scrollbar Horizontal");
        }

        [MenuItem("GameObject/DreamOS/Scrollbar/Vertical")]
        static void ScrollbarVertical()
        {
            CreateObject("UI Elements/Scrollbar/Scrollbar Vertical");
        }
        #endregion

        #region Selectors
        [MenuItem("GameObject/DreamOS/Selectors/Horizontal Selector")]
        static void HorizontalSelector()
        {
            CreateObject("UI Elements/Selectors/Horizontal Selector");
        }

        [MenuItem("GameObject/DreamOS/Selectors/Vertical Selector")]
        static void VerticalSelector()
        {
            CreateObject("UI Elements/Selectors/Vertical Selector");
        }
        #endregion

        #region Slider
        [MenuItem("GameObject/DreamOS/Slider/Standard")]
        static void Slider()
        {
            CreateObject("UI Elements/Slider/Slider");
        }
        #endregion

        #region Switch
        [MenuItem("GameObject/DreamOS/Switch/Standard")]
        static void Switch()
        {
            CreateObject("UI Elements/Switch/Switch");
        }
        #endregion

        #region UIM
        [MenuItem("GameObject/DreamOS/UI Manager/Image")]
        static void UIMImage()
        {
            CreateObject("UI Elements/UIM/Image");
        }

        [MenuItem("GameObject/DreamOS/UI Manager/Text (TMP)")]
        static void UIMText()
        {
            CreateObject("UI Elements/UIM/Text (TMP)");
        }
        #endregion

        #region Others
        [MenuItem("GameObject/DreamOS/Others/Information Popup")]
        static void InformationPopup()
        {
            CreateObject("UI Elements/Other/Information Popup");
        }
        #endregion
    }
}
#endif