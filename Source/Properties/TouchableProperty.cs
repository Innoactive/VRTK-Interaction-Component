using System;
using Innoactive.Hub.Unity;
using VRTK;

namespace Innoactive.Hub.Training.SceneObjects.Properties
{
    public class TouchableProperty : LockableProperty
    {
        public event EventHandler<EventArgs> Touched;
        public event EventHandler<EventArgs> Untouched;

        public virtual bool IsBeingTouched
        {
            get
            {
                if (interactable != null)
                {
                    return interactable.IsTouched();
                }
                return false;
            }
        }

        public bool UsableOnlyIfGrabbed { get; protected set; }
        public bool HoldButtonToUse { get; protected set; }

        protected VRTK_InteractableObject interactable;

        public void SetUsableOnlyIfGrabbed(bool value)
        {
            UsableOnlyIfGrabbed = value;
            if (interactable != null)
            {
                interactable.useOnlyIfGrabbed = value;
            }
        }

        public void SetHoldButtonToUse(bool value)
        {
            HoldButtonToUse = value;
            if (interactable != null)
            {
                interactable.holdButtonToUse = value;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            interactable = gameObject.GetComponent<VRTK_InteractableObject>(true);
            interactable.disableWhenIdle = false; // required to allow deactivating interactable object touching
            interactable.enabled = true;

            interactable.InteractableObjectTouched += HandleVRTKTouched;
            interactable.InteractableObjectUntouched += HandleVRTKUntouched;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            interactable.InteractableObjectUsed -= HandleVRTKTouched;
            interactable.InteractableObjectUnused -= HandleVRTKUntouched;
        }

        private void HandleVRTKTouched(object sender, InteractableObjectEventArgs args)
        {
            EmitTouched();
        }

        private void HandleVRTKUntouched(object sender, InteractableObjectEventArgs args)
        {
            EmitUntouched();
        }

        protected void EmitTouched()
        {
            if (Touched != null)
            {
                Touched.Invoke(this, EventArgs.Empty);
            }
        }

        protected void EmitUntouched()
        {
            if (Untouched != null)
            {
                Untouched.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void InternalSetLocked(bool lockState)
        {
            if (interactable.IsTouched())
            {
                interactable.ForceStopInteracting();
            }

            interactable.enabled = lockState == false;
        }

        /// <summary>
        /// Instantaneously simulate a touch.
        /// </summary>
        public void FastForwardTouch()
        {
            if (interactable.IsTouched())
            {
                interactable.StopTouching();
                interactable.StartTouching();
                interactable.ForceStopInteracting();
            }
            else
            {
                EmitTouched();
                EmitUntouched();
            }
        }
    }
}
