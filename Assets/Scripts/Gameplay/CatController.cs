using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhysicsCharacterController))]
public class CatController : MonoBehaviour
{
    public float Speed = 1f;
    public float JumpSpeed = 10f;
    public float Gravity = 20.0F;

    private PhysicsCharacterController _controller;

    void Awake()
    {
        GameManager.Instance.Cat = this;
    }

    void Start ()
    {
        _controller = GetComponent<PhysicsCharacterController>();
    }

    void Update ()
    {
        if(GameManager.Instance.CurrentControllableCharacter == Character.Cat)
        {

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
