using System.Collections.Generic;
using UnityEngine;
using VRTK;
namespace Innoactive.Creator.VRTKInteraction
{
    /// <summary>
    /// Extension if VRTK_InteractableObject that automatically applies the highlight color from HubSettings,
    /// and also fixes minor bugs with the VRTK_InteractableObject not yet fixes in the official repository
    /// </summary>
    public class InteractableObject : VRTK_InteractableObject
    {
        protected override void OnDisable()
        {
            base.OnDisable();

            // VRTK_InteractableObject calls ForceStopInteracting, which starts a Coroutine that is aborted
            // because the object becomes disabled afterwards. This causes problems like missed OnUntouch etc. event.
            // TO fix this, we explicitly call ForceStopAllInteractions
            if (!startDisabled)
            {
                ForceStopAllInteractions();
            }
        }

        /// <summary>
        ///  VRTK's implementation is bugged: objects are untouched, removing touched objects from touchingObjects,
        ///  leading to an enumerator corruption
        /// </summary>
        protected override void StopTouchingInteractions()
        {
            HashSet<GameObject> touchingObjectsCopy = new HashSet<GameObject>(touchingObjects);
            foreach (GameObject touchingObject in touchingObjectsCopy)
            {
                if (touchingObject.activeInHierarchy || forceDisabled)
                {
                    touchingObject.GetComponentInChildren<VRTK_InteractTouch>().ForceStopTouching();
                }
            }
        }
    }
}