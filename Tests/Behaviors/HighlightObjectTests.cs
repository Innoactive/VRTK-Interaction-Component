using System.Collections;
using UnityEngine.TestTools;
using UnityEngine;
using NUnit.Framework;
using VRTK.Highlighters;
using System.Collections.Generic;
using Innoactive.Creator.Core;
using Innoactive.Creator.Core.Behaviors;
using Innoactive.Creator.Core.Configuration;
using Innoactive.Creator.Core.Configuration.Modes;
using Innoactive.Creator.Tests.Utils;
using Innoactive.Creator.VRTKInteraction.Properties;
using Object = UnityEngine.Object;

namespace Innoactive.Creator.VRTKInteraction.Tests.Behaviors
{
    public class HighlightObjectTests : RuntimeTests
    {
        private class DummyHighlighter : VRTK_BaseHighlighter
        {
            public bool IsHighlighted { get; private set; }
            public Color? InitColor { get; private set; }
            public Color? HighlightColor { get; private set; }

            public override void Initialise(Color? color = null, GameObject affectObject = null, Dictionary<string, object> options = null)
            {
                InitColor = color;
            }

            public override void ResetHighlighter()
            {
            }

            public override void Highlight(Color? color = null, float duration = 0)
            {
                HighlightColor = color ?? InitColor;
                IsHighlighted = true;
            }

            public override void Unhighlight(Color? color = null, float duration = 0)
            {
                HighlightColor = null;
                IsHighlighted = false;
            }
        }

        private const string targetName = "TestReference";

        [UnityTest]
        public IEnumerator CreateWithoutVrtkBaseHighlighter()
        {
            // Given a VRTKObjectHighlight behavior with a game object without a highlighter,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject);

            // When we activate it,
            highlight.LifeCycle.Activate();

            // Then it has the VRTK_BaseHighlighter and VRTK_MaterialColorSwapHighlighter components.
            Assert.AreEqual(1, gameObject.GetComponents<VRTK_BaseHighlighter>().Length);
            Assert.IsNotNull(gameObject.GetComponent<VRTK_MaterialColorSwapHighlighter>());

            // Cleanup.
            Object.DestroyImmediate(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateWithVrtkBaseHighlighter()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object having a DummyHighlighter as component,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.AddComponent<DummyHighlighter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject);

            // When we activate it,
            highlight.LifeCycle.Activate();

            // Then it has the VRTK_BaseHighlighter component with only the DummyHighlighter as child.
            Assert.AreEqual(1, gameObject.GetComponents<VRTK_BaseHighlighter>().Length);
            Assert.IsNotNull(gameObject.GetComponent<DummyHighlighter>());

            yield break;
        }

        [UnityTest]
        public IEnumerator StepWithHighlightNonInteractable()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object and a linear chapter,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DummyHighlighter highlighter = gameObject.AddComponent<DummyHighlighter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            Color highlightColor = Color.yellow;

            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject, highlightColor);

            TestLinearChapterBuilder builder = TestLinearChapterBuilder.SetupChapterBuilder(1, true);
            Chapter chapter = builder.Build();
            builder.Steps[0].Data.Behaviors.Data.Behaviors.Add(highlight);
            chapter.Configure(RuntimeConfigurator.Configuration.GetCurrentMode());

            // When we activate the chapter,
            chapter.LifeCycle.Activate();

            while (highlight.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Stage highlightStageInStep = highlight.LifeCycle.Stage;
            bool objectHighlightedActiveInStep = highlighter.IsHighlighted;
            Color? colorInStep = highlighter.HighlightColor;

            chapter.Data.FirstStep.LifeCycle.Deactivate();

            while (chapter.Data.FirstStep.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                chapter.Update();
            }

            Stage highlightStageAfterStep = highlight.LifeCycle.Stage;
            bool objectHighlightedActiveAfterStep = highlighter.IsHighlighted;
            Color? colorAfterStep = highlighter.HighlightColor;

            // Then the highlight behavior is active during the step and inactive after it.
            Assert.AreEqual(Stage.Active, highlightStageInStep, "Highlight behavior should be active during step");
            Assert.IsTrue(objectHighlightedActiveInStep, "VRTK Highlighter should be active during step");
            Assert.AreEqual(highlightColor, colorInStep, "Highlight color should be " + highlightColor.ToString());

            Assert.AreEqual(Stage.Inactive, highlightStageAfterStep, "Highlight behavior should be deactivated after step");
            Assert.IsFalse(objectHighlightedActiveAfterStep, "VRTK Highlighter should be inactive after step");
            Assert.IsNull(colorAfterStep, "Hightlight color should be null after deactivation of step.");
        }

