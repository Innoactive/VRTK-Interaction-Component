using System;
using UnityEngine;

namespace Innoactive.Hub.Training.SceneObjects.Properties
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
