using Assets.Scripts.Gameplay;
using UnityEngine;

[RequireComponent(typeof(PhysicsCharacterController))]
public class PlayerController : MonoBehaviour
{
    public GameObject FlashlightObject;

    private PhysicsCharacterController _controller;

    void Awake()
    {
        GameManager.Instance.Player = this;
    }

    void Start ()
    {
        _controller = GetComponent<PhysicsCharacterController>();
    }

    void Update ()
    {
        if (GameManager.Instance.CurrentControllableCharacter == Character.Boy)
        {
            if (Input.GetMouseButtonDown(0)) //фонарик
            {
                //FlashlightObject.SetActive(!FlashlightObject.gameObject.activeSelf);
            }

            if (Input.GetButtonDown("Jump"))
            {
                _controller.Jump();
            }
            
            _controller.Move(Input.GetAxis("Horizontal"));
        }
    }

    void OnTriggerEnter (Collider col) //вызов метода респаун в GameManager
    {
        if(col.transform.tag == "Spikes")
        {
            GameManager.Instance.Respawn();
        }
    }
}
