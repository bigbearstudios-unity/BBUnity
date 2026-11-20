using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;

using BBUnity.Gameplay.Entities.Controllers;
using BBUnity.Gameplay.Entities.Controllers.Platforming.Internal;

namespace BBUnity.Gameplay.Entities.Controllers.Platforming {

    [AddComponentMenu("BBUnity/Gameplay/Entities/Controllers/Platforming/Platforming Player Movement Controller")]
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public class PlatformingPlayerMovementController : PlayerMovementController {

        /*
         * Configuration
         */

        /* ---------------------------------------------*/
        [Header("Collision")]

        [SerializeField,Tooltip("")]
        private LayerMask _staticLayers;

        [SerializeField, Tooltip("")]
        private LayerMask _platformLayers;

        [SerializeField, Tooltip("")]
        private float _collisionDistance = 0.05f;

        /* ---------------------------------------------*/
        [Header("Horizontal Movement")]

        [SerializeField, Tooltip("")]
        [FormerlySerializedAs("_horizontalMaxmumSpeed")]
        private float _horizontalMaximumSpeed = 9.0f;

        [SerializeField, Tooltip("")]
        private float _horizontalAcceleration = 120.0f;

        [SerializeField, Tooltip("")]
        private float _horizontalGroundedDeceleration = 60.0f;

        [SerializeField, Tooltip("")]
        [FormerlySerializedAs("_horizontalAirborneDeleration")]
        private float _horizontalAirborneDeceleration = 30.0f;

        /* ---------------------------------------------*/
        [Header("Movement Modifiers")]

        [SerializeField, Tooltip("")]
        private bool _snapInputMovement = true;

        /* ---------------------------------------------*/
        [Header("Vertical Movement")]

        [SerializeField, Tooltip("")]
        private float _verticalGroundingForce = 0.05f;

        [SerializeField, Tooltip("The force applied in order to 'jump'")]
        private float _verticalJumpForce = 25.0f;

        [SerializeField, Tooltip("")]
        private float _verticalMaximumFallSpeed = 40.0f;

        [SerializeField, Tooltip("")]
        private float _verticalFallAcceleration = 110.0f;

        /* ---------------------------------------------*/
        [Header("Jump Modifiers")]

        [SerializeField, Tooltip("Gravity multiplier applied while rising once the jump button has been released, so a short tap gives a short hop. 1 disables variable jump height; higher values cut the jump off harder.")]
        private float _earlyJumpReleaseGravityModifier = 3;

        // True from the moment a jump leaves the ground until its ascent ends (apex or landing)
        // — only that kind of upward movement can be cut short by releasing the button.
        private bool _isJumpAscent = false;
        private bool _earlyJumpReleaseActive = false;

        [SerializeField, Tooltip("Grace period after walking off a ledge during which a jump input is still honored.")]
        private CoyoteTime _coyoteTime = new CoyoteTime();

        private bool _justJumped = false;

        /* ---------------------------------------------*/
        [Header("Slope Handling")]

        [SerializeField, Tooltip("Extra downward cast distance when already grounded and moving horizontally, to keep contact over downhill slope edges")]
        private float _slopeStickDistance = 0.3f;

        /*
         * Required Unity Components
         * These are required using the attribute RequiredComponents and are needed for the function
         * of this controller.
         */

        private Rigidbody2D _rigidbody;
        private CapsuleCollider2D _capsuleCollider;

        /*
         * Internal State Management
         * This exposes the internal state of the collection / 
         */

        [SerializeField]
        private MovementState _state = new();
        private InputState _inputState = new();

        public MovementState State {
            get { return _state; }
        }

        public override Vector2 Velocity {
            get { return _velocity; }
        }

        /*
         * Motion numbers other systems plan around (for example level generation, which uses them
         * to decide what a player can jump to). All in world units per second.
         */

        /// <summary>The upward speed a jump starts with.</summary>
        public float JumpSpeed {
            get { return _verticalJumpForce; }
        }

        /// <summary>The downward acceleration while airborne, used while rising as well as falling.</summary>
        public float AirborneGravity {
            get { return _verticalFallAcceleration; }
        }

        public float MaximumFallSpeed {
            get { return _verticalMaximumFallSpeed; }
        }

        public float MaximumHorizontalSpeed {
            get { return _horizontalMaximumSpeed; }
        }

        /*
         * Computed Variables
         */
        [SerializeField]
        private Vector2 _velocity;

        private Vector2 _groundNormal = Vector2.up;

        private bool _horizontalVelocityOverrideActive = false;
        private float _horizontalVelocityOverrideValue = 0f;

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody2D>() ?? throw new System.Exception("A 'Rigidbody' component is required");
            _capsuleCollider = GetComponent<CapsuleCollider2D>() ?? throw new System.Exception("A 'CapsuleCollider2D' component is required");
        }

