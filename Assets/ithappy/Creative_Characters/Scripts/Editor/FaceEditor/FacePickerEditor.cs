using System;
using CharacterCustomizationTool.FaceManagement;
using UnityEditor;
using UnityEngine;

namespace CharacterCustomizationTool.Editor.FaceEditor
{
    [CustomEditor(typeof(FacePicker))]
    public class FacePickerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var facePicker = (FacePicker)target;

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Next"))
                {
                    ShiftFace(facePicker, p => p.NextFace());
                }

                var centeredStyle = GUI.skin.GetStyle("Label");
                centeredStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label(facePicker.FaceName, centeredStyle, GUILayout.ExpandWidth(false), GUILayout.Width(100));

                if (GUILayout.Button("Previous"))
                {
                    ShiftFace(facePicker, p => p.PreviousFace());
                }
            }
        }

        private static void ShiftFace(FacePicker facePicker, Action<FacePicker> facePickAction)
        {
            facePickAction(facePicker);

            EditorUtility.SetDirty(facePicker.gameObject);
            AssetDatabase.SaveAssetIfDirty(facePicker.gameObject);
        }
    }
}