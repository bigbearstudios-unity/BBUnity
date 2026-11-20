using NUnit.Framework;

using BBUnity.Gameplay.Entities.Controllers.Platforming.Internal;

namespace BBUnity.Gameplay.Entities.Controllers.Platforming.Tests {
    public class InputStateTests {

        private InputState _state;

        [SetUp]
        public void SetUp() {
            _state = new InputState();
        }

        [Test]
        public void SetJump_Pressed_RequestsJump() {
            _state.SetJump(jumpPressed: true, jumpHeld: true);

            Assert.IsTrue(_state.JumpRequested);
        }

        [Test]
        public void SetJump_NotPressedOnLaterFrames_DoesNotClearUnconsumedRequest() {
            // A press lands on a frame with no physics step; later Updates report "not pressed".
            _state.SetJump(jumpPressed: true, jumpHeld: true);
            _state.SetJump(jumpPressed: false, jumpHeld: true);
            _state.SetJump(jumpPressed: false, jumpHeld: true);

            Assert.IsTrue(_state.JumpRequested);
        }

        [Test]
        public void ConsumeJumpRequest_ClearsRequest() {
            _state.SetJump(jumpPressed: true, jumpHeld: true);

            _state.ConsumeJumpRequest();

            Assert.IsFalse(_state.JumpRequested);
        }

        [Test]
        public void SetJump_TracksHeldIndependentlyOfRequest() {
            _state.SetJump(jumpPressed: true, jumpHeld: true);
            Assert.IsTrue(_state.JumpHeld);

            _state.SetJump(jumpPressed: false, jumpHeld: false);

            Assert.IsFalse(_state.JumpHeld);
            Assert.IsTrue(_state.JumpRequested);
        }

        [Test]
        public void SetHorizontalMovement_Snaps_WhenRequested() {
            _state.SetHorizontalMovement(0.3f, snapMovement: true);

            Assert.AreEqual(1f, _state.HorizontalMovement);
        }
    }
}
