using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PhysicsCharacterController : MonoBehaviour
{
    private Rigidbody _rigidBody;
    private Collider _collider;
    private float _distToGround;
    private bool _jumpRequested = false;
    private Vector3 _dampVelocity;

    public float JumpForce = 200f;
    public float MoveForce = 100f;
    public float InAirControlCoeff = 0.5f;
    public Vector3 MaxVelocity = new Vector3(2f, 10f, 2f);
    public float GroundDampingTime = 0.1f;

    public bool IsGrounded { get; private set; }
    

	void Start ()
	{
	    _rigidBody = GetComponent<Rigidbody>();
	    _collider = GetComponent<Collider>();
	    _distToGround = _collider.bounds.extents.y;
    }
	
	void FixedUpdate ()
	{
	    RaycastHit hit;
	    var result = Physics.Raycast(_collider.bounds.center, Vector3.down, out hit, _distToGround + 0.01f);
	    IsGrounded = result && hit.collider != null && !hit.collider.isTrigger;

	    if (IsGrounded)
	    {
	        if (_jumpRequested)
	        {
	            Debug.Log("JUMP!");
	            _rigidBody.AddForce(0, JumpForce, 0);
	            IsGrounded = false;
	            _jumpRequested = false;
	        }
            
	        _rigidBody.velocity = Vector3.SmoothDamp(_rigidBody.velocity, Vector3.zero, ref _dampVelocity,
                GroundDampingTime);
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

    public void Move(Vector3 direction)
    {
        direction.Normalize();

        if (!IsGrounded)
            direction *= InAirControlCoeff;

        _rigidBody.AddForce(direction * MoveForce);
    }
}
