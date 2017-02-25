using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ActivateArea : MonoBehaviour
{
	public GameObject[] Targets;

	string _message = "";
	bool _inTrigger = false;

	void Start ()
	{
	}

	void Update ()
	{
		if (Input.GetButtonDown ("Activate") && _inTrigger == true && GameManager.Instance.CurrentControllableCharacter == Character.Boy) 
		{
			foreach (var target in Targets) 
			{
				target.SendMessage(ActivatorProxy.ActivateEvent);
			}
		}			
	}

	void OnTriggerEnter(Collider col)
	{
		Debug.LogWarning ("Trigger entered", this);
		_message = "Press E to open";
		_inTrigger = true;
	}

	void OnTriggerExit(Collider col)
	{
		Debug.LogWarning ("Trigger exited", this);
		_message = "";
		_inTrigger = false;
	}

	void OnGUI()
	{
        if (GameManager.Instance.CurrentControllableCharacter == Character.Boy)
        {
            GUI.Label(new Rect(300, 300, 200, 200), _message);
        }
	}		
}
