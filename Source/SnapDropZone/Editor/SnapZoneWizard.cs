using Innoactive.Hub.Interaction;
using Innoactive.Hub.Training.SceneObjects.Properties;
using Innoactive.Hub.Training.Utils;
using UnityEditor;
using UnityEngine;
using VRTK;

namespace Innoactive.Hub.Training.Editors.Windows
{
    /// <summary>
    /// Editor Window used to setup settings for <see cref="SnapDropZone"/>s
    /// Can be used e.g. for the creation functionality provided in <see cref="SnappableProperty"/>.
    /// </summary>
    public class SnapZoneWizard : EditorWindow
    {
        private static SnapZoneWizard window;
        private const string menuPath = "Innoactive/Creator/Utilities/Snap Zone Settings...";

        private VRTK_SnapDropZone.SnapTypes snapType;
        private float snapDuration = 0f;
        private bool highlightAlwaysActive = true;
        private Color highlightColor = new Color32(64, 200, 255, 50);
        private Color validHighlightColor = new Color32(0, 255, 0, 50);
        private Material highlightMaterial;
        private SnapZoneSettings settings;

        [MenuItem(menuPath, false, 12)]
        private static void ShowWizard()
        {
            window = GetWindow<SnapZoneWizard>();

            window.Show();
            window.Focus();
        }

        private void OnGUI()
        {
            titleContent = new GUIContent("Snap Zone Settings");

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("These settings help you to configure Snap Zones within your training. You can define colors and other values that will be set to Snap Zones created by clicking the 'Create Snap Zone' button of a Snappable Property.", MessageType.Info);
            EditorGUILayout.Space();

            if (settings == null)
            {
                settings = Resources.Load<SnapZoneSettings>("SnapZoneSettings");
                if (settings == null)
                {
                    settings = CreateInstance<SnapZoneSettings>();
                    AssetDatabase.CreateAsset(settings, "Assets/Resources/SnapZoneSettings.asset");
                    AssetDatabase.SaveAssets();
                }

                snapType = settings.SnapType;
                snapDuration = settings.SnapDuration;
                highlightColor = settings.HighlightColor;
                validHighlightColor = settings.ValidHighlightColor;
                highlightAlwaysActive = settings.HighlightAlwaysActive;

                if (settings.HighlightMaterial == null)
                {
                    settings.HighlightMaterial = Resources.Load<Material>("SnapZoneMaterials/HighlightMaterial");
                }
                highlightMaterial = settings.HighlightMaterial;
            }

            snapType = (VRTK_SnapDropZone.SnapTypes)EditorGUILayout.EnumPopup(new GUIContent("Snap Type", "The Snap Type to apply when a valid interactable object is dropped within the snap zone."), snapType);
            snapDuration = EditorGUILayout.FloatField(new GUIContent("Snap Duration", "The amount of time it takes for the object being snapped to move into the new snapped position, rotation and scale."), snapDuration);
            highlightColor = EditorGUILayout.ColorField(new GUIContent("Highlight Color", "The colour to use when showing the snap zone is active. This is used as the highlight colour when no object is hovering but `Highlight Always Active` is true."), highlightColor);
            validHighlightColor = EditorGUILayout.ColorField(new GUIContent("Valid Highlight Color", "The colour to use when showing the snap zone is active and a valid object is hovering. If this is `Color.clear` then the `Highlight Color` will be used."), validHighlightColor);
            highlightAlwaysActive = EditorGUILayout.Toggle(new GUIContent("Highlight Always Active", "The highlight object will always be displayed when the snap drop zone is available even if a valid item isn't being hovered over."), highlightAlwaysActive);
            highlightMaterial = EditorGUILayout.ObjectField(new GUIContent("Highlight Material", "The material used for the highlight object. Should be transparent."), highlightMaterial, typeof(Material), false) as Material;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("'Save And Apply' will additionally apply these settings to all Snap Zones in the current scene.", MessageType.Info);
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool isSaved = GUILayout.Button("Save Settings");
            bool isSavedAndApplied = GUILayout.Button("Save and Apply");
            GUILayout.EndHorizontal();

            if (isSaved || isSavedAndApplied)
            {
                settings.SnapType = snapType;
                settings.SnapDuration = snapDuration;
                settings.HighlightColor = highlightColor;
                settings.ValidHighlightColor = validHighlightColor;
                settings.HighlightMaterial = highlightMaterial;
                settings.HighlightAlwaysActive = highlightAlwaysActive;

                EditorUtility.SetDirty(settings);
            }

            if (isSavedAndApplied)
            {
                SnapDropZone[] snapZones = Resources.FindObjectsOfTypeAll<SnapDropZone>();

                foreach (SnapDropZone snapZone in snapZones)
                {
                    snapZone.snapType = settings.SnapType;
                    snapZone.highlightColor = settings.HighlightColor;
                    snapZone.validHighlightColor = settings.ValidHighlightColor;
                    snapZone.highlightAlwaysActive = settings.HighlightAlwaysActive;
                    snapZone.snapDuration = settings.SnapDuration;
                }
            }
        }
    }
}
