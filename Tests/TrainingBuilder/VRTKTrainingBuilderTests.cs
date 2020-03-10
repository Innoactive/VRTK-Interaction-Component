using NUnit.Framework;
using System.Collections;
using System.Linq;
using Innoactive.Creator.Core.Tests.Utils;
using Innoactive.Hub.Training;
using Innoactive.Hub.Training.Conditions;
using Innoactive.Hub.Training.Interaction.Conditions;
using Innoactive.Hub.Training.SceneObjects;
using Innoactive.Hub.Training.SceneObjects.Properties;
using Innoactive.Hub.Training.Utils.Builders;
using Innoactive.Hub.Training.Utils.Interaction.Builders;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Innoactive.Creator.VRTKInteraction.Tests.Utils
{
    public class VRTKTrainingBuilderTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator BuildingGrabTest()
        {
            // Given a `GrabbableProperty` and a builder for a training with a Grab default step
            GameObject go = new GameObject("TestGrabbable");
            TrainingSceneObject to = go.AddComponent<TrainingSceneObject>();
            go.AddComponent<GrabbableProperty>();
            to.ChangeUniqueName("Grabbable");


            LinearTrainingBuilder builder = new LinearTrainingBuilder("TestTraining")
                .AddChapter(new LinearChapterBuilder("TestChapter")
                    .AddStep(InteractionDefaultSteps.Grab("TestGrabStep", "Grabbable")));

            // When we build a training from it
            IStep step = builder.Build().Data.FirstChapter.Data.FirstStep;

            //Then it should has a stap with a GrabbedCondition which refers to the `GrabbableProperty`.
            Assert.True(step != null);
            Assert.True(step.Data.Name == "TestGrabStep");
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.Count == 1);
            GrabbedCondition condition = step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as GrabbedCondition;

            Assert.True(ReferenceEquals(to, condition.Data.GrabbableProperty.Value.SceneObject));

            // Cleanup
            Object.DestroyImmediate(go);

            yield return null;
        }

        [UnityTest]
        public IEnumerator BuildingSnapZonePutTest()
        {
            // Given a snap zone and snappable property and a builder for a training with a PutIntoSnapZone default step
            GameObject snapZoneGo = new GameObject("SnapZone");
            TrainingSceneObject snapZone = snapZoneGo.AddComponent<TrainingSceneObject>();
            snapZoneGo.AddComponent<SnapZoneProperty>();
            snapZone.ChangeUniqueName("SnapZone");

            GameObject putGo = new GameObject("Puttable");
            TrainingSceneObject objectToPut = putGo.AddComponent<TrainingSceneObject>();
            putGo.AddComponent<SnappableProperty>();
            objectToPut.ChangeUniqueName("ToPut");

            LinearTrainingBuilder builder = new LinearTrainingBuilder("TestTraining")
                .AddChapter(new LinearChapterBuilder("TestChapter")
                    .AddStep(InteractionDefaultSteps.PutIntoSnapZone("TestSnapZonePutStep", "SnapZone", "ToPut")));

            // When you build a training with it
            IStep step = builder.Build().Data.FirstChapter.Data.FirstStep;

            // Then it has a step with a SnappedCondition
            Assert.True(step != null);
            Assert.True(step.Data.Name == "TestSnapZonePutStep");
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.Count == 1);
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() is SnappedCondition);
            Assert.True(ReferenceEquals((step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as SnappedCondition).Data.Target.Value.SceneObject, objectToPut));

            // Cleanup
            Object.DestroyImmediate(snapZoneGo);
            Object.DestroyImmediate(putGo);

            return null;
        }

        [UnityTest]
        public IEnumerator BuildingUseTest()
        {
            // Given a usable property and a builder for a training with Use default step
            GameObject usableGo = new GameObject("Usable");
            TrainingSceneObject usable = usableGo.AddComponent<TrainingSceneObject>();
            usableGo.AddComponent<UsableProperty>();
            usable.ChangeUniqueName("Usable");

            LinearTrainingBuilder builder = new LinearTrainingBuilder("TestTraining")
                .AddChapter(new LinearChapterBuilder("TestChapter")
                    .AddStep(InteractionDefaultSteps.Use("TestUseStep", "Usable")));

            // When you build a training with it
            IStep step = builder.Build().Data.FirstChapter.Data.FirstStep;

            // Then it has a step with an UsedCondition
            Assert.True(step != null);
            Assert.True(step.Data.Name == "TestUseStep");
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.Count == 1);
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() is UsedCondition);
            Assert.True(ReferenceEquals((step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as UsedCondition).Data.UsableProperty.Value.SceneObject, usable));

            // Cleanup
            Object.DestroyImmediate(usableGo);

            return null;
        }

        [UnityTest]
        public IEnumerator BuildingTouchTest()
        {
            // Given you have a touchable property and a builder for a training with Touch default step
            GameObject touchableGo = new GameObject("Touchable");
            TrainingSceneObject touchable = touchableGo.AddComponent<TrainingSceneObject>();
            touchableGo.AddComponent<TouchableProperty>();
            touchable.ChangeUniqueName("Touchable");

            LinearTrainingBuilder builder = new LinearTrainingBuilder("TestTraining")
                .AddChapter(new LinearChapterBuilder("TestChapter")
                    .AddStep(InteractionDefaultSteps.Touch("TestTouchStep", "Touchable")));

            // When you build a training with it
            IStep step = builder.Build().Data.FirstChapter.Data.FirstStep;

            // Then it has a step with a TouchCOndition
            Assert.True(step != null);
            Assert.True(step.Data.Name == "TestTouchStep");
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.Count == 1);
            Assert.True(step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() is TouchedCondition);
            Assert.True(ReferenceEquals((step.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as TouchedCondition).Data.TouchableProperty.Value.SceneObject, touchable));

            // Cleanup
            Object.DestroyImmediate(touchableGo);

            return null;
        }
    }
}
