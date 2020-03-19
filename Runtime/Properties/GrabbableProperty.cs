﻿using Innoactive.Creator.VRTKInteraction;
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
                if (interactable != null)
                {
                    return interactable.IsGrabbed();
                }
                return false;
            }
        }

        private VRTK_InteractableObject interactable;
        private VRTK_InteractObjectHighlighter highlighter;

        protected override void OnEnable()
        {
            base.OnEnable();

            interactable = gameObject.GetComponent<VRTK_InteractableObject>();
            if (interactable == null)
            {
                interactable = gameObject.AddComponent<InteractableObject>();
            }

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

            interactable.isGrabbable = true;

            interactable.InteractableObjectGrabbed += HandleVRTKGrabbed;
            interactable.InteractableObjectUngrabbed += HandleVRTKUngrabbed;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (interactable == null)
            {
                return;
            }

            interactable.InteractableObjectGrabbed -= HandleVRTKGrabbed;
            interactable.InteractableObjectUngrabbed -= HandleVRTKUngrabbed;
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
            if (interactable.IsGrabbed())
            {
                if (lockState)
                {
                    interactable.ForceStopInteracting();
                }

                if (highlighter == null)
                {
                    highlighter = gameObject.GetComponent<VRTK_InteractObjectHighlighter>();
                }

                if (highlighter != null && highlighter.grabHighlight != Color.clear)
                {
                    if (lockState)
                    {
                        highlighter.Unhighlight();
                    }
                    else
                    {
                        highlighter.Highlight(highlighter.grabHighlight);
                    }
                }
            }

            interactable.isGrabbable = lockState == false;
        }

        /// <summary>
        /// Instantaneously simulate that the object was grabbed.
        /// </summary>
        public void FastForwardGrab()
        {
            if (interactable.IsGrabbed())
            {
                VRTK_InteractGrab grab = interactable.GetGrabbingObject().GetComponent<VRTK_InteractGrab>();
                interactable.Ungrabbed();
                grab.AttemptGrab();
                interactable.ForceStopInteracting();
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
            if (interactable.IsGrabbed())
            {
                interactable.ForceStopInteracting();
            }
            else
            {
                EmitGrabbed();
                EmitUngrabbed();
            }
        }
    }
}
