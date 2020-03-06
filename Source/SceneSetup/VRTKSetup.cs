using Innoactive.Creator.Interaction.Utils;
using Innoactive.Hub.Training.Unity.Utils;
using UnityEditor;
using UnityEngine;

namespace Innoactive.Creator.VRTK.Utils
{
    /// <summary>
    /// Scene setup for VRTK.
    /// </summary>
    public class VRTKSetup : OnInteractionFrameworkSetup
    {
        /// <inheritdoc />
        public override void Setup()
        {
            SetupVRTK();
        }
        
        private void SetupVRTK()
        {
            GameObject camera = GameObject.Find("Main Camera");
            if (camera != null)
            {
                GameObject.DestroyImmediate(camera);
            }

            GameObject vrtkSetup = GameObject.Find("[VRTK_Setup]");
            if (vrtkSetup == null)
            {
                GameObject prefab = (GameObject)GameObject.Instantiate(Resources.Load("[VRTK_Setup]", typeof(GameObject)));
                prefab.name = prefab.name.Replace("(Clone)", "");
            }
            else if (SceneUtils.ContainsMissingScripts(vrtkSetup))
            {
                if (EditorUtility.DisplayDialog("Broken VRTK Setup found", "Your VRTK setup requires the Hub-SDK, do you want to replace it?", "Replace", "Cancel"))
                {
                    GameObject.DestroyImmediate(vrtkSetup);
                    GameObject prefab = (GameObject)GameObject.Instantiate(Resources.Load("[VRTK_Setup]", typeof(GameObject)));
                    prefab.name = prefab.name.Replace("(Clone)", "");
                }
            }
        }
    }
}