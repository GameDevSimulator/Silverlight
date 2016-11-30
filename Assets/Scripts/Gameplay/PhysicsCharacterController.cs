using Assets.Scripts.Game;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class PhysicsCharacterController : MonoBehaviour
    {
        private Rigidbody _rigidBody;
        private bool _jumpRequested = false;
        private Vector3 _dampVelocity;

        public float JumpForce = 200f;
        public float MoveForce = 100f;

        [Range(0f, 1f)]
        public float InAirControlModifier = 0.5f;
        public Vector3 MaxVelocity = new Vector3(2f, 10f, 2f);
        public float GroundDampingTime = 0.1f;
    
        public Vector3 GroundTesterOffset;
        public float GroundTesterRadius = 1f;

        public bool IsGrounded { get; private set; }

#if DEBUG
        public bool ShowDebugInfo = false;
#endif

        private bool _isDarknessForceApplied;

        void Start ()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }
	
        void FixedUpdate ()
        {
            IsGrounded = _isDarknessForceApplied;
            _isDarknessForceApplied = false;
            if (!IsGrounded)
            {
                var colliders = Physics.OverlapSphere(transform.position + GroundTesterOffset, GroundTesterRadius);
                foreach (var collider1 in colliders)
                {
                    if (collider1.gameObject != this.gameObject && !collider1.isTrigger)
                    {
                        IsGrounded = true;
                        break;
                    }
                }
            }

            if (IsGrounded)
            {
                if (_jumpRequested)
                {
                    Debug.Log("JUMP!");
                    _rigidBody.AddForce(0, JumpForce, 0);
                    IsGrounded = false;
                    _jumpRequested = false;
                }
            
                /*
            _rigidBody.velocity = Vector3.SmoothDamp(_rigidBody.velocity, Vector3.zero, ref _dampVelocity,
                GroundDampingTime);*/
            }

            // clamp velocity
            var velocity = _rigidBody.velocity;
            velocity.x = Mathf.Clamp(velocity.x, -MaxVelocity.x, MaxVelocity.x);
            velocity.y = Mathf.Clamp(velocity.y, -MaxVelocity.y, MaxVelocity.y);
            velocity.z = Mathf.Clamp(velocity.z, -MaxVelocity.z, MaxVelocity.z);

            _rigidBody.velocity = velocity;
        }

        public void Jump()
        {
            if(!_jumpRequested && IsGrounded)
                _jumpRequested = true;
        }

        public void Move(float horizonatalInput)
        {
            var xInput = Mathf.Clamp(horizonatalInput, -1f, 1f);

            if (!IsGrounded)
                xInput *= InAirControlModifier;

            _rigidBody.AddForce(Vector3.right * xInput * MoveForce * Time.deltaTime, ForceMode.Impulse);
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position + GroundTesterOffset, GroundTesterRadius);
        }

        void OnDarknessForceApplied(Darkness.Darkness darkness)
        {
            _isDarknessForceApplied = true;
        }

#if DEBUG
        void OnGUI()
        {
            if (ShowDebugInfo)
            {
                GUI.TextField(new Rect(0, 0, 200, 20), string.Format("input: {0}", Input.GetAxis("Horizontal")));
                GUI.TextField(new Rect(0, 25, 200, 20), string.Format("velocity: {0}", _rigidBody.velocity.ToString()));
                GUI.TextField(new Rect(0, 50, 200, 20), string.Format("velocity: {0}", _rigidBody.velocity.magnitude));
            }
        }
#endif
    }
}
