using UnityEngine;
using VRTK;

namespace Innoactive.Creator.VRTKInteraction
{
    /// <summary>
    /// Allows to choose between different controller configs. These Configs must be instances in the scene,
    /// and registered at the ControllerConfigManager's configurations field. Each config must be a GameObject
    /// with child objects 'LeftController' and 'RightController'. Based on the value in the PlayerConfig's
    /// controllerLayout entry, the config with the respective index is activated and its controllers
    /// are assigned to the VRTK setup. Configs can also be switched at run-time using SetupConfig.
    /// Additionally, setups can have additional child components ControllerUsageDisplay and ControllerTooltips,
    /// and the ControllerConfigManager handles showing them for the specified duration after a config was enbaled.
    /// </summary>
    public class ControllerConfigSwitcher : MonoBehaviour
    {
        private ControllerConfig currentConfig = null;
        private GameObject currentLeftController = null;
        private GameObject currentRightController = null;

        public ControllerConfig CurrentControllerConfig
        {
            get
            {
                return currentConfig;
            }
        }

        public void SwitchConfig(ControllerConfig config)
        {
            if (config == currentConfig)
            {
                return;
            }

            ControllerConfig previousConfig = currentConfig;
            currentConfig = config;

            if (previousConfig != null)
            {
                if (currentLeftController != null)
                {
                    currentLeftController.SetActive(false);
                    currentLeftController.transform.SetParent(null);
                }
                if (currentRightController != null)
                {
                    currentRightController.SetActive(false);
                    currentRightController.transform.SetParent(null);
                }
                previousConfig.OnDeactivate(currentLeftController, currentRightController);

                currentRightController = null;
                currentLeftController = null;
            }

            currentConfig = config;

            GameObject leftController = currentConfig.GetLeftController();
            if (leftController)
            {
                leftController.transform.SetParent(transform);
                VRTK_SDKManager.instance.scriptAliasLeftController = leftController;
                currentLeftController = leftController;
                currentLeftController.name = "LeftController";
            } else
            {
                VRTK_SDKManager.instance.scriptAliasLeftController = null;
            }

            GameObject rightController = currentConfig.GetRightController();
            if (rightController)
            {
                rightController.transform.SetParent(transform);
                VRTK_SDKManager.instance.scriptAliasRightController = rightController;
                currentRightController = rightController;
                currentRightController.name = "RightController";
            } else
            {
                VRTK_SDKManager.instance.scriptAliasRightController = null;
            }

            currentConfig.OnActivate(leftController, rightController);

            VRTK_SDKManager.instance.TryLoadSDKSetupFromList();
        }
    }
}