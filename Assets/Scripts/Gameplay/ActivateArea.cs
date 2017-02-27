using UnityEngine;

namespace Assets.Scripts.Gameplay
{
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
            if (Input.GetButtonDown ("Activate") && _inTrigger && GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Boy) 
            {
                foreach (var target in Targets) 
                {
                    target.SendMessage(ActivatorProxy.ActivateEvent);
                }
            }			
        }

        void OnTriggerEnter(Collider col)
        {
            Debug.Log("Trigger entered", this);
            _message = "Press E to open";
            _inTrigger = true;
        }

        void OnTriggerExit(Collider col)
        {
            Debug.Log("Trigger exited", this);
            _message = "";
            _inTrigger = false;
        }

        void OnGUI()
        {
            if (GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Boy)
            {
                GUI.Label(new Rect(300, 300, 200, 200), _message);
            }
        }		
    }
}
