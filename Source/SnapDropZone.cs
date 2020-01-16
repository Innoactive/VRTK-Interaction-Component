using System;
using Innoactive.Hub.Unity;
using UnityEngine;
using VRTK;

namespace Innoactive.Hub.Interaction
{
    /// <summary>
    /// Extension of the `VRTK_SnapDropZone` that allows forcing a snapped object to stay snapped, even if it leaves the collider.
    /// Also provides a method to force snapping without any animation.
    /// Observes snapped object, to remove them from the snap zone when they are deleted.
    /// </summary>
    public class SnapDropZone : VRTK_SnapDropZone
    {
        /// <summary>
        /// Name of the highlight container GameObject.
        /// </summary>
        public const string HighlightContainerName = HIGHLIGHT_CONTAINER_NAME;

        #region Properties

        /// <summary>
        /// Indicates if the snapped objects should stay snapped even if they leave the collider.
        /// </summary>
        public bool ForceStaySnapped { get; set; }

        /// <summary>
        /// Indicates if the snapped objects should stay snapped while GameObject is getting disabled regardless of the <see cref="ForceStaySnapped"/> settings.
        /// </summary>
        public bool StaySnappedOnDisable { get; set; }

        /// <summary>
        /// Ignores collisions with other `VRTK_SnapDropZone`s to prevent unwanted snapping/unsnapping.
        /// Can be used to solve the immediate unsnapping after snapping together two objects with `VRTK_SnapDropZone`s.
        /// </summary>
        [Obsolete("No longer necessary and is not being used.")]
        public bool IgnoreCollisionsWithSnapDropZones { get; set; }

        /// <summary>
        /// GameObject that is used to highlight snapping target for the `VRTK_SnapDropZone`s.
        /// </summary>
        public GameObject HighlightObject
        {
            get
            {
                return highlightObject;
            }
        }

        /// <summary>
        /// Disable colliders of snapped object.
        /// </summary>
        public bool DisableSnappedObjectColliders { get; set; }

        #endregion

        private bool isGettingDisabled = false;

        public void DestroySnappedObject(bool destroyChildSnappedObjects)
        {
            if (currentSnappedObject)
            {
                if (destroyChildSnappedObjects)
                {
                    SnapDropZone[] zones = currentSnappedObject.GetComponentsInChildren<SnapDropZone>();
                    foreach (SnapDropZone zone in zones)
                    {
                        if (zone.currentSnappedObject)
                        {
                            GameObject.Destroy(zone.currentSnappedObject);
                        }
                    }
                }
                GameObject.Destroy(currentSnappedObject);
                ForceStaySnapped = false;
            }
        }

        protected override void OnDisable()
        {
            isGettingDisabled = true;
            base.OnDisable();
            isGettingDisabled = false;
        }

        protected override void OnTriggerEnter(Collider collider)
        {
            if (enabled)
            {
                base.OnTriggerEnter(collider);
            }
        }

        protected override void OnTriggerExit(Collider collider)
        {
            if (enabled == false)
            {
                return;
            }

            // If the current valid snapped object is the collider leaving the trigger then attempt to turn off the highlighter.
            if (IsObjectHovering(collider.gameObject))
            {
                ToggleHighlight(collider.GetComponentInParent<VRTK_InteractableObject>(), false);
            }

            // parent logic
            base.OnTriggerExit(collider);

            if (currentSnappedObject != null && currentSnappedObject.gameObject == collider.gameObject)
            {
                // Do not unsnap if the snapzone on your own is not ours.
                Rigidbody rigidBody = GetComponentInParent<Rigidbody>();
                if (rigidBody != null)
                {
                    if (rigidBody.detectCollisions == false)
                    {
                        return;
                    }
                }

                ForceUnsnap();
            }
        }

        public override bool IsObjectHovering(GameObject checkObject)
        {
            bool res = base.IsObjectHovering(checkObject);
            if (res == false)
            {
                VRTK_InteractableObject ioCheck = checkObject.GetComponentInParent<VRTK_InteractableObject>();
                if (ioCheck != null && ioCheck.gameObject != checkObject)
                {
                    res = base.IsObjectHovering(ioCheck.gameObject);
                }
            }
            return res;
        }

        protected override void SnapObject(VRTK_InteractableObject interactable)
        {
            bool wasSnapped = isSnapped;
            base.SnapObject(interactable);
            if (wasSnapped == false && isSnapped)
            {
                if (DisableSnappedObjectColliders)
                {
                    DisableColliders(interactable.gameObject);
                }
            }
        }

        protected void DisableColliders(GameObject targetObject)
        {
            Rigidbody rigidBody = targetObject.GetComponent<Rigidbody>();

            if (rigidBody)
            {
                rigidBody.detectCollisions = false;
            }

            Collider[] colliders = targetObject.GetComponentsInChildren<Collider>();

            foreach (Collider subCollider in colliders)
            {
                subCollider.enabled = false;
            }
        }

        /// <summary>
        /// Snaps <paramref name="objectToSnap"/> immediately to this snap drop zone.
        /// </summary>
        /// <param name="objectToSnap">GameObject to snap to this snap drop zone</param>
        public virtual void ForceSnapObjectWithoutTransition(GameObject objectToSnap)
        {
            VRTK_InteractableObject interactableSnapObject = objectToSnap.GetComponent<VRTK_InteractableObject>();
            float previousSnapDuration = snapDuration;
            snapDuration = -1f;
            interactableSnapObject.SaveCurrentState();
            AttemptForceSnap(interactableSnapObject);
            snapDuration = previousSnapDuration;
        }

        public override void ForceUnsnap()
        {
            if (ForceStaySnapped || (StaySnappedOnDisable && isGettingDisabled))
            {
                return;
            }

            base.ForceUnsnap();
        }

        /// <summary>
        /// Refreshes the highlighter on the snap zone by reinitializing it.
        /// </summary>
        public void RefreshHighlighter()
        {
            if (HighlightObject == null)
            {
                InitaliseHighlightObject();
            }

            if (HighlightObject != null)
            {
                InitialiseHighlighter();
            }
        }

        /// <summary>
        /// Always uses DestroyImmediate to delete the highlight object.
        /// This way, initialising a new highlight object during the same frame does not fail.
        /// </summary>
        protected override void DeleteHighlightObject()
        {
            if (transform.Find(HIGHLIGHT_CONTAINER_NAME))
            {
                DestroyImmediate(transform.Find(HIGHLIGHT_CONTAINER_NAME).gameObject);
            }

            highlightContainer = null;
            highlightObject = null;
            objectHighlighter = null;
        }
    }
}
