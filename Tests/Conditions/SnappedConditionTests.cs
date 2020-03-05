using VRTK;
using System.Collections;
using System.Collections.Generic;
using Innoactive.Hub.Training;
using Innoactive.Hub.Training.Conditions;
using Innoactive.Hub.Training.Configuration;
using Innoactive.Hub.Training.Configuration.Modes;
using Innoactive.Hub.Training.SceneObjects.Properties;
using Innoactive.Creator.Core.Tests.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Innoactive.Creator.Core.Tests.Conditions
{
    public class SnappedConditionTests : RuntimeTests
    {
        private class SnappablePropertyMock : SnappableProperty
        {
            public void SetSnapZone(SnapZoneProperty snapZone)
            {
                SnappedZone = snapZone;
            }

            public new void EmitSnapped(SnapZoneProperty snapZone)
            {
                snapZone.ForceSnap(gameObject);
                base.EmitSnapped(snapZone);
            }
        }

        [UnityTest]
        public IEnumerator HighlightNotShownAlways()
        {
            // Given a snappable property with an AlwaysShowSnapzoneHighlight parameter set to false.
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();

            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test", new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"AlwaysShowSnapzoneHighlight", false}}),
            });

            RuntimeConfigurator.Configuration = testRuntimeConfiguration;

            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            VRTK_SnapDropZone zone = snapZoneProperty.SnapZone;

            zone.highlightObjectPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mockedProperty.SetSnapZone(snapZoneProperty);
            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.Configure(RuntimeConfigurator.Configuration.GetCurrentMode());

            // When activated
            condition.LifeCycle.Activate();
            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Then the highlightAlwaysActive variable should be false
            Assert.IsFalse(snapZoneProperty.SnapZone.highlightAlwaysActive);
        }

        [UnityTest]
        public IEnumerator HighlightIsShownAlways()
        {
            // Given a snappable property with an AlwaysShowSnapzoneHighlight parameter set to true
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();

            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test",  new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"AlwaysShowSnapzoneHighlight", true}}),
            });

            RuntimeConfigurator.Configuration = testRuntimeConfiguration;

            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            VRTK_SnapDropZone zone = snapZoneProperty.SnapZone;

            zone.highlightObjectPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mockedProperty.SetSnapZone(snapZoneProperty);
            zone.InitaliseHighlightObject();
            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.Configure(RuntimeConfigurator.Configuration.GetCurrentMode());

            yield return null;
            condition.Update();

            // When activated
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Then the highlightAlwaysActive variable should be true
            Assert.IsTrue(snapZoneProperty.SnapZone.highlightAlwaysActive);
        }

        [UnityTest]
        public IEnumerator HighlightNotShown()
        {
            // Given a snappable property with a snapzone highlight deactivated parameter active.
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();

            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test", new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"ShowSnapzoneHighlight", false}}),
            });

            RuntimeConfigurator.Configuration = testRuntimeConfiguration;

            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            VRTK_SnapDropZone zone = snapZoneProperty.SnapZone;

            zone.highlightObjectPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zone.InitaliseHighlightObject();
            mockedProperty.SetSnapZone(snapZoneProperty);
            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.Configure(RuntimeConfigurator.Configuration.GetCurrentMode());
            yield return null;
            condition.Update();

            // When activated
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            //todo: add highlight again
            // Then the highlight is still inactive.
            Assert.IsFalse(snapZoneProperty.SnapZone.HighlightObject.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator HighlightColorCanBeChanged()
        {
            // Given a snappable property with a highlight color changed
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();

            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test",  new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"HighlightColor", Color.yellow}}),
            });

            RuntimeConfigurator.Configuration = testRuntimeConfiguration;

            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            VRTK_SnapDropZone zone = snapZoneProperty.SnapZone;

            zone.highlightObjectPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zone.InitaliseHighlightObject();
            mockedProperty.SetSnapZone(snapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.Configure(RuntimeConfigurator.Configuration.GetCurrentMode());
            yield return null;
            condition.Update();

            // When activated
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Then the highlight color changed properly
            Assert.AreEqual(Color.yellow, snapZoneProperty.SnapZone.highlightColor);
        }

        [UnityTest]
        public IEnumerator CompleteOnTargetedSnapZone()
        {
            // Setup object with mocked grabbed property and activate
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Emit grabbed event
            mockedProperty.SetSnapZone(snapZoneProperty);
            mockedProperty.EmitSnapped(snapZoneProperty);

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator DontCompleteOnWrongSnapZone()
        {
            // Setup object with mocked grabbed property and activate
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            SnapZoneProperty wrongSnapZoneProperty = CreateSnapZoneProperty();

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Emit grabbed event
            mockedProperty.SetSnapZone(wrongSnapZoneProperty);
            mockedProperty.EmitSnapped(wrongSnapZoneProperty);

            float startTime = Time.time;
            while (Time.time < startTime + 0.1f)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is not completed
            Assert.IsFalse(condition.IsCompleted, "Condition should not be completed!");
        }

        [UnityTest]
        public IEnumerator CompleteWhenSnappedOnStart()
        {
            // Setup object with mocked grabbed property and activate
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            mockedProperty.SetSnapZone(snapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.LifeCycle.Activate();

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator CompleteWhenSnappedOnActivationWithTargetSnapZone()
        {
            // Setup object with mocked grabbed property and activate
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            mockedProperty.SetSnapZone(snapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.LifeCycle.Activate();

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator DontCompleteWhenSnappedWrongOnActivation()
        {
            // Setup object with mocked grabbed property and activate
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();
            SnapZoneProperty wrongSnapZoneProperty = CreateSnapZoneProperty();
            mockedProperty.SetSnapZone(wrongSnapZoneProperty);

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);
            condition.LifeCycle.Activate();

            float startTime = Time.time;
            while (Time.time < startTime + 0.1f)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is not completed
            Assert.IsFalse(condition.IsCompleted, "SnappedCondition should not be complete!");
        }

        [UnityTest]
        public IEnumerator AutoCompleteActive()
        {
            // Given a snapped condition
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);

            // When you activate and autocomplete it,
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            condition.Autocomplete();

            // Then the condition is completed and the object is snapped.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsTrue(condition.IsCompleted);
            Assert.IsTrue(snapZoneProperty.IsObjectSnapped);
        }

        [UnityTest]
        public IEnumerator FastForwardDoesNotCompleteCondition()
        {
            // Given a snapped condition
            SnappablePropertyMock mockedProperty = CreateSnappablePropertyMock();
            SnapZoneProperty snapZoneProperty = CreateSnapZoneProperty();

            yield return null;

            SnappedCondition condition = new SnappedCondition(mockedProperty, snapZoneProperty);

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
            Assert.IsFalse(snapZoneProperty.IsObjectSnapped);
        }

        private SnappablePropertyMock CreateSnappablePropertyMock()
        {
            return new GameObject("Target").AddComponent<SnappablePropertyMock>();
        }

        private SnapZoneProperty CreateSnapZoneProperty()
        {
            return new GameObject("SnapZone Gus").AddComponent<SnapZoneProperty>();
        }
    }
}
