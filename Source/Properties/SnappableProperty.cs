﻿﻿using Innoactive.Hub.Interaction;
using System;
using Innoactive.Hub.Training.Unity.Utils;
using UnityEngine;
using VRTK;

namespace Innoactive.Hub.Training.SceneObjects.Properties
{
    [RequireComponent(typeof(Rigidbody), typeof(GrabbableProperty))]
    public class SnappableProperty : TrainingSceneObjectProperty
    {
        public class SnappedEventArgs : EventArgs
        {
            public readonly SnapZoneProperty SnapZone;
            public SnappedEventArgs(SnapZoneProperty snapZone)
            {
                SnapZone = snapZone;
            }
        }

        public event EventHandler<SnappedEventArgs> Snapped;
        public event EventHandler<SnappedEventArgs> Unsnapped;

        public bool IsSnapped
        {
            get
            {
                return SnappedZone != null;
            }
        }

        public SnapZoneProperty SnappedZone { get; protected set; }

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

            SnapZoneProperty previouslySnappedZone = SnappedZone;
            SnappedZone = null;
            EmitUnsnapped(previouslySnappedZone);
        }

        protected void EmitSnapped(SnapZoneProperty snapZone)
        {
            if (Snapped != null)
            {
                Snapped.Invoke(this, new SnappedEventArgs(snapZone));
            }
        }

        protected void EmitUnsnapped(SnapZoneProperty snapZone)
        {
            if (Unsnapped != null)
            {
                Unsnapped.Invoke(this, new SnappedEventArgs(snapZone));
            }
        }

        /// <summary>
        /// Instantaneously snap the object into <paramref name="snapZone"/>
        /// </summary>
        public void FastForwardSnapInto(SnapZoneProperty snapZone)
        {
            if (SnappedZone != null)
            {
                SnappedZone.SnapZone.ForceUnsnap();
            }

            if (snapZone.SnapZone.GetCurrentSnappedInteractableObject() != null)
            {
                snapZone.SnapZone.ForceUnsnap();
            }

            snapZone.SnapZone.ForceSnapObjectWithoutTransition(gameObject);
        }
    }
}