        // This method is going to be pretty bad in terms of size, we might need to consider changing its
        // call signature
        //
        // jumpPressed is the one-frame "button went down" event (it's latched until a physics
        // step consumes it); jumpHeld is the continuous "button is down" state.
        public void ApplyMovement(
            float horizontalMovement,
            bool jumpPressed = false,
            bool jumpHeld = false
            ) {
            _inputState.SetHorizontalMovement(horizontalMovement, _snapInputMovement);
            _inputState.SetJump(jumpPressed, jumpHeld);
        }

        /// <summary>
        /// Bypasses normal acceleration and deceleration to apply an exact horizontal velocity.
        /// Call ClearHorizontalVelocityOverride() to return to normal movement.
        /// </summary>
        public void SetHorizontalVelocityOverride(float velocity) {
            _horizontalVelocityOverrideActive = true;
            _horizontalVelocityOverrideValue = velocity;
        }

        public void ClearHorizontalVelocityOverride() {
            _horizontalVelocityOverrideActive = false;
            _horizontalVelocityOverrideValue = 0f;
        }

        public void TogglePlatformCollision(int toggleBackAfter = 400) {
            if (_togglingPlatformCollisions) { return; }

            StartCoroutine(TogglePlatformCollisionsOver(toggleBackAfter));
        }

        private bool _togglingPlatformCollisions = false;
        private readonly List<Collider2D> _ignoredPlatformColliders = new List<Collider2D>();

        // Ignores collision only against the specific platform colliders the player is
        // currently overlapping, rather than the whole layer, and always restores them via
        // finally — even if the coroutine is stopped early.
        private IEnumerator TogglePlatformCollisionsOver(int waitTime) {
            _togglingPlatformCollisions = true;

            try {
                foreach (Collider2D platformCollider in Physics2D.OverlapCapsuleAll(_capsuleCollider.bounds.center, _capsuleCollider.size, _capsuleCollider.direction, 0, _platformLayers)) {
                    Physics2D.IgnoreCollision(_capsuleCollider, platformCollider, true);
                    _ignoredPlatformColliders.Add(platformCollider);
                }

                yield return new WaitForSeconds(waitTime / 1000f);
            } finally {
                foreach (Collider2D platformCollider in _ignoredPlatformColliders) {
                    if (platformCollider != null) {
                        Physics2D.IgnoreCollision(_capsuleCollider, platformCollider, false);
                    }
                }

                _ignoredPlatformColliders.Clear();
                _togglingPlatformCollisions = false;
            }
        }

        /// <summary>
        /// Moves the player to `position` instantly and kills all of its momentum — for spawning
        /// at a PlayerSpawnNode or crossing a scene transition. Plain transform.position would be
        /// overwritten by the next FixedUpdate's velocity and keep the old velocity, so a fall
        /// started before the move would carry on afterwards.
        /// </summary>
        public override void Teleport(Vector2 position) {
            transform.position = new Vector3(position.x, position.y, transform.position.z);

            _rigidbody.position = position;
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0.0f;

            _velocity = Vector2.zero;
            _state.SetVelocity(_velocity);
            ClearHorizontalVelocityOverride();

            Physics2D.SyncTransforms();
        }

        public void ApplyZeroHorizontalMovement() {
            _inputState.SetHorizontalMovement(0.0f, false);
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = 0.0f;
        }

        private void FixedUpdate() {
            GatherCollisionState();
            ApplyVerticalMovement();
            ApplyHorizontalMovement();
            ApplyGravity();
            ApplySlopeMovement();

            _rigidbody.linearVelocity = _velocity;

            _state.SetVelocity(_velocity);
        }

