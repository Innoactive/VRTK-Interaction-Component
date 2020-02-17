#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Innoactive.Hub.Training.Audio;
using Innoactive.Hub.Training;
using Innoactive.Hub.Training.Behaviors;
using Innoactive.Hub.Training.Conditions;
using Innoactive.Hub.Training.SceneObjects;
using Innoactive.Hub.Training.SceneObjects.Properties;
using Innoactive.Hub.Training.Utils.Builders;
using Innoactive.Hub.Training.Utils.Serialization;
using Innoactive.Hub.Unity.Tests.Training.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using HighlightProperty = Innoactive.Hub.Training.SceneObjects.Properties.VRTK.HighlightProperty;

namespace Innoactive.Hub.Unity.Tests.Training
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
            ICourse training2 = Serializer.ToCourse(Serializer.ToByte(training1));

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
                        .DisableAutomaticAudioHandling()
                        .AddBehavior(new HighlightObjectBehavior(testObject.GetProperty<HighlightProperty>(), Color.green))))
                .Build();

            // When we serialize and deserialize it
            byte[] serialized = Serializer.ToByte(training1);
            ICourse training2 = Serializer.ToCourse(serialized);

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

        [Obsolete("VRTKObjectHighlight behavior is obsolete. It will be removed in v2.x.x.")]
        [UnityTest]
        // ReSharper disable once InconsistentNaming
        public IEnumerator VRTKObjectHighlight()
        {
            // Given a training with a VRTK highlight behavior
            TrainingSceneObject testObject = TestingUtils.CreateSceneObject("TestObject");

            ICourse training1 = new LinearTrainingBuilder("Training")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .DisableAutomaticAudioHandling()
                        .AddBehavior(new VRTKObjectHighlight(testObject, Color.green))))
                .Build();

            // When we serialize and deserialize it
            byte[] serialized = Serializer.ToByte(training1);
            ICourse training2 = Serializer.ToCourse(serialized);

            // Then highlight color, duration and target should stay the same.
            VRTKObjectHighlight condition1 = training1.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as VRTKObjectHighlight;
            VRTKObjectHighlight condition2 = training2.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as VRTKObjectHighlight;

            Assert.IsNotNull(condition1);
            Assert.IsNotNull(condition2);
            Assert.AreEqual(condition1.Data.HighlightColor, condition2.Data.HighlightColor);
            Assert.AreEqual(condition1.Data.Target.Value, condition2.Data.Target.Value);

            // Cleanup
            TestingUtils.DestroySceneObject(testObject);

            return null;
        }
    }
}

#endif
