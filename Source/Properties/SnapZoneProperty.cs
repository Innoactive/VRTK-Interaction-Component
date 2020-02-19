﻿using Innoactive.Hub.Interaction;
using System;
using Innoactive.Hub.Training.Configuration;
using Innoactive.Hub.Training.Configuration.Modes;
using UnityEngine;
using VRTK;

namespace Innoactive.Hub.Training.SceneObjects.Properties
{
    [RequireComponent(typeof(SnapDropZone))]
    public class SnapZoneProperty : LockableProperty
    {
        public event EventHandler<SnappedEventArgs> ObjectSnapped;
        public event EventHandler<SnappedEventArgs> ObjectUnsnapped;

        private GameObject highlightPrefab;

        public ModeParameter<bool> IsShowingHighlight { get; private set; }
        public ModeParameter<bool> IsAlwaysShowingHighlight { get; private set; }
        public ModeParameter<Color> HighlightColor { get; private set; }

        public bool IsObjectSnapped
        {
            get
            {
                return SnappedObject != null;
            }
        }

        public SnappableProperty SnappedObject { get; protected set; }

        public SnapDropZone SnapZone { get; protected set; }

        public void Configure(IMode mode)
        {
            InitializeModeParameters();

            IsAlwaysShowingHighlight.Configure(mode);
            IsShowingHighlight.Configure(mode);
            HighlightColor.Configure(mode);
        }

        public void ForceUnsnap()
        {
            SnapZone.ForceUnsnap();
        }

        public void ForceSnap(GameObject gameObject)
        {
            SnappedObject = gameObject.GetComponent<SnappableProperty>();
            SnapZone.ForceSnap(gameObject);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            InitializeModeParameters();

            SnapZone.ObjectSnappedToDropZone += HandleObjectSnapped;
            SnapZone.ObjectUnsnappedFromDropZone += HandleObjectUnsnapped;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            SnapZone.ObjectSnappedToDropZone -= HandleObjectSnapped;
            SnapZone.ObjectUnsnappedFromDropZone -= HandleObjectUnsnapped;
        }

        private void HandleObjectSnapped(object sender, SnapDropZoneEventArgs args)
        {
            SnappedObject = args.snappedObject.GetComponent<SnappableProperty>();
            if (SnappedObject == null)
            {
                Debug.LogWarningFormat("SnapZone '{0}' received snap from object '{1}' without SnappableProperty", SceneObject.UniqueName, args.snappedObject.name);
            }
            else
            {
                EmitSnapped(args.snappedObject);
            }
        }

        private void HandleObjectUnsnapped(object sender, SnapDropZoneEventArgs args)
        {
            if (SnappedObject != null)
            {
                EmitUnsnapped(args.snappedObject);
            }
        }

        private void InitializeModeParameters()
        {
            if (SnapZone == null)
            {
                SnapZone = GetComponent<SnapDropZone>();
            }

            if (IsShowingHighlight == null)
            {
                highlightPrefab = SnapZone.highlightObjectPrefab;

                IsShowingHighlight = new ModeParameter<bool>("ShowSnapzoneHighlight", highlightPrefab != null);
                IsShowingHighlight.ParameterModified += (sender, args) =>
                {
                    SnapZone.highlightObjectPrefab = IsShowingHighlight.Value ? highlightPrefab : null;
                    SnapZone.RefreshHighlighter();

                    if (SnapZone.HighlightObject != null)
                    {
                        SnapZone.HighlightObject.SetActive(IsShowingHighlight.Value);
                    }
                };
            }

            if (IsAlwaysShowingHighlight == null)
            {
                IsAlwaysShowingHighlight = new ModeParameter<bool>("AlwaysShowSnapzoneHighlight", SnapZone.highlightAlwaysActive);
                IsAlwaysShowingHighlight.ParameterModified += (sender, args) =>
                {
                    SnapZone.highlightAlwaysActive = IsAlwaysShowingHighlight.Value;
                    SnapZone.RefreshHighlighter();
                };
            }

            if (HighlightColor == null)
            {
                HighlightColor = new ModeParameter<Color>("HighlightColor", SnapZone.highlightColor);
                HighlightColor.ParameterModified += (sender, args) =>
                {
                    SnapZone.highlightColor = HighlightColor.Value;
                    SnapZone.RefreshHighlighter();
                };
            }
        }

        protected void EmitSnapped(GameObject snappedObject)
        {
            if (ObjectSnapped != null)
            {
                ObjectSnapped.Invoke(this, new SnappedEventArgs(snappedObject));
            }
        }

        protected void EmitUnsnapped(GameObject unsnappedObject)
        {
            if (ObjectUnsnapped != null)
            {
                ObjectUnsnapped.Invoke(this, new SnappedEventArgs(unsnappedObject));
            }
        }

        protected override void InternalSetLocked(bool lockState)
        {
            SnapZone.enabled = lockState == false;
        }
    }
}
