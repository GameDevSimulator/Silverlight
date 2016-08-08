using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 _offset;
    private Vector3 _target;

    void Start ()
	{
        if(GameManager.Instance.CurrentControllableCharacter == Character.Boy) {
            _offset = transform.position - GameManager.Instance.Player.transform.position; //расстояние между камерой и игроком
        }

        if(GameManager.Instance.CurrentControllableCharacter == Character.Cat) {
            _offset = transform.position - GameManager.Instance.Cat.transform.position; //расстояние между камерой и котом
        }
	}

	void Update ()
	{
        if (GameManager.Instance.CurrentControllableCharacter == Character.Boy)
        {
            _target = GameManager.Instance.Player.transform.position + _offset;
        }

        if(GameManager.Instance.CurrentControllableCharacter == Character.Cat)
        {
            _target = GameManager.Instance.Cat.transform.position + _offset;
        }

        // easy lerp camera, TODO: update
	    transform.position = Vector3.Lerp(transform.position, _target, Time.deltaTime);
	}
}
