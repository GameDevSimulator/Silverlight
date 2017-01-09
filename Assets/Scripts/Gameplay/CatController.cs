using UnityEngine;
using System.Collections;
using Assets.Scripts.Gameplay;

[RequireComponent(typeof(PhysicsCharacterController))]
public class CatController : MonoBehaviour
{
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
            if (!_controller.AcceptInput)
                _controller.AcceptInput = true;
        }
        else
        {
            if (_controller.AcceptInput)
                _controller.AcceptInput = false;
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
