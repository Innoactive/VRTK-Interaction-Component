using System;
using UnityEngine;

namespace Innoactive.Creator.VRTKInteraction.Properties
{
    public class SnappedEventArgs : EventArgs
    {
        public readonly GameObject SnappedObject;

        public SnappedEventArgs(GameObject snappedObject)
        {
            SnappedObject = snappedObject;
        }
    }
}
