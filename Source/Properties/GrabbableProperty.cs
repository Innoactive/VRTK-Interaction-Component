﻿using Innoactive.Hub.Interaction;
using System;
using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;
using VRTK.Highlighters;

namespace Innoactive.Hub.Training.SceneObjects.Properties
{
    [RequireComponent(typeof(TouchableProperty))]
    public class GrabbableProperty : LockableProperty
    {
        public class GrabbedEventArgs : EventArgs { }

        public event EventHandler<GrabbedEventArgs> Grabbed;
        public event EventHandler<GrabbedEventArgs> Ungrabbed;

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

            interactable.InteractableObjectGrabbed += HandleVrtkGrabbed;
            interactable.InteractableObjectUngrabbed += HandleVrtkUngrabbed;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (interactable == null)
            {
                return;
            }

            interactable.InteractableObjectGrabbed -= HandleVrtkGrabbed;
            interactable.InteractableObjectUngrabbed -= HandleVrtkUngrabbed;
        }

        private void HandleVrtkGrabbed(object sender, InteractableObjectEventArgs args)
        {
            EmitGrabbed();
        }

        private void HandleVrtkUngrabbed(object sender, InteractableObjectEventArgs args)
        {
            EmitUngrabbed();
        }

        protected void EmitGrabbed()
        {
            if (Grabbed != null)
            {
                Grabbed.Invoke(this, new GrabbedEventArgs());
            }
        }

        protected void EmitUngrabbed()
        {
            if (Ungrabbed != null)
            {
                Ungrabbed.Invoke(this, new GrabbedEventArgs());
            }
        }

        protected override void InternalSetLocked(bool lockState)
        {
            if (interactable.IsGrabbed())
            {
                interactable.ForceStopInteracting();
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
