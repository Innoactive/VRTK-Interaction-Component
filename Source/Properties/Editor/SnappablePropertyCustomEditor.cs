using System.Collections.Generic;
using System.Linq;
using Innoactive.Hub.Interaction;
using Innoactive.Hub.Training.Utils;
using UnityEditor;
using UnityEngine;

namespace Innoactive.Hub.Training.SceneObjects.Properties.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="SnappableProperty"/>, adding a button to create <see cref="SnapDropZone"/>s automatically.
    /// </summary>
    [CustomEditor(typeof(SnappableProperty))]
    public class SnappablePropertyCustomEditor : UnityEditor.Editor
    {
        private const string prefabPath = "Assets/Resources/SnapZonePrefabs";

        public override void OnInspectorGUI()
        {
            SnappableProperty snappable = (SnappableProperty)target;

            // Show default inspector property editor
            DrawDefaultInspector();

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Snap Zone"))
            {
                CreateSnapZone(snappable);
            }
        }

        private void CreateSnapZone(SnappableProperty snappable)
        {
            SnapZoneSettings settings = Resources.Load<SnapZoneSettings>("SnapZoneSettings");

            if (settings == null)
            {
                settings = CreateInstance<SnapZoneSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/Resources/SnapZoneSettings.asset");
                AssetDatabase.SaveAssets();
            }

            GameObject snapZoneBlueprint = DuplicateObject(snappable.gameObject);
            SetHighlightMaterial(snapZoneBlueprint, settings.HighlightMaterial);

            if (AssetDatabase.IsValidFolder(prefabPath) == false)
            {
                string[] folders = prefabPath.Split('/');
                string subPath = folders[0]; // must be Assets

                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = subPath + "/" + folders[i];
                    if (AssetDatabase.IsValidFolder(newPath) == false)
                    {
                        AssetDatabase.CreateFolder(subPath, folders[i]);
                        subPath = newPath;
                    }
                }
            }

#if UNITY_2018_3_OR_NEWER
            GameObject snapZonePrefab = PrefabUtility.SaveAsPrefabAsset(snapZoneBlueprint, prefabPath + "/" + snappable.name + "Highlight.prefab");
#elif UNITY_2017_4_OR_NEWER
            GameObject snapZonePrefab = PrefabUtility.CreatePrefab(prefabPath + "/" + snappable.name + "Highlight.prefab", snapZoneBlueprint);
#endif

            snapZoneBlueprint.name = snappable.name + "SnapZone";
            snapZoneBlueprint.transform.position = Vector3.zero;
            snapZoneBlueprint.transform.rotation = Quaternion.identity;

            snapZoneBlueprint.AddComponent<SnapZoneProperty>();
            SnapDropZone snapDropZone = snapZoneBlueprint.GetComponent<SnapDropZone>();
            snapDropZone.snapType = settings.SnapType;
            snapDropZone.highlightColor = settings.HighlightColor;
            snapDropZone.validHighlightColor = settings.ValidHighlightColor;
            snapDropZone.highlightAlwaysActive = settings.HighlightAlwaysActive;
            snapDropZone.snapDuration = settings.SnapDuration;
            snapDropZone.highlightObjectPrefab = snapZonePrefab;

            foreach (Renderer renderer in snapZoneBlueprint.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }

            foreach (Collider collider in snapZoneBlueprint.GetComponentsInChildren<Collider>())
            {
                collider.isTrigger = true;
            }
        }

        private GameObject DuplicateObject(GameObject originalObject)
        {
            GameObject cloneObject = new GameObject(originalObject.name + "_Clone");
            CopyComponentsNeededForSnapZone(originalObject, cloneObject);
            CreateChildObjects(originalObject, cloneObject);

            return cloneObject;
        }

        private void CreateChildObjects(GameObject sourceParent, GameObject destinationParent)
        {
            foreach (Transform child in sourceParent.transform)
            {
                GameObject newChild = new GameObject(child.name);
                newChild.transform.SetParent(destinationParent.transform);
                CopyComponentsNeededForSnapZone(child.gameObject, newChild);
                CreateChildObjects(child.gameObject, newChild);
            }
        }

        private void CopyComponentsNeededForSnapZone(GameObject sourceObject, GameObject destinationObject)
        {
            List<Component> components = sourceObject.gameObject.GetComponents<Component>().ToList();

            foreach (Component component in components)
            {
                if (IsComponentNeeded(component))
                {
                    Component targetComponent = destinationObject.GetComponent(component.GetType());

                    if (targetComponent == null)
                    {
                        targetComponent = destinationObject.AddComponent(component.GetType());
                    }

                    EditorUtility.CopySerialized(component, targetComponent);
                }
            }
        }

        private bool IsComponentNeeded(Component component)
        {
            return component.GetType() == typeof(MeshRenderer) ||
                   component.GetType() == typeof(Transform) ||
                   component.GetType() == typeof(MeshFilter) ||
                   component is Collider;
        }

        private void SetHighlightMaterial(GameObject snapZonePrefab, Material highlightMaterial)
        {
            foreach (MeshRenderer renderer in snapZonePrefab.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.sharedMaterials = new[] { highlightMaterial };
            }
        }
    }
}
