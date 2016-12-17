using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ActivateArea : MonoBehaviour
{
	public GameObject[] Targets;

	//private int _playersTriggered = 0;

	private float _currentState = 0f;
	private Vector3 _initialPosition;
	private Vector3 _pressedPosition;

	string message = "";
	bool InTrigger = false;
	public bool IsActivated { get; private set; }

	void Start ()
	{
		//_initialPosition = transform.position;
		//_pressedPosition = transform.position + Vector3.up*PressDepth;
		IsActivated = false;

	//		if (!GetComponent<Collider>().isTrigger)
	//			Debug.LogWarning("Button collider must be a trigger", this);
	}

	void Update ()
	{
		if (Input.GetButtonDown ("Activate") && InTrigger == true) {
			//Debug.LogWarning ("E pressed");
			IsActivated = true;
			foreach (var target in Targets) {
				target.SendMessage (ActivatorProxy.ActivateEvent);
				}
			}
			
	}

	void OnTriggerEnter(Collider col)
	{
		Debug.LogWarning ("Trigger entered", this);
		message = "Press E to open";
		InTrigger = true;
	}

	void OnTriggerExit(Collider col)
	{
		Debug.LogWarning ("Trigger exited", this);
		message = "";
		InTrigger = false;
	}

	void OnGUI(){
		GUI.Label (new Rect (300, 300, 200, 200), message);
	}

		
}
