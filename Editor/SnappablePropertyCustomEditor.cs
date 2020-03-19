using System.Collections.Generic;
using System.IO;
using System.Linq;
using Innoactive.Creator.VRTKInteraction;
using Innoactive.Creator.VRTKInteraction.Properties;
using UnityEditor;
using UnityEngine;

namespace Innoactive.CreatorEditor.VRTKInteraction
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
                if (Directory.Exists("Assets/Resources/") == false)
                {
                    Directory.CreateDirectory("Assets/Resources/");
                }
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
                    }
                    subPath = newPath;
                }
            }

#if UNITY_2018_3_OR_NEWER
            GameObject snapZonePrefab = PrefabUtility.SaveAsPrefabAsset(snapZoneBlueprint, prefabPath + "/" + snappable.name + "Highlight.prefab");
#elif UNITY_2017_4_OR_NEWER
            GameObject snapZonePrefab = PrefabUtility.CreatePrefab(prefabPath + "/" + snappable.name + "Highlight.prefab", snapZoneBlueprint);
#endif
            snapZonePrefab.transform.localScale = Vector3.one;

            snapZoneBlueprint.transform.localScale = Vector3.one;
            snapZoneBlueprint.transform.position = Vector3.zero;
            snapZoneBlueprint.transform.rotation = Quaternion.identity;
            
            GameObject snapZone = new GameObject(snappable.name + "SnapZone");
            EditorUtility.CopySerialized(snappable.transform, snapZone.transform);

            snapZone.AddComponent<SnapZoneProperty>();
            SnapDropZone snapDropZone = snapZone.GetComponent<SnapDropZone>();
            snapDropZone.snapType = settings.SnapType;
            snapDropZone.highlightColor = settings.HighlightColor;
            snapDropZone.validHighlightColor = settings.ValidHighlightColor;
            snapDropZone.highlightAlwaysActive = settings.HighlightAlwaysActive;
            snapDropZone.snapDuration = settings.SnapDuration;
            snapDropZone.highlightObjectPrefab = snapZonePrefab;

            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            if (snapZoneBlueprint.transform.GetComponent<Renderer>() != null)
            {
                bounds.Encapsulate(snapZoneBlueprint.transform.GetComponent<Renderer>().bounds);
            }
            
            foreach (Renderer renderer in snapZoneBlueprint.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            BoxCollider boxCollider = snapZone.AddComponent<BoxCollider>();
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size;
            boxCollider.isTrigger = true;
            
            DestroyImmediate(snapZoneBlueprint);
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
                CopyComponentsNeededForSnapZone(child.gameObject, newChild);
                // Kill child if empty
                if (child.transform.childCount == 0 && newChild.GetComponents<Component>().Length <= 1)
                {
                    DestroyImmediate(newChild);
                    continue;
                }
                
                newChild.transform.SetParent(destinationParent.transform, false);
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
                    Component targetComponent = null;

                    // Every gameObject has a Transform, so we need to check for this seperately
                    if (component.GetType() == typeof(Transform))
                    {
                        targetComponent = destinationObject.GetComponent<Transform>();
                    }

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
                   component.GetType() == typeof(MeshFilter);
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
