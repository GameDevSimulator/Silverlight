﻿using Assets.Scripts.Gameplay;
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
