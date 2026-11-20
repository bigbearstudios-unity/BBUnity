using UnityEngine;
using NUnit.Framework;

using BBUnity.Support.Testing;

namespace BBUnity.Gameplay.Controllers.Tests {
    public class TimeControllerTests {

        private TimeController _controller;

        [SetUp]
        public void SetUp() {
            _controller = TestUtilities.CreateGameObject(name: "Time Controller").AddComponent<TimeController>();
        }

        [TearDown]
        public void TearDown() {
            TestUtilities.DestroyRootObjectsInScene();
            Time.timeScale = 1f;
        }

        [Test]
        public void RequestTimeScale_SlowsTimeImmediately() {
            _controller.RequestTimeScale(0.1f, 500, cumulative: false);

            Assert.AreEqual(0.1f, Time.timeScale, 0.0001f);
        }

        [Test]
        public void Advance_AfterDuration_ReturnsToNormalSpeed() {
            _controller.RequestTimeScale(0.1f, 500, cumulative: false);

            _controller.Advance(0.6f);

            Assert.AreEqual(1f, Time.timeScale, 0.0001f);
            Assert.AreEqual(0, _controller.ActiveModifierCount);
        }

        [Test]
        public void Advance_BeforeDuration_KeepsModifier() {
            _controller.RequestTimeScale(0.1f, 500, cumulative: false);

            _controller.Advance(0.2f);

            Assert.AreEqual(0.1f, Time.timeScale, 0.0001f);
        }

        [Test]
        public void RequestTimeScale_ZeroDuration_IsIgnored() {
            int id = _controller.RequestTimeScale(0.1f, 0, cumulative: false);

            Assert.AreEqual(-1, id);
            Assert.AreEqual(1f, Time.timeScale, 0.0001f);
        }

        [Test]
        public void Cumulative_Modifiers_Multiply() {
            _controller.RequestTimeScale(0.5f, 1000, cumulative: true);
            _controller.RequestTimeScale(0.5f, 1000, cumulative: true);

            Assert.AreEqual(0.25f, Time.timeScale, 0.0001f);
        }

        [Test]
        public void Cumulative_MultipliesWithBaseTimeScale() {
            _controller.BaseTimeScale = 2f;
            _controller.RequestTimeScale(0.5f, 1000, cumulative: true);

            Assert.AreEqual(1f, Time.timeScale, 0.0001f);
        }

        [Test]
        public void NonCumulative_OverridesBaseAndCumulativeModifiers() {
            _controller.BaseTimeScale = 2f;
            _controller.RequestTimeScale(0.5f, 1000, cumulative: true);
            _controller.RequestTimeScale(0.1f, 1000, cumulative: false);

            Assert.AreEqual(0.1f, Time.timeScale, 0.0001f);
        }

        [Test]
        public void NonCumulative_MostRecentWins_ThenEarlierTakesOver() {
            _controller.RequestTimeScale(0.5f, 1000, cumulative: false);
            _controller.RequestTimeScale(0.1f, 200, cumulative: false);
            Assert.AreEqual(0.1f, Time.timeScale, 0.0001f);

            _controller.Advance(0.3f);

            Assert.AreEqual(0.5f, Time.timeScale, 0.0001f);
        }

        [Test]
        public void NonCumulative_Expiring_RestoresCumulativeResult() {
            _controller.RequestTimeScale(0.5f, 1000, cumulative: true);
            _controller.RequestTimeScale(0.1f, 200, cumulative: false);

            _controller.Advance(0.3f);

            Assert.AreEqual(0.5f, Time.timeScale, 0.0001f);
        }

        [Test]
        public void Cancel_RemovesModifier() {
            int id = _controller.RequestTimeScale(0.1f, 1000, cumulative: false);

            Assert.IsTrue(_controller.Cancel(id));
            Assert.AreEqual(1f, Time.timeScale, 0.0001f);
            Assert.IsFalse(_controller.Cancel(id));
        }

        [Test]
        public void ResetToNormal_ClearsEverything() {
            _controller.BaseTimeScale = 3f;
            _controller.RequestTimeScale(0.1f, 5000, cumulative: false);
            _controller.RequestTimeScale(0.5f, 5000, cumulative: true);

            _controller.ResetToNormal();

            Assert.AreEqual(1f, Time.timeScale, 0.0001f);
            Assert.AreEqual(0, _controller.ActiveModifierCount);
        }

        [Test]
        public void Destroy_RestoresNormalSpeed() {
            _controller.RequestTimeScale(0.1f, 5000, cumulative: false);

            Object.DestroyImmediate(_controller.gameObject);

            Assert.AreEqual(1f, Time.timeScale, 0.0001f);
        }
    }
}
