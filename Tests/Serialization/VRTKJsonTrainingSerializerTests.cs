using System.Collections;
using System.Linq;
using Innoactive.Creator.BasicInteraction.Conditions;
using Innoactive.Creator.Core;
using Innoactive.Creator.Core.Behaviors;
using Innoactive.Creator.Core.SceneObjects;
using Innoactive.Creator.Core.Properties;
using Innoactive.Creator.Tests.Builder;
using Innoactive.Creator.Tests.Utils;
using Innoactive.Creator.VRTKInteraction.Properties;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using HighlightProperty = Innoactive.Creator.VRTKInteraction.Properties.HighlightProperty;

namespace Innoactive.Creator.VRTKInteraction.Tests.Utils
{
    public class VRTKJsonTrainingSerializerTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator SnappedCondition()
        {
            // Given a training with a SnappedCondition,
            SnapZoneProperty snapZone = TestingUtils.CreateSceneObject<SnapZoneProperty>("SnapZone").GetProperty<SnapZoneProperty>();
            SnappableProperty testObject = TestingUtils.CreateSceneObject<SnappableProperty>("TestObject").GetProperty<SnappableProperty>();

            ICourse training1 = new LinearTrainingBuilder("Training")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new SnappedCondition(testObject, snapZone))))
                .Build();

            // When we serialize and deserialize it,
            ICourse training2 = Serializer.CourseFromByteArray(Serializer.CourseToByteArray(training1));

            // Then that condition's target object and snap zone should stay unchanged.
            SnappedCondition condition1 = training1.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as SnappedCondition;
            SnappedCondition condition2 = training2.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as SnappedCondition;

            Assert.IsNotNull(condition1);
            Assert.IsNotNull(condition2);
            Assert.AreEqual(condition1.Data.Target.Value, condition2.Data.Target.Value);
            Assert.AreEqual(condition1.Data.ZoneToSnapInto.Value, condition2.Data.ZoneToSnapInto.Value);

            // Cleanup
            TestingUtils.DestroySceneObject(snapZone);
            TestingUtils.DestroySceneObject(testObject);

            return null;
        }

        [UnityTest]
        // ReSharper disable once InconsistentNaming
        public IEnumerator HighlightObjectBehavior()
        {
            // Given a training with a VRTK highlight behavior
            TrainingSceneObject testObject = TestingUtils.CreateSceneObject("TestObject");
            testObject.AddTrainingProperty<HighlightProperty>();

            ICourse training1 = new LinearTrainingBuilder("Training")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddBehavior(new HighlightObjectBehavior(testObject.GetProperty<HighlightProperty>(), Color.green))))
                .Build();

            // When we serialize and deserialize it
            byte[] serialized = Serializer.CourseToByteArray(training1);
            ICourse training2 = Serializer.CourseFromByteArray(serialized);

            // Then highlight color, duration and target should stay the same.
            HighlightObjectBehavior highlightObjectBehavior = training1.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as HighlightObjectBehavior;
            HighlightObjectBehavior highlightObjectBehavior2 = training2.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as HighlightObjectBehavior;

            Assert.IsNotNull(highlightObjectBehavior);
            Assert.IsNotNull(highlightObjectBehavior2);
            Assert.AreEqual(highlightObjectBehavior.Data.HighlightColor, highlightObjectBehavior2.Data.HighlightColor);
            Assert.AreEqual(highlightObjectBehavior.Data.ObjectToHighlight.Value, highlightObjectBehavior2.Data.ObjectToHighlight.Value);

            // Cleanup
            TestingUtils.DestroySceneObject(testObject);

            return null;
        }
    }
}