        private void GatherCollisionState() {
            bool previousQueriesStartInColliders = Physics2D.queriesStartInColliders;

            Physics2D.queriesStartInColliders = false;

            // Extend the downward check when already grounded and moving horizontally so the
            // character doesn't briefly leave ground when the floor drops away on a downhill slope.
            float groundCheckDist = _collisionDistance;
            // Not while rising: right after a jump the player is still within this longer range of
            // the floor, which would keep them "grounded" for a few frames after takeoff — and
            // on a slope, ApplySlopeMovement would then flatten the jump out of the velocity.
            if (_state.IsGrounded && Mathf.Abs(_velocity.x) > float.Epsilon && _velocity.y <= _verticalGroundingForce + float.Epsilon) {
                groundCheckDist += _slopeStickDistance;
            }

            RaycastHit2D groundStaticHit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, _capsuleCollider.direction, 0, Vector2.down, groundCheckDist, _staticLayers);
            RaycastHit2D groundPlatformHit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, _capsuleCollider.direction, 0, Vector2.down, groundCheckDist, _platformLayers);
            bool ceilingStaticHit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, _capsuleCollider.direction, 0, Vector2.up, _collisionDistance, _staticLayers);

            _state.SetCollisionState(up: ceilingStaticHit, down: groundStaticHit || groundPlatformHit);

            // Store the surface normal so ApplySlopeMovement can project velocity along slopes.
            if (groundStaticHit) {
                _groundNormal = groundStaticHit.normal;
            } else if (groundPlatformHit) {
                _groundNormal = groundPlatformHit.normal;
            } else {
                _groundNormal = Vector2.up;
            }

            if (_state.IsHittingHead) {
                _velocity.y = Mathf.Min(0, _velocity.y);
            }

            if (_state.IsGrounded && !_justJumped) {
                _isJumpAscent = false;
                _earlyJumpReleaseActive = false;
                _coyoteTime.Reset();
            } else if (_state.HasBecomeAirborne) {
                // Only start the grace window when the ground disappeared out from under us
                // (walking off a ledge). A voluntary jump already leaves the ground on purpose,
                // so it must not also grant a coyote-time jump.
                if (!_justJumped) {
                    _coyoteTime.SetAvailable(true);
                    _coyoteTime.StartTimer();
                }
            }

            _justJumped = false;

            Physics2D.queriesStartInColliders = previousQueriesStartInColliders;
        }

        private void ApplyVerticalMovement() {
            if (_inputState.JumpRequested) {
                if (_state.IsGrounded || _coyoteTime.IsAvailable) {
                    ApplyJumpForce();
                }

                // Acted on or not, this press is spent — a request that couldn't be honored
                // shouldn't fire later, on a landing the player never asked to jump from.
                _inputState.ConsumeJumpRequest();
            }
        }

        private void ApplyJumpForce() {
            _velocity.y = _verticalJumpForce;
            _coyoteTime.Reset();
            _justJumped = true;
            _isJumpAscent = true;
            _earlyJumpReleaseActive = false;
        }

        private void ApplyHorizontalMovement() {
            if (_horizontalVelocityOverrideActive) {
                _velocity.x = _horizontalVelocityOverrideValue;
                return;
            }

            if (_inputState.HasHorizontalMovement) {
                _velocity.x = Mathf.MoveTowards(_velocity.x, _inputState.HorizontalMovement * _horizontalMaximumSpeed, _horizontalAcceleration * Time.fixedDeltaTime);
            } else {
                var deceleration = _state.IsGrounded ? _horizontalGroundedDeceleration : _horizontalAirborneDeceleration;
                _velocity.x = Mathf.MoveTowards(_velocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
        }

        private void ApplySlopeMovement() {
            // Skip when airborne, on flat ground, or jumping this frame, (jumping must preserve the full vertical jump force, not the slope's y component).
            if (!_state.IsGrounded || _groundNormal == Vector2.up || _justJumped) {
                return;
            }

            if (Mathf.Abs(_velocity.x) < float.Epsilon) {
                _velocity.x = 0f;
                _velocity.y = -Physics2D.gravity.y * _rigidbody.gravityScale * Time.fixedDeltaTime;
                return;
            }

            // slopeRight is the unit vector pointing along the slope surface in the +x direction.
            // For flat ground (normal = (0,1)) this equals (1,0), so projection is a no-op.
            // For a slope with normal = (-sinθ, cosθ): slopeRight = (cosθ, sinθ) — correctly inclined.
            Vector2 slopeRight = new Vector2(_groundNormal.y, -_groundNormal.x);
            float speed = Mathf.Abs(_velocity.x);
            int direction = _velocity.x > 0 ? 1 : -1;
            _velocity = slopeRight * (speed * direction);
        }

        private void ApplyGravity() {
            if (_state.IsGrounded && _velocity.y <= 0.0f) {
                _velocity.y = _verticalGroundingForce;
            } else {
                var inAirGravity = _verticalFallAcceleration;

                if (_velocity.y <= 0) {
                    // Ascent is over (apex reached) — nothing left to cut short.
                    _isJumpAscent = false;
                    _earlyJumpReleaseActive = false;
                } else if (_isJumpAscent && !_inputState.JumpHeld) {
                    // Latched, so re-pressing mid-air doesn't hand the height back.
                    _earlyJumpReleaseActive = true;
                }

                if (_earlyJumpReleaseActive) {
                    inAirGravity *= _earlyJumpReleaseGravityModifier;
                }

                _velocity.y = Mathf.MoveTowards(_velocity.y, -_verticalMaximumFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }
    }
}