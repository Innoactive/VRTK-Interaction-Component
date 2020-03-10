﻿﻿using Innoactive.Hub.Interaction;
using System;
 using Innoactive.Hub.Training.SceneObjects.Interaction.Properties;
 using UnityEngine;
using VRTK;

 namespace Innoactive.Hub.Training.SceneObjects.Properties
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

        protected VRTK_InteractObjectHighlighter highlighter;

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

            interactable = gameObject.GetComponent<VRTK_InteractableObject>();
            if (interactable == null)
            {
                interactable = gameObject.AddComponent<InteractableObject>();
            }

            interactable.disableWhenIdle = false; // required to allow deactivating interactable object touching
            interactable.enabled = true;

            interactable.InteractableObjectTouched += HandleVRTKTouched;
            interactable.InteractableObjectUntouched += HandleVRTKUntouched;

            highlighter = gameObject.GetComponent<VRTK_InteractObjectHighlighter>();
            if (highlighter == null)
            {
                highlighter = gameObject.AddComponent<VRTK_InteractObjectHighlighter>();
                highlighter.touchHighlight = new Color(0.259f, 0.7843f, 1.0f, 0.5f);
            }
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
            Touched?.Invoke(this, EventArgs.Empty);
        }

        protected void EmitUntouched()
        {
            Untouched?.Invoke(this, EventArgs.Empty);
        }

        protected override void InternalSetLocked(bool lockState)
        {
            if (interactable.IsTouched())
            {
                if (lockState)
                {
                    interactable.ForceStopInteracting();
                }

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