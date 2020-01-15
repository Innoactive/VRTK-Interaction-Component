using Innoactive.Hub.Interaction;
using UnityEditor;
using UnityEngine;
using VRTK;

namespace Innoactive.Hub.Training.Utils
{
    /// <summary>
    /// Settings for <see cref="SnapDropZone"/>s for e.g. automatic creation of such snap zones.
    /// </summary>
    [CreateAssetMenu(fileName = "SnapZoneSettings", menuName = "Innoactive/SnapZoneSettings", order = 1)]
    public class SnapZoneSettings : ScriptableObject
    {
        /// <summary>
        /// The Snap Type to apply when a valid interactable object is dropped within the snap zone.
        /// </summary>
        [Tooltip("The Snap Type to apply when a valid interactable object is dropped within the snap zone.")]
        public VRTK_SnapDropZone.SnapTypes SnapType = VRTK_SnapDropZone.SnapTypes.UseKinematic;

        /// <summary>
        /// The amount of time it takes for the object being snapped to move into the new snapped position, rotation and scale.
        /// </summary>
        [Tooltip("The amount of time it takes for the object being snapped to move into the new snapped position, rotation and scale.")]
        public float SnapDuration = 0f;

        /// <summary>
        /// The colour to use when showing the snap zone is active. This is used as the highlight colour when no object is hovering but `Highlight Always Active` is true.
        /// </summary>
        [Tooltip("The colour to use when showing the snap zone is active. This is used as the highlight colour when no object is hovering but `Highlight Always Active` is true.")]
        public Color HighlightColor = new Color32(64, 200, 255, 50);

        /// <summary>
        /// The colour to use when showing the snap zone is active and a valid object is hovering. If this is `Color.clear` then the `Highlight Color` will be used.
        /// </summary>
        [Tooltip("The colour to use when showing the snap zone is active and a valid object is hovering. If this is `Color.clear` then the `Highlight Color` will be used.")]
        public Color ValidHighlightColor = new Color32(0, 255, 0, 50);

        /// <summary>
        /// The highlight object will always be displayed when the snap drop zone is available even if a valid item isn't being hovered over.
        /// </summary>
        [Tooltip("The highlight object will always be displayed when the snap drop zone is available even if a valid item isn't being hovered over.")]
        public bool HighlightAlwaysActive = true;

        /// <summary>
        /// The material used for the highlight object. Should be transparent.
        /// </summary>
        [SerializeField]
        [Tooltip("The material used for the highlight object. Should be transparent.")]
        private Material highlightMaterial;

        /// <summary>
        /// The material used for the highlight object. Should be transparent.
        /// </summary>
        public Material HighlightMaterial
        {
            get
            {
                if (highlightMaterial == null)
                {
                    Material newHighlightMaterial = CreateHighlightMaterial();
                    highlightMaterial = newHighlightMaterial;
                }

                return highlightMaterial;
            }
            set
            {
                highlightMaterial = value;
            }
        }

        private Material CreateHighlightMaterial()
        {
            Shader standardShader = Shader.Find("Standard");

            Material material = new Material(standardShader);
            material.SetFloat("_Mode", 3);
            material.name = "SnapZoneHighlightMaterial";

#if UNITY_EDITOR
            AssetDatabase.CreateAsset(material, "Assets/Resources/SnapZoneHighlightMaterial.mat");
            AssetDatabase.SaveAssets();
#endif

            return material;
        }
    }
}
