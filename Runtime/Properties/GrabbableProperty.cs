using System;
using Innoactive.Creator.BasicInteraction.Properties;
using Innoactive.Creator.Core.Properties;
using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;
using VRTK.Highlighters;

namespace Innoactive.Creator.VRTKInteraction.Properties
{
    /// <summary>
    /// VRTK implementation of the IGrabbableProperty.
    /// </summary>
    [RequireComponent(typeof(TouchableProperty))]
    public class GrabbableProperty : LockableProperty, IGrabbableProperty
    {
        public event EventHandler<EventArgs> Grabbed;
        public event EventHandler<EventArgs> Ungrabbed;

        /// <summary>
        /// Returns true if the Interactable of this property is grabbed.
        /// </summary>
        public virtual bool IsGrabbed
        {
            get
            {
                if (Interactable != null)
                {
                    return Interactable.IsGrabbed();
                }
                return false;
            }
        }

        private VRTK_InteractableObject interactable = null;
        private VRTK_InteractableObject Interactable
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
        private VRTK_InteractObjectHighlighter highlighter;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (gameObject.GetComponent<VRTK_BaseHighlighter>() == null)
            {
                // TODO: configurable defaults
                gameObject.AddComponent<VRTK_MaterialColorSwapHighlighter>();
            }

            if (gameObject.GetComponent<VRTK_BaseGrabAttach>() == null)
            {
                // TODO: configurable defaults
                VRTK_FixedJointGrabAttach grab = gameObject.AddComponent<VRTK_FixedJointGrabAttach>();
                grab.precisionGrab = true;
            }

            Interactable.isGrabbable = true;

            Interactable.InteractableObjectGrabbed += HandleVRTKGrabbed;
            Interactable.InteractableObjectUngrabbed += HandleVRTKUngrabbed;

            InternalSetLocked(IsLocked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (Interactable == null)
            {
                return;
            }

            Interactable.InteractableObjectGrabbed -= HandleVRTKGrabbed;
            Interactable.InteractableObjectUngrabbed -= HandleVRTKUngrabbed;
        }

        private void HandleVRTKGrabbed(object sender, InteractableObjectEventArgs args)
        {
            EmitGrabbed();
        }

        private void HandleVRTKUngrabbed(object sender, InteractableObjectEventArgs args)
        {
            EmitUngrabbed();
        }

        protected void EmitGrabbed()
        {
            Grabbed?.Invoke(this, EventArgs.Empty);
        }

        protected void EmitUngrabbed()
        {
            Ungrabbed?.Invoke(this, EventArgs.Empty);
        }

        protected override void InternalSetLocked(bool lockState)
        {
            if (Interactable.IsGrabbed() && lockState)
            {
                Interactable.ForceStopInteracting();
                
                if (highlighter == null)
                {
                    highlighter = gameObject.GetComponent<VRTK_InteractObjectHighlighter>();
                }

                Interactable.isGrabbable = false;
                if (highlighter != null && highlighter.grabHighlight != Color.clear)
                {
                    highlighter.Unhighlight();
                }
            }
            else
            {
                Interactable.isGrabbable = lockState == false;
            }
        }

        /// <summary>
        /// Instantaneously simulate that the object was grabbed.
        /// </summary>
        public void FastForwardGrab()
        {
            if (Interactable.IsGrabbed())
            {
                VRTK_InteractGrab grab = Interactable.GetGrabbingObject().GetComponent<VRTK_InteractGrab>();
                Interactable.Ungrabbed();
                grab.AttemptGrab();
                Interactable.ForceStopInteracting();
            }
            else
            {
                EmitGrabbed();
                EmitUngrabbed();
            }
        }

        /// <summary>
        /// Instantaneously simulate that the object was ungrabbed.
        /// </summary>
        public void FastForwardUngrab()
        {
            if (Interactable.IsGrabbed())
            {
                Interactable.ForceStopInteracting();
            }
            else
            {
                EmitGrabbed();
                EmitUngrabbed();
            }
        }
    }
}
