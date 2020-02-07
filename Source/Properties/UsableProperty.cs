﻿using Innoactive.Hub.Interaction;
using System;
using UnityEngine;
using VRTK;

namespace Innoactive.Hub.Training.SceneObjects.Properties
{
    [RequireComponent(typeof(TouchableProperty))]
    public class UsableProperty : LockableProperty
    {
        public class UsedEventArgs : EventArgs { }

        public event EventHandler<UsedEventArgs> UsageStarted;
        public event EventHandler<UsedEventArgs> UsageStopped;

        public virtual bool IsBeingUsed
        {
            get
            {
                if (interactable != null)
                {
                    return interactable.IsUsing();
                }
                return false;
            }
        }

        [SerializeField]
        private bool usableOnlyIfGrabbed = false;
        public bool UsableOnlyIfGrabbed { get { return usableOnlyIfGrabbed; } protected set { usableOnlyIfGrabbed = value; } }

        [SerializeField]
        private bool holdButtonToUse = false;
        public bool HoldButtonToUse { get { return holdButtonToUse; } protected set { holdButtonToUse = value; } }

        protected VRTK_InteractableObject interactable;
        protected VRTK_InteractObjectHighlighter highlighter;

        public UsableProperty()
        {
            UsableOnlyIfGrabbed = false; // TODO: default config
            HoldButtonToUse = true; // TODO: default config
        }

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

            interactable.holdButtonToUse = HoldButtonToUse;
            interactable.useOnlyIfGrabbed = UsableOnlyIfGrabbed;

            interactable.isUsable = true;

            interactable.InteractableObjectUsed += HandleVRTKUsageStarted;
            interactable.InteractableObjectUnused += HandleVRTKUsageStopped;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (interactable != null)
            {
                interactable.InteractableObjectUsed -= HandleVRTKUsageStarted;
                interactable.InteractableObjectUnused -= HandleVRTKUsageStopped;
            }
        }

        private void HandleVRTKUsageStarted(object sender, InteractableObjectEventArgs args)
        {
            EmitUsageStarted();
        }

        private void HandleVRTKUsageStopped(object sender, InteractableObjectEventArgs args)
        {
            EmitUsageStopped();
        }

        protected void EmitUsageStarted()
        {
            if (UsageStarted != null)
            {
                UsageStarted.Invoke(this, new UsedEventArgs());
            }
        }

        protected void EmitUsageStopped()
        {
            if (UsageStopped != null)
            {
                UsageStopped.Invoke(this, new UsedEventArgs());
            }
        }

        protected override void InternalSetLocked(bool lockState)
        {
            if (interactable.IsUsing())
            {
                if (lockState)
                {
                    interactable.ForceStopInteracting();
                }

                if (highlighter == null)
                {
                    highlighter = gameObject.GetComponent<VRTK_InteractObjectHighlighter>();
                }

                if (highlighter != null && highlighter.useHighlight != Color.clear)
                {
                    if (lockState)
                    {
                        highlighter.Unhighlight();
                    }
                    else
                    {
                        highlighter.Highlight(highlighter.useHighlight);
                    }
                }
            }

            interactable.isUsable = lockState == false;
        }

        /// <summary>
        /// Instantaneously simulate that the object was used.
        /// </summary>
        public void FastForwardUse()
        {
            if (interactable.IsUsing())
            {
                VRTK_InteractUse user = interactable.GetUsingScript();
                interactable.StopUsing();
                user.AttemptUse();
                interactable.StopUsing();
            }
            else
            {
                EmitUsageStarted();
                EmitUsageStopped();
            }
        }
    }
}
