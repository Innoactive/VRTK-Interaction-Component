using Innoactive.Hub.Training.SceneObjects.Properties;
using Innoactive.Hub.Unity;
using VRTK;

namespace Innoactive.Hub.Training.Utils.VRTK
{
    /// <summary>
    /// Makes sure that a <see cref="TraineeSceneObject"/> is applied to the current VRTK headset.
    /// </summary>
    public class TraineeManager : UnitySingleton<TraineeManager>
    {
        /// <summary>
        /// Current trainee object.
        /// </summary>
        public TraineeSceneObject Trainee { get; protected set; }
        
        protected void OnEnable()
        {
            VRTK_SDKManager.instance.LoadedSetupChanged += OnVRTKLoaded;
        }

        private void OnDisable()
        {
            VRTK_SDKManager.instance.LoadedSetupChanged -= OnVRTKLoaded;
        }

        private void OnVRTKLoaded(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs args)
        {
            if (Trainee != null)
            {
                Destroy(Trainee);
            }
            
            Trainee = args.currentSetup.actualHeadset.AddComponent<TraineeSceneObject>();
        }
    }
}