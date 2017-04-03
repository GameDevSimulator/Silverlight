using Assets.Scripts;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(0f, 100f)] public float ZFlyAwayCoeff = 5f;
    [Range(0f, 100f)] public float LookAheadCoeff = 5f;
    public float MaxFlyaway = 10f;

    private Vector3 _offset;
    private Vector3 _target;

    private bool _isEvent;
    private float _z0;
    private float _zoomEvent = -3;

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
        if (_isEvent)
        {            
            if (transform.position.z < _zoomEvent)
            {
                //  transform.position += new Vector3(0, 0, (float)0.5);
                transform.position = Vector3.Lerp(transform.position, _target, Time.deltaTime);
            }
            else
            {
                if (GameManager.Instance.Cat._inBag)
                {
                    //animation of jumping of the bag cat playing
                    GameManager.Instance.Cat._inBag = false;
                    GameManager.Instance.Cat.gameObject.SetActive(true);
                    GameManager.Instance.Cat.transform.position = new Vector3(GameManager.Instance.Player.transform.position.x-(float)0.4, GameManager.Instance.Cat.transform.position.y,0);
                }
                else
                {
                    //animation of jumping to the bag cat playing
                    GameManager.Instance.Cat._inBag = true;
                    GameManager.Instance.Cat.gameObject.SetActive(false);
                   
                }

                _isEvent = false;
            }
        }
        else
        {
            if (Input.GetButtonDown(WellKnown.Buttons.CallCat) && GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Boy)
            {
               // _z0 = transform.position.z;
               // transform.position = new Vector3(GameManager.Instance.Player.transform.position.x, GameManager.Instance.Player.transform.position.y+(float)0.5, transform.position.z);
               _target= new Vector3(GameManager.Instance.Player.transform.position.x, GameManager.Instance.Player.transform.position.y + (float)0.5, _zoomEvent+1);
                _isEvent = true;

            }
            else
            {
                if (GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Boy)
                    _target = GameManager.Instance.Player.transform.position;
                if (GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Cat)
                    _target = GameManager.Instance.Cat.transform.position;


                var targetVelocity = _target - _lastTargetPosition;
                var flyaway = Vector3.ClampMagnitude(Vector3.back * (targetVelocity.magnitude * ZFlyAwayCoeff), MaxFlyaway);

                transform.position = Vector3.Lerp(transform.position, _target + _offset + flyaway + targetVelocity * LookAheadCoeff, Time.deltaTime);
                _lastTargetPosition = _target;
            }
        }
	}
}
