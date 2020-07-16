using System;
using Innoactive.Creator.BasicInteraction.Properties;
using Innoactive.Creator.Core.Properties;
using UnityEngine;
using VRTK;

namespace Innoactive.Creator.VRTKInteraction.Properties
{
    /// <summary>
    /// VRTK implementation of the ITouchableProperty.
    /// </summary>
    public class TouchableProperty : LockableProperty, ITouchableProperty
    {
        public event EventHandler<EventArgs> Touched;
        public event EventHandler<EventArgs> Untouched;

        /// <summary>
        /// Returns true if the GameObject is touched.
        /// </summary>
        public virtual bool IsBeingTouched
        {
            get
            {
                if (Interactable != null)
                {
                    return Interactable.IsTouched();
                }

                return false;
            }
        }

        public bool UsableOnlyIfGrabbed { get; protected set; }
        public bool HoldButtonToUse { get; protected set; }

        private VRTK_InteractableObject interactable;

        protected VRTK_InteractableObject Interactable
        {
            get
            {
                if (interactable == null)
                {
                    interactable = gameObject.GetComponent<VRTK_InteractableObject>();
                    if (interactable == null)
                    {
                        interactable = gameObject.AddComponent<InteractableObject>();
                    }
                }

                return interactable;
            }
        }

        protected VRTK_InteractObjectHighlighter highlighter;

        public void SetUsableOnlyIfGrabbed(bool value)
        {
            UsableOnlyIfGrabbed = value;
            if (Interactable != null)
            {
                Interactable.useOnlyIfGrabbed = value;
            }
        }

        public void SetHoldButtonToUse(bool value)
        {
            HoldButtonToUse = value;
            if (Interactable != null)
            {
                Interactable.holdButtonToUse = value;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Interactable.disableWhenIdle = false; // required to allow deactivating interactable object touching
            Interactable.enabled = true;

            Interactable.InteractableObjectTouched += HandleVRTKTouched;
            Interactable.InteractableObjectUntouched += HandleVRTKUntouched;

            highlighter = gameObject.GetComponent<VRTK_InteractObjectHighlighter>();
            if (highlighter == null)
            {
                highlighter = gameObject.AddComponent<VRTK_InteractObjectHighlighter>();
                highlighter.touchHighlight = new Color(0.259f, 0.7843f, 1.0f, 0.5f);
            }
            
            InternalSetLocked(IsLocked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Interactable.InteractableObjectUsed -= HandleVRTKTouched;
            Interactable.InteractableObjectUnused -= HandleVRTKUntouched;
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
            Touched?.Invoke(this, EventArgs.Empty);
        }

        protected void EmitUntouched()
        {
            Untouched?.Invoke(this, EventArgs.Empty);
        }

        protected override void InternalSetLocked(bool lockState)
        {
            Interactable.enabled = lockState == false;
            if (Interactable.IsTouched())
            {
                if (lockState)
                {
                    Interactable.ForceStopInteracting();
                }
                
                Interactable.enabled = lockState == false;

                if (highlighter != null && highlighter.touchHighlight != Color.clear)
                {
                    if (lockState)
                    {
                        highlighter.Unhighlight();
                    }
                    else
                    {
                        highlighter.Highlight(highlighter.touchHighlight);
                    }
                }
            }
        }

        /// <summary>
        /// Instantaneously simulate a touch.
        /// </summary>
        public void FastForwardTouch()
        {
            if (Interactable.IsTouched())
            {
                Interactable.StopTouching();
                Interactable.StartTouching();
                Interactable.ForceStopInteracting();
            }
            else
            {
                EmitTouched();
                EmitUntouched();
            }
        }
    }
}