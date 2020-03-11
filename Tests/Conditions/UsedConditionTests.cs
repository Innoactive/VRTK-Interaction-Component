using System.Collections;
using Innoactive.Hub.Training;
using Innoactive.Hub.Training.Conditions;
using Innoactive.Hub.Training.SceneObjects.Properties;
using Innoactive.Creator.Core.Tests.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Innoactive.Creator.VRTKInteraction.Tests.Conditions
{
    public class UsedConditionTests : RuntimeTests
    {
        private class UsablePropertyMock : UsableProperty
        {
            private bool isUsed;
            public override bool IsBeingUsed
            {
                get
                {
                    return isUsed;
                }
            }

            public void SetUsed(bool value)
            {
                isUsed = value;
            }

            public void EmitIsUsed()
            {
                EmitUsageStarted();
            }
        }

        [UnityTest]
        public IEnumerator CompleteWhenUsed()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            UsablePropertyMock mockedProperty = obj.AddComponent<UsablePropertyMock>();

            yield return null;

            UsedCondition condition = new UsedCondition(mockedProperty);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When it is used
            mockedProperty.SetUsed(true);

            yield return null;
            condition.Update();

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator CompleteWhenItIsDoneOnStart()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            UsablePropertyMock mockedProperty = obj.AddComponent<UsablePropertyMock>();
            mockedProperty.SetUsed(true);

            yield return null;

            UsedCondition condition = new UsedCondition(mockedProperty);
            condition.LifeCycle.Activate();

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed due IsGrabbed is true
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator ConditionNotCompleted()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            UsableProperty usableProperty = obj.AddComponent<UsablePropertyMock>();
            UsedCondition condition = new UsedCondition(usableProperty);
            condition.LifeCycle.Activate();

            float startTime = Time.time;
            while (Time.time < startTime + 0.1f)
            {
                yield return null;
                condition.Update();
            }

            // Assert after doing nothing the condition is not completed.
            Assert.IsFalse(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator AutoCompleteActive()
        {
            // Given an used condition,
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            UsablePropertyMock mockedProperty = obj.AddComponent<UsablePropertyMock>();

            bool wasUsageStarted = false;
            bool wasUsageStopped = false;

            mockedProperty.UsageStarted += (sender, args) =>
            {
                wasUsageStarted = true;
                mockedProperty.UsageStopped += (o, eventArgs) => wasUsageStopped = true;
            };

            UsedCondition condition = new UsedCondition(mockedProperty);

            // When you activate and autocomplete it,
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            condition.Autocomplete();

            // Then condition is completed.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsTrue(condition.IsCompleted);
            Assert.IsTrue(wasUsageStarted);
            Assert.IsTrue(wasUsageStopped);
        }

        [UnityTest]
        public IEnumerator FastForwardDoesNotCompleteCondition()
        {
            // Given an used condition,
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            UsablePropertyMock mockedProperty = obj.AddComponent<UsablePropertyMock>();

            bool wasUsageStarted = false;
            bool wasUsageStopped = false;

            mockedProperty.UsageStarted += (sender, args) =>
            {
                wasUsageStarted = true;
                mockedProperty.UsageStopped += (o, eventArgs) => wasUsageStopped = true;
            };

            UsedCondition condition = new UsedCondition(mockedProperty);

            // When you activate it,
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When you fast-forward it
            condition.LifeCycle.MarkToFastForward();

            // Then nothing happens.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsFalse(condition.IsCompleted);
            Assert.IsFalse(wasUsageStarted);
            Assert.IsFalse(wasUsageStopped);
        }
    }
}
