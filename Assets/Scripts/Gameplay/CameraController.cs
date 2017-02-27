using Assets.Scripts;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(0f, 100f)] public float ZFlyAwayCoeff = 5f;
    [Range(0f, 100f)] public float LookAheadCoeff = 5f;
    public float MaxFlyaway = 10f;

    private Vector3 _offset;
    private Vector3 _target;

    private Vector3 _lastTargetPosition;

    void Start ()
	{
        if(GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Boy)
        {
            _offset = transform.position - GameManager.Instance.Player.transform.position; //расстояние между камерой и игроком
        }

        if(GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Cat) {
            _offset = transform.position - GameManager.Instance.Cat.transform.position; //расстояние между камерой и котом
        }
	}

	void Update ()
	{
        if (GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Boy)
            _target = GameManager.Instance.Player.transform.position;
        if(GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Cat)
            _target = GameManager.Instance.Cat.transform.position;


	    var targetVelocity = _target - _lastTargetPosition;
	    var flyaway = Vector3.ClampMagnitude(Vector3.back*(targetVelocity.magnitude*ZFlyAwayCoeff), MaxFlyaway);

        transform.position = Vector3.Lerp(transform.position, _target + _offset + flyaway + targetVelocity * LookAheadCoeff, Time.deltaTime);
	    _lastTargetPosition = _target;
	}
}
