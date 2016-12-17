using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ActivatorProxy))]
public class ActivatableTransform : MonoBehaviour
{
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
	private float ActualAngle = 0f;

    private float _state = 0f;
	private ActivatorProxy _activator;
//    private Vector3 _initialPosition;
//    private Vector3 _targedPosition;
	//[Tooltip("Настройки оси вращения")]
	[Header("Hinge settings")]
	[Tooltip("Центр оси вращения")]
	public Vector3 HingePoint;
	[Tooltip("Направление оси вращения")]
	public Vector3 HingeDirection = Vector3.up;

    void Awake()
    {
     //   _initialPosition = transform.position;
      //  _targedPosition = _initialPosition + DeltaPosition;
    }

    void Start ()
    {
        _activator = GetComponent<ActivatorProxy>();
    }
	
	void Update ()
    {
		if (_activator.IsActivated && ActualAngle < TargetAngle)
	    {
			transform.RotateAround(transform.TransformPoint(HingePoint), transform.TransformDirection(HingeDirection), ActivationSpeed);
			Debug.LogWarning ("ActivatedOpen");
			ActualAngle+=ActivationSpeed;
		}

		/*if (_activator.IsActivated && ActualAngle >= TargetAngle)
		{
			transform.RotateAround(transform.TransformPoint(HingePoint), transform.TransformDirection(HingeDirection), ActivationSpeed);
			Debug.LogWarning ("ActivatedClose");
			ActualAngle-=ActivationSpeed;}*/
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawLine(transform.TransformPoint(HingePoint),
			transform.TransformPoint(HingePoint) + transform.TransformDirection(HingeDirection));
		Gizmos.DrawWireCube(transform.TransformPoint(HingePoint), Vector3.one * 0.2f); 
	}
}
