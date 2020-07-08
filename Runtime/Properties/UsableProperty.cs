using System;
using Innoactive.Creator.BasicInteraction.Properties;
using Innoactive.Creator.Core.Properties;
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
                if (Interactable != null)
                {
                    return Interactable.IsUsing();
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

        public UsableProperty()
        {
            UsableOnlyIfGrabbed = false; // TODO: default config
            HoldButtonToUse = true; // TODO: default config
        }

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

            Interactable.holdButtonToUse = HoldButtonToUse;
            Interactable.useOnlyIfGrabbed = UsableOnlyIfGrabbed;

            Interactable.isUsable = true;

            Interactable.InteractableObjectUsed += HandleVRTKUsageStarted;
            Interactable.InteractableObjectUnused += HandleVRTKUsageStopped;
            
            InternalSetLocked(IsLocked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (Interactable != null)
            {
                Interactable.InteractableObjectUsed -= HandleVRTKUsageStarted;
                Interactable.InteractableObjectUnused -= HandleVRTKUsageStopped;
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
            if (Interactable.IsUsing())
            {
                if (lockState)
                {
                    Interactable.ForceStopInteracting();
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
            Interactable.isUsable = lockState == false;
        }

        /// <summary>
        /// Instantaneously simulate that the object was used.
        /// </summary>
        public void FastForwardUse()
        {
            if (Interactable.IsUsing())
            {
                VRTK_InteractUse user = Interactable.GetUsingScript();
                Interactable.StopUsing();
                user.AttemptUse();
                Interactable.StopUsing();
            }
            else
            {
                EmitUsageStarted();
                EmitUsageStopped();
            }
        }
    }
}
