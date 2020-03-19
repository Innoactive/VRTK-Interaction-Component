using UnityEngine;

namespace Innoactive.Creator.VRTKInteraction
{
    /// <summary>
    /// Abstract class that can used 
    /// </summary>
    public abstract class ControllerConfig : ScriptableObject
    {
        [SerializeField]
        private string configName;

        public string ConfigName
        {
            get
            {
                return configName;
            }
            set
            {
                configName = value;
            }
        }

        /// <summary>
        /// Provide an instance of the game object to be used as ScriptAlias for the left controller
        /// </summary>
        public abstract GameObject GetLeftController();

        /// <summary>
        /// Provide an instance of the game object to be used as ScriptAlias for the right controller
        /// </summary>
        public abstract GameObject GetRightController();

        /// <summary>
        /// Called before the config will be loaded, passing the instances retrieved from GetLeft/RightController
        /// </summary>
        public abstract void OnActivate(GameObject leftController, GameObject rightController);

        /// <summary>
        /// Called when the config will be unloaded, passing the instances retrieved from GetLeft/RightController
        /// </summary>
        public abstract void OnDeactivate(GameObject leftController, GameObject rightController);
    }
}