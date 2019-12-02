using System;
using Innoactive.Hub.Unity;
using UnityEngine;
using VRTK;

namespace Innoactive.Hub.Training.SceneObjects.Properties
{
    [RequireComponent(typeof(GrabbableProperty))]
    public class SnappableProperty : TrainingSceneObjectProperty
    {
        private static readonly Common.Logging.ILog logger = Logging.LogManager.GetLogger<SnappableProperty>();

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

        protected override void OnEnable()
        {
            base.OnEnable();

            // TODO: think about init order of multiple properties
            interactable = gameObject.GetComponent<VRTK_InteractableObject>(true);
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
                logger.WarnFormat("Object '{0}' was snapped to SnapZone '{1}' without SnappableProperty", SceneObject.UniqueName, args.interactingObject.name);
                return;
            }

            if (LockObjectOnSnap == false )
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
                Snapped.Invoke(this, new SnappedEventArgs(snapZone, interactable.gameObject));
            }
        }

        protected void EmitUnsnapped(SnapZoneProperty snapZone)
        {
            if (Unsnapped != null)
            {
                Unsnapped.Invoke(this, new SnappedEventArgs(snapZone, interactable.gameObject));
            }
        }

        /// <summary>
        /// Instantaneously snap the object into <paramref name="snapZone"/>
        /// </summary>
        public void FastForwardSnapInto(SnapZoneProperty snapZone)
        {
            if (SnappedZone != null)
            {
                SnappedZone.ForceUnsnap();
            }

            SnapZoneProperty zoneToSnapTo = snapZone as SnapZoneProperty;
            if (zoneToSnapTo != null)
            {
                if (zoneToSnapTo.SnapZone.GetCurrentSnappedInteractableObject() != null)
                {
                    zoneToSnapTo.SnapZone.ForceUnsnap();
                }

                // In case this Interactable Object has not been touched yet.
                if (zoneToSnapTo.SnapZone.snapType == VRTK_SnapDropZone.SnapTypes.UseJoint && GetComponent<Rigidbody>() == null)
                {
                    gameObject.AddComponent<Rigidbody>();
                }

                zoneToSnapTo.ForceSnap(gameObject);
            }
            else
            {
                logger.ErrorFormat("Fastforward failed, different ISnapZoneProperty implemented, do you use VRTK?");
            }
        }
    }
}
