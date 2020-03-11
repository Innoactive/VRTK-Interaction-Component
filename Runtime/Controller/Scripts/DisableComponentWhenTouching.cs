using UnityEngine;
using VRTK;

namespace Innoactive.Hub.Interaction
{
    /// <summary>
    /// Script that disables a set of components when the corresponding VRTK_InteractTouch touches an object.
    /// </summary>
    public class DisableComponentWhenTouching : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("When an object is touched, objects in toEnable are disabled, unless this field is true, in which case they are enabled")]
        private bool setWhenTouching = false;

        [SerializeField]
        [Tooltip("Scripts to disable while the object is touched")]
        private MonoBehaviour[] toDisable = null;

        [SerializeField]
        [Tooltip("InteractTouch to observe - if null, a VRTK_InteractTouch will be searched on this game object and its parents")]
        private VRTK_InteractTouch interactHandler = null;

        private void Reset()
        {
            GrabInteractHandler();
        }

        private void OnEnable()
        {
            GrabInteractHandler();

            if (interactHandler != null)
            {
                interactHandler.ControllerTouchInteractableObject += HandleControllerTouchInteractableObject;
                interactHandler.ControllerUntouchInteractableObject += HandleControllerUntouchInteractableObject;
            }
            else
            {
                Debug.LogError("No VRTK_InteractTouch found");
            }
        }

        private void OnDisable()
        {
            if (interactHandler != null)
            {
                interactHandler.ControllerTouchInteractableObject -= HandleControllerTouchInteractableObject;
                interactHandler.ControllerUntouchInteractableObject -= HandleControllerUntouchInteractableObject;
            }
        }

        private void GrabInteractHandler()
        {
            if (interactHandler == null)
            {
                interactHandler = GetComponentInParent<VRTK_InteractTouch>();
            }
        }

        private void HandleControllerTouchInteractableObject(object sender, ObjectInteractEventArgs e)
        {
            SetAllActive(setWhenTouching);
        }

        private void HandleControllerUntouchInteractableObject(object sender, ObjectInteractEventArgs e)
        {
            SetAllActive(!setWhenTouching);
        }

        private void SetAllActive(bool activate = true)
        {
            foreach (MonoBehaviour component in toDisable)
            {
                component.enabled = activate;
            }
        }
    }
}