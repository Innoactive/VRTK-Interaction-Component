using System;
using UnityEngine;
using VRTK.Highlighters;
using System.Runtime.Serialization;
using Innoactive.Creator.Core;
using Innoactive.Creator.Core.Attributes;
using Innoactive.Creator.Core.Behaviors;
using Innoactive.Creator.Core.Configuration;
using Innoactive.Creator.Core.Configuration.Modes;
using Innoactive.Creator.Core.SceneObjects;
using Innoactive.Creator.Core.Utils;
using Innoactive.Creator.Unity;
using Innoactive.Creator.VRTKInteraction.Properties;

namespace Innoactive.Creator.VRTKInteraction.Behaviors
{
    /// <summary>
    /// Behavior that highlights the target <see cref="ISceneObject"/> with the specified color until the behavior is being deactivated.
    /// </summary>
    [Obsolete("This behavior is obsolete now. Use the `Innoactive.Hub.Training.Behaviors.HighlightObject` behavior instead.")]
    [DataContract(IsReference = true)]
    public class VRTKObjectHighlight : Behavior<VRTKObjectHighlight.EntityData>
    {
        [DisplayName("Highlight Object")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// <see cref="ModeParameter{T}"/> of the highlight color.
            /// Training modes can change the highlight color.
            /// </summary>
            public ModeParameter<Color> CustomHighlightColor { get; set; }

            /// <summary>
            /// Highlight color set in the Step Inspector.
            /// </summary>
            [DataMember]
            [DisplayName("Highlight color")]
            public Color HighlightColor
            {
                get { return CustomHighlightColor.Value; }

                set { CustomHighlightColor = new ModeParameter<Color>("HighlightColor", value); }
            }

            /// <summary>
            /// Target scene object to be highlighted.
            /// </summary>
            [DataMember]
            [DisplayName("Object to highlight")]
            public SceneObjectReference Target { get; set; }

            public VRTK_BaseHighlighter Highlight { get; set; }

            /// <summary>
            /// Metadata used for undo and redo feature.
            /// </summary>
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            public string Name { get; set; }
        }

        private class ActivatingProcess : InstantProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                HighlightProperty highlighter = Data.Target.Value.GameObject.GetOrAddComponent<HighlightProperty>();
                highlighter.Highlight(Data.HighlightColor);
            }
        }

        private class DeactivatingProcess : InstantProcess<EntityData>
        {
            public DeactivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                HighlightProperty highlighter = Data.Target.Value.GameObject.GetOrAddComponent<HighlightProperty>();
                highlighter.Unhighlight();
            }
        }

        private class EntityConfigurator : Configurator<EntityData>
        {
            public EntityConfigurator(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Configure(IMode mode, Stage stage)
            {
                Data.CustomHighlightColor.Configure(mode);
            }
        }

        public VRTKObjectHighlight() : this("", Color.magenta)
        {
        }

        public VRTKObjectHighlight(string name, Color highlightColor)
        {
            Data.Target = new SceneObjectReference(name);
            Data.HighlightColor = highlightColor;
        }

        public VRTKObjectHighlight(ISceneObject target, Color highlightColor) : this(TrainingReferenceUtils.GetNameFrom(target), highlightColor)
        {
        }

        /// <inheritdoc />
        public override IProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }

        /// <inheritdoc />
        public override IProcess GetDeactivatingProcess()
        {
            return new DeactivatingProcess(Data);
        }

        /// <inheritdoc />
        protected override IConfigurator GetConfigurator()
        {
            return new EntityConfigurator(Data);
        }
    }
}