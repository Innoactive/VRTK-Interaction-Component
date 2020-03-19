﻿﻿using Innoactive.Creator.VRTKInteraction;
using System;
using Innoactive.Creator.BasicInteraction.Properties;
using Innoactive.Creator.Core.Properties;
using Innoactive.Creator.Unity;
using UnityEngine;
using VRTK;

namespace Innoactive.Creator.VRTKInteraction.Properties
{
    /// <summary>
    /// VRTK implementation of the ISnappableProperty.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(GrabbableProperty))]
    public class SnappableProperty : TrainingSceneObjectProperty, ISnappableProperty
    {
        public event EventHandler<EventArgs> Snapped;
        public event EventHandler<EventArgs> Unsnapped;

        /// <summary>
        /// Returns true if the Snappable object is snapped.
        /// </summary>
        public bool IsSnapped
        {
            get
            {
                return SnappedZone != null;
            }
        }

        /// <summary>
        /// Will return the ISnapZoneProperty of the SnapZone which snapped this object.
        /// </summary>
        public ISnapZoneProperty SnappedZone { get; set; }

        [SerializeField]
        private bool lockObjectOnSnap = false;
        public bool LockObjectOnSnap
        {
            get
            {
                return lockObjectOnSnap;
            }
            set
            {
                lockObjectOnSnap = value;
            }
        }

        private VRTK_InteractableObject interactable = null;

        protected void Awake()
        {
            // The Rigidbody is required for the snap type `Use Kinematic` of the snap drop zone script to work properly.
            gameObject.GetOrAddComponent<Rigidbody>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // TODO: think about init order of multiple properties

            interactable = gameObject.GetComponent<VRTK_InteractableObject>();
            if (interactable == null)
            {
                interactable = gameObject.AddComponent<InteractableObject>();
            }

            interactable.InteractableObjectSnappedToDropZone += HandleSnappedToDropZone;
            interactable.InteractableObjectUnsnappedFromDropZone += HandleUnsnappedFromDropZone;
        }

        protected virtual void OnDisable()
        {
            if (interactable != null)
            {
                interactable.InteractableObjectSnappedToDropZone -= HandleSnappedToDropZone;
                interactable.InteractableObjectUnsnappedFromDropZone -= HandleUnsnappedFromDropZone;
            }
        }

        protected void HandleSnappedToDropZone(object sender, InteractableObjectEventArgs args)
        {
            SnappedZone = args.interactingObject.GetComponent<SnapZoneProperty>();

            if (SnappedZone == null)
            {
                Debug.LogWarningFormat("Object '{0}' was snapped to SnapZone '{1}' without SnappableProperty", SceneObject.UniqueName, args.interactingObject.name);
                return;
            }

            if (LockObjectOnSnap)
            {
                SceneObject.SetLocked(true);
            }
            EmitSnapped(SnappedZone);
        }
        protected void HandleUnsnappedFromDropZone(object sender, InteractableObjectEventArgs args)
        {
            if (SnappedZone == null)
            {
                return;
            }

            ISnapZoneProperty previouslySnappedZone = SnappedZone;
            SnappedZone = null;
            EmitUnsnapped(previouslySnappedZone);
        }

        protected void EmitSnapped(ISnapZoneProperty snapZone)
        {
            Snapped?.Invoke(this, EventArgs.Empty);
        }

        protected void EmitUnsnapped(ISnapZoneProperty snapZone)
        {
            Unsnapped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Instantaneously snap the object into <paramref name="snapZone"/>
        /// </summary>
        public void FastForwardSnapInto(ISnapZoneProperty snapZone)
        {
            if (SnappedZone != null)
            {
                SnappedZone.SnapZoneObject.GetComponent<SnapDropZone>().ForceUnsnap();
            }

            SnapDropZone snapDropZone = snapZone.SnapZoneObject.GetComponent<SnapDropZone>();

            if (snapDropZone.GetCurrentSnappedInteractableObject() != null)
            {
                snapDropZone.ForceUnsnap();
            }

            snapDropZone.ForceSnapObjectWithoutTransition(gameObject);
        }
    }
}
