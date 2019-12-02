using System;
using UnityEngine;

namespace Innoactive.Hub.Training.SceneObjects.Properties
{
    public class SnappedEventArgs : EventArgs
    {
        public readonly GameObject SnappedObject;
        public readonly SnapZoneProperty SnapZone;

        public SnappedEventArgs(SnapZoneProperty snapZone, GameObject snappedObject)
        {
            SnappedObject = snappedObject;
            SnapZone = snapZone;
        }
    }
}
