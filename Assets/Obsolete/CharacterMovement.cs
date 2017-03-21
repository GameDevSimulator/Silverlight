using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMovement : MonoBehaviour
    {
        enum MovementState
        {
            NotControllable,
            Controllable,
            Climbing,
        }

        public bool IsControllable
        {
            get { return _state == MovementState.Climbing || _state == MovementState.Controllable; }
        }

        private CharacterController _controller;
        private MovementState _state;
        private Vector3 _verticalMovement = Vector3.zero;
        private Vector3 _horizontalMovement = Vector3.zero;

        [Header("Movement")]
        [Range(0f, 10f)]
        public float BaseMovementSpeed = 2f;
        public float AccelerationTime = 2f;
        public float MaxMovementSpeed = 3f;

        [Range(0f, 1f)]
        public float AirControl = 0.8f;
        public float GravityModifier = 0.02f;

        private float _timeAtMaxSpeed = 0;

        /// <summary>
        /// JUMPING SECTION
        /// </summary>
        [Header("Jumping")]
        [Range(0f, 10f)]
        public float BaseJumpSpeed = 1f;

        public AnimationCurve JumpSpeedOverTime = AnimationCurve.EaseInOut(0, 1f, 1f, 0);
        public float GhostJumpTime = 0.1f;
        [Range(0, 4)]
        public int InAirJumps = 1;

        private int _inAirJumpsRemaining = 0;
        private bool _jumpRequestedLastFrame = false;
        private bool _isJumping = false;
        private float _jumpTime = 0f;
        private float _timeOffGround = 0f;

        /// <summary>
        /// CLIMBING SECTION
        /// </summary>
        [Header("Climbing")]
        [Range(0, 5f)]
        public float ClimbingSpeed;

        private Collider _climbLadder = null;

        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _state = MovementState.NotControllable;
        }

        void Update()
        {
            var isJumpRequested = false;
            var inputMovement = Vector3.zero;

            if (_state == MovementState.Controllable)
            {
                isJumpRequested = Input.GetButton(WellKnown.Buttons.Jump);
                inputMovement.x = Input.GetAxis(WellKnown.Axis.Horizontal);
                inputMovement.z = Input.GetAxis(WellKnown.Axis.Vertical);
            }

            if (_state != MovementState.Climbing)
            {
                if (isJumpRequested)
                {
                    if (!_jumpRequestedLastFrame)
                    {
                        var isGroundedOrGhostAvailable = _controller.isGrounded || _timeOffGround < GhostJumpTime;
                        if (isGroundedOrGhostAvailable || _inAirJumpsRemaining > 0)
                        {
                            _verticalMovement = new Vector3(0f, BaseJumpSpeed, 0f);

                            if (!isGroundedOrGhostAvailable)
                                _inAirJumpsRemaining--;

                            _isJumping = true;
                            _jumpTime = 0f;
                        }
                    }

                    if (_isJumping)
                    {
                        _jumpTime += Time.deltaTime;
                        _verticalMovement += new Vector3(0f, JumpSpeedOverTime.Evaluate(_jumpTime), 0f);
                    }
                }
                else
                {
                    _isJumping = false;
                }


                if (_controller.isGrounded)
                {
                    //_isJumping = false;
                    _inAirJumpsRemaining = InAirJumps;
                    _horizontalMovement = inputMovement;
                    _timeOffGround = 0f;
                }
                else
                {
                    _horizontalMovement += inputMovement * AirControl;
                    _horizontalMovement = Vector3.ClampMagnitude(_horizontalMovement, 1f);
                    _verticalMovement += Physics.gravity * GravityModifier;
                    _timeOffGround += Time.deltaTime;
                }

                var horiz = Vector3.ClampMagnitude(_horizontalMovement, 1f) * BaseMovementSpeed;

                if (horiz.magnitude - BaseMovementSpeed > -0.1f)
                {
                    _timeAtMaxSpeed = Mathf.Clamp(_timeAtMaxSpeed + Time.deltaTime, 0, AccelerationTime);
                    horiz += horiz.normalized * ((MaxMovementSpeed - BaseMovementSpeed) * _timeAtMaxSpeed / AccelerationTime);
                }
                else
                {
                    _timeAtMaxSpeed = 0f;
                }

                _controller.Move((_verticalMovement + horiz) * Time.deltaTime);
                _jumpRequestedLastFrame = isJumpRequested;
            }

            if (_state == MovementState.Climbing)
            {
                if (_climbLadder != null)
                {
                    var rel = _climbLadder.transform.InverseTransformVector(inputMovement);
                    var vertMovement = rel.y + rel.z + inputMovement.z;
                    if (vertMovement < 0 && _isJumping)
                    {
                        _state = MovementState.Controllable;
                    }
                    else
                    {
                        _controller.Move(new Vector3(0, ClimbingSpeed * vertMovement * Time.deltaTime, 0));
                    }

                    if (isJumpRequested)
                    {
                        _state = MovementState.Controllable;
                    }
                }
                else
                {
                    _state = MovementState.Controllable;
                }
            }
        }
        
        void OnTriggerEnter(Collider coll)
        {
            if (coll.CompareTag(WellKnown.Tags.Ladder) && _climbLadder == null)
            {
                _climbLadder = coll;
                _state = MovementState.Climbing;
            }
        }

        void OnTriggerExit(Collider coll)
        {
            if (coll.CompareTag(WellKnown.Tags.Ladder) && _climbLadder != null)
            {
                _climbLadder = null;
            }
        }

        public void Deactivate()
        {
            _state = MovementState.NotControllable;
        }

        public void Activate()
        {
            _state = MovementState.Controllable;
        }
    }
}
