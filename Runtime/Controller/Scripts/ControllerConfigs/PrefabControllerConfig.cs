using UnityEngine;

namespace Innoactive.Creator.VRTKInteraction
{
    /// <summary>
    /// Controller config that provides left and right controllers by instantiating the provided prefab objects.
    /// </summary>
    [CreateAssetMenu(fileName = "ControllerConfig", menuName = "InnoactiveHub/Controller/PrefabControllerConfig")]
    public class PrefabControllerConfig : ControllerConfig
    {
        [SerializeField]
        public GameObject leftController = null;
        [SerializeField]
        public GameObject rightController = null;

        public override GameObject GetLeftController()
        {
            if (leftController != null)
            {
                bool wasEnabled = leftController.activeSelf;
                leftController.SetActive(false);
                GameObject instance = GameObject.Instantiate(leftController);
                leftController.SetActive(wasEnabled);
                return instance;
            }
            return null;
        }

        public override GameObject GetRightController()
        {
            if (rightController != null)
            {
                bool wasEnabled = rightController.activeSelf;
                rightController.SetActive(false);
                GameObject instance = GameObject.Instantiate(rightController);
                rightController.SetActive(wasEnabled);
                return instance;
            }
            return null;
        }

        public override void OnActivate(GameObject leftController, GameObject rightController)
        {
            if (leftController != null)
            {
                leftController.SetActive(true);
            }
            if (rightController != null)
            {
                rightController.SetActive(true);
            }
        }

        public override void OnDeactivate(GameObject leftController, GameObject rightController)
        {
            if (leftController != null)
            {
                GameObject.DestroyImmediate(leftController);
            }
            if (rightController != null)
            {
                GameObject.DestroyImmediate(rightController);
            }
        }
    }
}