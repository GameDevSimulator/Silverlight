using UnityEngine;
using System.Collections;

public class CatController : MonoBehaviour
{
    public float Speed = 1f;
    public float JumpSpeed = 10f;
    public float Gravity = 20.0F;

    private CharacterController _controller;

    void Awake()
    {
        GameManager.Instance.Cat = this;
    }

    void Start ()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update ()
    {
        if(GameManager.Instance.CurrentControllableCharacter == Character.Cat)
        {
            var moveDirection = new Vector3(Input.GetAxis("Horizontal") * Speed, 0, 0);
            
            if (_controller.isGrounded && Input.GetButton("Jump")) //прыжок
            {
                moveDirection.y = JumpSpeed;
            }
            moveDirection.y -= Gravity * Time.deltaTime;
            Debug.Log(moveDirection);
            _controller.Move(moveDirection * Time.deltaTime);
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
