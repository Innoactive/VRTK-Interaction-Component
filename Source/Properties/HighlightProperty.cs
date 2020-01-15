using System;
using UnityEngine;
using VRTK;
using VRTK.Highlighters;

namespace Innoactive.Hub.Training.SceneObjects.Properties
{
    /// <summary>
    /// Property that is used to highlight training scene objects.
    /// Interaction highlights (e.g. color change when touching an object) are still working properly.
    /// </summary>
    public class HighlightProperty : TrainingSceneObjectProperty
    {
        private static readonly Common.Logging.ILog logger = Logging.LogManager.GetLogger<HighlightProperty>();

        /// <summary>
        /// Event data for events of <see cref="HighlightProperty"/>.
        /// </summary>
        public class HighlightEventArgs : EventArgs { }

        /// <summary>
        /// Emitted when the object gets highlighted.
        /// </summary>
        public event EventHandler<HighlightEventArgs> ObjectHighlighted;

        /// <summary>
        /// Emitted when the object gets unhighlighted.
        /// </summary>
        public event EventHandler<HighlightEventArgs> ObjectUnhighlighted;

        /// <summary>
        /// Returns the highlight color, if the object is currently highlighted.
        /// Returns null, otherwise.
        /// </summary>
        public Color? CurrentHighlightColor { get; protected set; }

        /// <summary>
        /// The VRTK highlighter which is used to highlight the target object, if it is an interactable object.
        /// </summary>
        protected VRTK_InteractObjectHighlighter InteractableHighlighter { get; set; }

        /// <summary>
        /// The VRTK highlighter which is used to highlight the target object, if it is not an interactable object.
        /// </summary>
        protected VRTK_BaseHighlighter NonInteractableHighlighter { get; set; }

        /// <summary>
        /// Event handler that changes the object highlight color to this behavior's highlight color again
        /// when the object color was reset because of a VRTK interaction.
        /// </summary>
        protected InteractObjectHighlighterEventHandler UnhighlightHandler;

        /// <summary>
        /// Sets up the highlighter to be used.
        /// If the game object is an interactable object (touchable, grabbable, useable, ...), then <see cref="InteractableHighlighter"/> is initialised.
        /// If not, then <see cref="NonInteractableHighlighter"/> is.
        /// </summary>
        public void Initialise()
        {
            InteractableHighlighter = null;
            NonInteractableHighlighter = null;
            CurrentHighlightColor = null;

            InteractableHighlighter = gameObject.GetComponent<VRTK_InteractObjectHighlighter>();

            if (InteractableHighlighter != null)
            {
                return;
            }

            NonInteractableHighlighter = GetComponent<VRTK_BaseHighlighter>();

            if (NonInteractableHighlighter == null)
            {
                NonInteractableHighlighter = gameObject.AddComponent<VRTK_MaterialColorSwapHighlighter>();
            }
        }

        /// <summary>
        /// Turns on the highlighter with the given <paramref name="highlightColor"/>.
        /// </summary>
        /// <exception cref="NullReferenceException">Thrown, when there is no valid VRTK highlighter and none can be automatically added.</exception>
        public void Highlight(Color highlightColor = default(Color))
        {
            if (InteractableHighlighter == null && NonInteractableHighlighter == null)
            {
                Initialise();
            }

            if (InteractableHighlighter != null)
            {
                if (UnhighlightHandler != null)
                {
                    InteractableHighlighter.InteractObjectHighlighterUnhighlighted -= UnhighlightHandler;
                }

                UnhighlightHandler = (sender, args) =>
                {
                    InteractableHighlighter.Highlight(highlightColor);
                    CurrentHighlightColor = highlightColor;
                };

                InteractableHighlighter.InteractObjectHighlighterUnhighlighted += UnhighlightHandler;

                if (IsInteractableObjectHighlighted(InteractableHighlighter) == false)
                {
                    InteractableHighlighter.Highlight(highlightColor);
                }
            }
            else if (NonInteractableHighlighter != null)
            {
                NonInteractableHighlighter.Unhighlight();
                NonInteractableHighlighter.Initialise();
                NonInteractableHighlighter.Highlight(highlightColor);
            }
            else
            {
                throw new NullReferenceException("No valid VRTK highlighter found. Object cannot be highlighted.");
            }

            CurrentHighlightColor = highlightColor;

            if (ObjectHighlighted != null)
            {
                ObjectHighlighted.Invoke(this, new HighlightEventArgs());
            }
        }

        /// <summary>
        /// Turns off the highlighter.
        /// </summary>
        public void Unhighlight()
        {
            if (InteractableHighlighter != null)
            {
                if (UnhighlightHandler != null)
                {
                    InteractableHighlighter.InteractObjectHighlighterUnhighlighted -= UnhighlightHandler;
                    UnhighlightHandler = null;
                }

                if (IsInteractableObjectHighlighted(InteractableHighlighter) == false)
                {
                    InteractableHighlighter.Unhighlight();
                }
            }
            else if (NonInteractableHighlighter != null)
            {
                NonInteractableHighlighter.Unhighlight();
            }
            else
            {
                logger.Warn("No valid highlighter found. Cannot be unhighlighted.");
                return;
            }

            CurrentHighlightColor = null;

            if (ObjectUnhighlighted != null)
            {
                ObjectUnhighlighted.Invoke(this, new HighlightEventArgs());
            }
        }

        /// <summary>
        /// Check if the target game object is currently being highlighted by a VRTK interaction.
        /// </summary>
        /// <param name="highlighter">The current highlighter instance.</param>
        protected bool IsInteractableObjectHighlighted(VRTK_InteractObjectHighlighter highlighter)
        {
            VRTK_InteractableObject target = highlighter.objectToMonitor;
            return (target.IsNearTouched() && highlighter.nearTouchHighlight != Color.clear) ||
                (target.IsTouched() && highlighter.touchHighlight != Color.clear) ||
                (target.IsGrabbed() && highlighter.grabHighlight != Color.clear);
        }
    }
}
