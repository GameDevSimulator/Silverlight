using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightActivate : MonoBehaviour {

    [Tooltip("Состояние на запуск уровня")]
    public bool State;
    public GameObject target;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {


    }

    void OnActivate()
    {
        State = !State;
        Debug.Log("SWITCH ACTIVATED, STATE:" + State);
        if (State==true)
        {
            target.SetActive(true);
        }
        else
        {
            target.SetActive(false);
        }
    }
}