        [UnityTest]
        public IEnumerator StepWithHighlightInteractable()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object and a linear chapter,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DummyHighlighter highlighter = gameObject.AddComponent<DummyHighlighter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            Color highlightColor = Color.yellow;

            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject, highlightColor);

            TestLinearChapterBuilder builder = TestLinearChapterBuilder.SetupChapterBuilder(1, true);
            Chapter chapter = builder.Build();
            builder.Steps[0].Data.Behaviors.Data.Behaviors.Add(highlight);
            chapter.Configure(RuntimeConfigurator.Configuration.GetCurrentMode());

            // When we activate the chapter,
            chapter.LifeCycle.Activate();

            while (highlight.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Stage highlightStageInStep = highlight.LifeCycle.Stage;
            bool objectHighlightedActiveInStep = highlighter.IsHighlighted;
            Color? colorInStep = highlighter.HighlightColor;

            chapter.Data.FirstStep.LifeCycle.Deactivate();

            while (chapter.Data.FirstStep.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                chapter.Update();
            }

            Stage highlightStageAfterStep = highlight.LifeCycle.Stage;
            bool objectHighlightedActiveAfterStep = highlighter.IsHighlighted;
            Color? colorAfterStep = highlighter.HighlightColor;

            // Then the highlight behavior is active during the step and inactive after it.
            Assert.AreEqual(Stage.Active, highlightStageInStep, "Highlight behavior should be active during step");
            Assert.IsTrue(objectHighlightedActiveInStep, "VRTK Highlighter should be active during step");
            Assert.AreEqual(highlightColor, colorInStep, "Highlight color should be " + highlightColor.ToString());

            Assert.AreEqual(Stage.Inactive, highlightStageAfterStep, "Highlight behavior should be deactivated after step");
            Assert.IsFalse(objectHighlightedActiveAfterStep, "VRTK Highlighter should be inactive after step");
            Assert.IsNull(colorAfterStep, "Hightlight color should be null after deactivation of step.");
        }

        [UnityTest]
        public IEnumerator ColorChangeNonInteractable()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object and a color,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DummyHighlighter highlighter = gameObject.AddComponent<DummyHighlighter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            Color highlightColor = Color.cyan;

            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject, highlightColor);

            TestLinearChapterBuilder builder = TestLinearChapterBuilder.SetupChapterBuilder(1, true);
            Chapter chapter = builder.Build();
            builder.Steps[0].Data.Behaviors.Data.Behaviors.Add(highlight);
            chapter.Configure(RuntimeConfigurator.Configuration.GetCurrentMode());

            // When we activate the chapter,
            chapter.LifeCycle.Activate();

            while (builder.Steps[0].LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            // Then the color is changed.
            Assert.AreEqual(highlightColor, highlight.Data.HighlightColor);
            Assert.AreEqual(highlightColor, highlighter.HighlightColor);
            Assert.AreEqual(highlightColor, ((HighlightProperty)highlight.Data.ObjectToHighlight).CurrentHighlightColor);
        }

        [UnityTest]
        public IEnumerator ColorChangeInteractable()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object and a color,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DummyHighlighter highlighter = gameObject.AddComponent<DummyHighlighter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);
            
            Color highlightColor = Color.cyan;

            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject, highlightColor);

            TestLinearChapterBuilder builder = TestLinearChapterBuilder.SetupChapterBuilder(1, true);
            Chapter chapter = builder.Build();
            builder.Steps[0].Data.Behaviors.Data.Behaviors.Add(highlight);
            chapter.Configure(RuntimeConfigurator.Configuration.GetCurrentMode());

            // When we activate the chapter,
            chapter.LifeCycle.Activate();

            while (builder.Steps[0].LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            // Then the color is changed.
            Assert.AreEqual(highlight.Data.HighlightColor, highlightColor);
            Assert.AreEqual(highlighter.HighlightColor, highlightColor);
            Assert.AreEqual(highlightColor, ((HighlightProperty)highlight.Data.ObjectToHighlight).CurrentHighlightColor);
        }

        [UnityTest]
        public IEnumerator HighlightColorIsSetByParameter()
        {
            // Given a HighlightProperty with a HighlightColor parameter set,
            DynamicRuntimeConfiguration testRuntimeConfiguration = new DynamicRuntimeConfiguration();

            Color highlightColor = Color.red;

            testRuntimeConfiguration.SetAvailableModes(new List<IMode>
            {
                new Mode("Test", new WhitelistTypeRule<IOptional>(), new Dictionary<string, object> {{"HighlightColor", highlightColor}}),
            });

            RuntimeConfigurator.Configuration = testRuntimeConfiguration;

            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DummyHighlighter highlighter =  gameObject.AddComponent<DummyHighlighter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior highlight = new HighlightObjectBehavior(targetObject, Color.cyan);
            highlight.Configure(testRuntimeConfiguration.GetCurrentMode());

            // When we activate it,
            highlight.LifeCycle.Activate();

            // Then the highlight color is changed.
            Assert.AreEqual(highlightColor, highlight.Data.HighlightColor);
            Assert.AreEqual(highlightColor, highlighter.HighlightColor);
            Assert.AreEqual(highlightColor, ((HighlightProperty)highlight.Data.ObjectToHighlight).CurrentHighlightColor);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.AddComponent<DummyHighlighter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior behavior = new HighlightObjectBehavior(targetObject, Color.cyan);

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.AddComponent<DummyHighlighter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior behavior = new HighlightObjectBehavior(targetObject, Color.red);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndDeactivateIt()
        {
            // Given a VRTKObjectHighlight behavior with a proper game object,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.AddComponent<DummyHighlighter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior behavior = new HighlightObjectBehavior(targetObject, Color.red);

            // When we mark it to fast-forward, activate, and deactivate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();
            behavior.LifeCycle.Deactivate();

            // Then the behavior should be deactivated immediately.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given an active VRTKObjectHighlight behavior with a proper game object,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.AddComponent<DummyHighlighter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior behavior = new HighlightObjectBehavior(targetObject, Color.blue);

            behavior.LifeCycle.Activate();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardDeactivatingBehavior()
        {
            // Given a deactivating VRTKObjectHighlight behavior with a proper game object,
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.AddComponent<DummyHighlighter>();
            HighlightProperty targetObject = gameObject.AddComponent<HighlightProperty>();
            targetObject.SceneObject.ChangeUniqueName(targetName);

            HighlightObjectBehavior behavior = new HighlightObjectBehavior(targetObject, Color.blue);
            behavior.Configure(RuntimeConfigurator.Configuration.GetCurrentMode());

            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then the behavior should be deactivated immediately.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
        }
    }
}
