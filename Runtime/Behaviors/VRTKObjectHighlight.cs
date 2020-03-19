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
                get
                {
                    return CustomHighlightColor.Value;
                }

                set
                {
                    CustomHighlightColor = new ModeParameter<Color>("HighlightColor", value);
                }
            }

            /// <summary>
            /// Target scene object to be highlighted.
            /// </summary>
            [DataMember]
            [DisplayName("Object to highlight")]
            public SceneObjectReference Target { get; set; }

            public float FadeInDuration { get; set; }

            public float FadeOutDuration { get; set; }

            public VRTK_BaseHighlighter Highlight { get; set; }

            /// <summary>
            /// Metadata used for undo and redo feature.
            /// </summary>
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            public string Name { get; set; }
        }

        private class ActivatingProcess : InstantStageProcess<EntityData>
        {
            /// <inheritdoc />
            public override void Start(EntityData data)
            {
                HighlightProperty highlighter = data.Target.Value.GameObject.GetOrAddComponent<HighlightProperty>();
                highlighter.Highlight(data.HighlightColor);
            }
        }

        private class DeactivatingProcess : InstantStageProcess<EntityData>
        {
            /// <inheritdoc />
            public override void Start(EntityData data)
            {
                HighlightProperty highlighter = data.Target.Value.GameObject.GetOrAddComponent<HighlightProperty>();
                highlighter.Unhighlight();
            }
        }

        private class EntityConfigurator : IConfigurator<EntityData>
        {
            /// <inheritdoc />
            public void Configure(EntityData data, IMode mode, Stage stage)
            {
                data.CustomHighlightColor.Configure(mode);
            }
        }

        public VRTKObjectHighlight() : this("", Color.magenta)
        {
        }

        public VRTKObjectHighlight(string name, Color highlightColor)
        {
            Data = new EntityData()
            {
                Target = new SceneObjectReference(name),
                HighlightColor = highlightColor
            };
        }

        public VRTKObjectHighlight(ISceneObject target) : this(target, Color.magenta)
        {
        }

        public VRTKObjectHighlight(ISceneObject target, Color highlightColor) : this(TrainingReferenceUtils.GetNameFrom(target), highlightColor)
        {
        }

        [Obsolete("The parameters \"fadeInDuration\" and \"fadeOutDuration\" are not used anymore.")]
        public VRTKObjectHighlight(string name, Color highlightColor, float fadeInDuration = 0.5f, float fadeOutDuration = 0.5f)
        {
            Data = new EntityData()
            {
                Target = new SceneObjectReference(name),
                HighlightColor = highlightColor,
                FadeInDuration = fadeInDuration,
                FadeOutDuration = fadeOutDuration
            };
        }

        [Obsolete("The parameters \"fadeInDuration\" and \"fadeOutDuration\" are not used anymore.")]
        public VRTKObjectHighlight(ISceneObject target, Color highlightColor, float fadeInDuration = 0.5f, float fadeOutDuration = 0.5f) : this(TrainingReferenceUtils.GetNameFrom(target), highlightColor, fadeInDuration, fadeOutDuration)
        {
        }

        private readonly IProcess<EntityData> process = new Process<EntityData>(new ActivatingProcess(), new EmptyStageProcess<EntityData>(), new DeactivatingProcess());

        /// <inheritdoc />
        protected override IProcess<EntityData> Process
        {
            get
            {
                return process;
            }
        }

        private readonly IConfigurator<EntityData> configurator = new BaseConfigurator<EntityData>().Add(new EntityConfigurator());

        /// <inheritdoc />
        protected override IConfigurator<EntityData> Configurator
        {
            get
            {
                return configurator;
            }
        }
    }
}
