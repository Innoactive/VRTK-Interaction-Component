﻿using Innoactive.Hub.Training.Editors.Configuration;
using UnityEngine;

namespace Innoactive.Hub.Training.Conditions.Editors
{
    public class GrabbedMenuItem : Menu.Item<ICondition>
    {
        public override GUIContent DisplayedName
        {
            get
            {
                return new GUIContent("Grab Object");
            }
        }

        public override ICondition GetNewItem()
        {
            return new GrabbedCondition();
        }
    }
}
