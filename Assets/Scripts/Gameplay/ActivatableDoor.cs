using UnityEngine;
using System.Collections;

public class ActivatableDoor: MonoBehaviour
{
	public enum DoorState
	{
		Closed, Closing, Opened, Opening
	}
    public Vector3 DeltaPosition = Vector3.zero;

	[Header("Door Settings")]
	[Tooltip("Скорость активации")]
	public float ActivationSpeed = 2f;
	[Tooltip("Скорость деактивации")]
    public float DeactivationSpeed = 2f;
	[Tooltip("Изначальный угол")]
	public float InitialAngle = 0f;
	[Tooltip("Искомый угол")]
	public float TargetAngle = 90f;

	private float _transition = 0f;
	private DoorState _state = DoorState.Closed;
	private ActivatorProxy _activator;

	private Quaternion _initialRotation;
	private Quaternion _targetRotation;

	[Header("Hinge settings")]
	[Tooltip("Центр оси вращения")]
	public Vector3 HingePoint;
	[Tooltip("Направление оси вращения")]
	public Vector3 HingeDirection = Vector3.up;

    void Start ()
    {
        _activator = GetComponent<ActivatorProxy>();

		_initialRotation = transform.rotation * Quaternion.AngleAxis(InitialAngle, HingeDirection);
		_targetRotation = transform.rotation * Quaternion.AngleAxis(TargetAngle, HingeDirection);
    }

	void Update ()
    {
		if (_state == DoorState.Opening)
		{
			
			transform.RotateAround(transform.TransformPoint(HingePoint), 
				transform.TransformDirection(HingeDirection), -ActivationSpeed);
			//transform.rotation = Quaternion.Lerp(_initialRotation, _targetRotation, _transition);
			_transition += Time.deltaTime;

			if (_transition >= 1f)
			{
				_state = DoorState.Opened;
				_transition = 0f;
			}
		}

		if (_state == DoorState.Closing)
		{
			transform.RotateAround(transform.TransformPoint(HingePoint), 
				transform.TransformDirection(HingeDirection), ActivationSpeed);
			//transform.rotation = Quaternion.Lerp(_targetRotation, _initialRotation, _transition);
			_transition += Time.deltaTime;

			if (_transition >= 1f)
			{
				_state = DoorState.Closed;
				_transition = 0f;
			}
		}
	}

	void OnActivate()
	{
		Debug.LogWarning("Activating door");
		if (_state == DoorState.Closed) 
		{
			_state = DoorState.Opening;
			_transition = 0f;
			Debug.LogWarning("Opening");
		}

		if (_state == DoorState.Opened)
		{
			_state = DoorState.Closing;
			_transition = 0f;
			Debug.LogWarning("Closing");
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawLine(transform.TransformPoint(HingePoint),
			transform.TransformPoint(HingePoint) + transform.TransformDirection(HingeDirection));
		Gizmos.DrawWireCube(transform.TransformPoint(HingePoint), Vector3.one * 0.2f); 
	}
		
}
