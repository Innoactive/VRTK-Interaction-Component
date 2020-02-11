﻿﻿using Innoactive.Hub.Interaction;
using System;
  using Innoactive.Hub.Training.SceneObjects.Interaction.Properties;
  using Innoactive.Hub.Training.Unity.Utils;
using UnityEngine;
using VRTK;

namespace Innoactive.Hub.Training.SceneObjects.Properties
{
    [RequireComponent(typeof(Rigidbody), typeof(GrabbableProperty))]
    public class SnappableProperty : TrainingSceneObjectProperty, ISnappableProperty
    {
        public class SnappedEventArgs : EventArgs
        {
            public readonly ISnapZoneProperty SnapZone;
            
            public SnappedEventArgs(ISnapZoneProperty snapZone)
            {
                SnapZone = snapZone;
            }
        }

        public event EventHandler<EventArgs> Snapped;
        public event EventHandler<EventArgs> Unsnapped;

        public bool IsSnapped
        {
            get
            {
                return SnappedZone != null;
            }
        }

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

        protected virtual void HandleSnappedToDropZone(object sender, InteractableObjectEventArgs args)
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

        protected virtual void HandleUnsnappedFromDropZone(object sender, InteractableObjectEventArgs args)
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
            if (Snapped != null)
            {
                Snapped.Invoke(this, new SnappedEventArgs(snapZone));
            }
        }

        protected void EmitUnsnapped(ISnapZoneProperty snapZone)
        {
            if (Unsnapped != null)
            {
                Unsnapped.Invoke(this, new SnappedEventArgs(snapZone));
            }
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
