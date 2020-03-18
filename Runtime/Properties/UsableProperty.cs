﻿using Innoactive.Creator.VRTKInteraction;
using System;
using Innoactive.Creator.BasicInteraction.Properties;
using Innoactive.Creator.Core.SceneObjects.Properties;
using UnityEngine;
using VRTK;

namespace Innoactive.Creator.VRTKInteraction.Properties
{
    /// <summary>
    /// VRTK implementation of the IUsableProperty.
    /// </summary>
    [RequireComponent(typeof(TouchableProperty))]
    public class UsableProperty : LockableProperty, IUsableProperty
    {
        public event EventHandler<EventArgs> UsageStarted;
        public event EventHandler<EventArgs> UsageStopped;

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
            UsageStarted?.Invoke(this, EventArgs.Empty);
        }

        protected void EmitUsageStopped()
        {
            UsageStopped?.Invoke(this, EventArgs.Empty);
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
