using System;
using UnityEngine;
using VRTK;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Innoactive.Creator.VRTKInteraction
{
    /// <summary>
    /// Defines what configs should be loaded on scene start based on the SDK and HMD types.
    /// </summary>
    public class ChooseController : MonoBehaviour
    {
        public enum SDKType
        {
            None,
            SteamVR,
            Oculus,
            WindowsMixedReality
        }

        [Serializable]
        public class Config
        {
            public SDKType sdkType;
            public SDK_BaseHeadset.HeadsetType headsetType;
            public ControllerConfig config;
        }

        [SerializeField]
        private ControllerConfigSwitcher configSwitcher = null;

        [SerializeField]
        protected ControllerConfig defaultConfig = null;
        public ControllerConfig DefaultConfig
        {
            get
            {
                return defaultConfig;
            }
            set
            {
                defaultConfig = value;
            }
        }

        [SerializeField]
        protected Config[] configs = null;
        public Config[] Configs
        {
            get
            {
                return configs;
            }
            set
            {
                configs = value;
            }
        }

        [SerializeField]
        private bool applyLastUsedConfigOnAwake = true;

        private static SDKType lastUsedSDKType = SDKType.None;

        private static SDK_BaseHeadset.HeadsetType lastUsedHMDType = SDK_BaseHeadset.HeadsetType.Undefined;

        private void Awake()
        {
            if (configSwitcher == null)
            {
                configSwitcher = GetComponent<ControllerConfigSwitcher>();
            }
            if (configSwitcher == null)
            {
                Debug.LogError("ChooseController could not find a ControllerConfigSwitcher");
            }

            if (VRTK_SDKManager.instance.autoLoadSetup == false)
            {
                Debug.LogWarning("ChooseController used while VRTK_SDKManager.autoLoadSetup is set to false - this may cause problems!");
            }

            if (applyLastUsedConfigOnAwake && lastUsedSDKType != SDKType.None && lastUsedHMDType != SDK_BaseHeadset.HeadsetType.Undefined)
            {
                TryConfigureForSetup(lastUsedSDKType, lastUsedHMDType);
            }
        }

        private void OnEnable()
        {
            VRTK_SDKManager.instance.LoadedSetupChanged += VRTKSetup_LoadedSetupChanged;
        }

        private void OnDisable()
        {
            VRTK_SDKManager.instance.LoadedSetupChanged += VRTKSetup_LoadedSetupChanged;
        }

        private void VRTKSetup_LoadedSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e)
        {
            if (e.currentSetup != null)
            {
                SDKType currentSDKType = GetCurrentSDKType(e.currentSetup);
                SDK_BaseHeadset.HeadsetType currentHeadsetType = GetCurrentHMDType();
                // store last used name for applyLastUsedconfigOnAwake
                lastUsedSDKType = currentSDKType;
                lastUsedHMDType = currentHeadsetType;

                TryConfigureForSetup(currentSDKType, currentHeadsetType);
            }
        }

        private void TryConfigureForSetup(SDKType sdkType, SDK_BaseHeadset.HeadsetType headsetType)
        {
            Config config = GetConfigForType(sdkType, headsetType);

            if (config == null)
            {
                if (defaultConfig == null)
                {
                    Debug.LogWarningFormat("ChooseControllerByType could not find a config for setup \"{0}\" and \"{1}\"", sdkType, headsetType);
                }
                else
                {
                    Debug.LogFormat("ChooseControllerByType could not find a config for setup \"{0}\" and \"{1}\" - loading default config", sdkType, headsetType);
                    configSwitcher.SwitchConfig(defaultConfig);
                }
            }
            else if (config.config == null)
            {
                Debug.LogWarningFormat("ChooseControllerByType has no assigned controller config for setup \"{0}\" and \"{1}\"", sdkType, headsetType);
            }
            else if (config.config != configSwitcher.CurrentControllerConfig)
            {
                configSwitcher.SwitchConfig(config.config);
            }
        }

        protected virtual SDKType GetCurrentSDKType(VRTK_SDKSetup setup)
        {
            Type infoType = setup.systemSDKInfo.type;

            if (infoType == typeof(SDK_SteamVRSystem) || infoType == typeof(SDK_SimSystem))
            {
                return SDKType.SteamVR;
            }
            else if(infoType == typeof(SDK_OculusSystem))
            {
                return SDKType.Oculus;
            }
            // TODO: Windows MR
            //else if (infoType == typeof(SDK_WindowsMRSystem))
            //{
            //    return SDKType.MIXED_REALITY;
            //}
            else
            {
                return SDKType.None;
            }
        }

        protected virtual SDK_BaseHeadset.HeadsetType GetCurrentHMDType()
        {
            return VRTK_DeviceFinder.GetHeadsetType();
        }

        protected virtual Config GetConfigForType(SDKType sdkType, SDK_BaseHeadset.HeadsetType headsetType)
        {
            Config config = Array.Find(configs, test => (test.sdkType == sdkType && test.headsetType == headsetType));

            return config;
        }
    }
}
