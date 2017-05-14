using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class ActivateArea : MonoBehaviour
    {
        public GameObject[] Targets;
        public string MsgText;

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
            if (col.tag != "Light")
            {
                Debug.Log("Trigger entered", this);
                Tooltip.Instance.Show(MsgText);
                _inTrigger = true;
            }
            
        }

        void OnTriggerExit(Collider col)
        {
            Debug.Log("Trigger exited", this);
            Tooltip.Instance.Hide();
            _inTrigger = false;
        }
    }
}
