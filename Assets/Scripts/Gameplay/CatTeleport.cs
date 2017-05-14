using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class CatTeleport : MonoBehaviour
    {
        enum TeleportState
        {
            Inactive, Activated, Teleported
        }
        public GameObject Target;
        private string _message;
        private bool _inTrigger;
        private TeleportState _state=TeleportState.Inactive;
        private GameObject _currentObject;
        private float z0;

        void Start()
        {
        }

        void Update()
        {
            if (Input.GetButtonDown(WellKnown.Buttons.Activate) && _inTrigger && GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Cat)
            {
                _currentObject.transform.Rotate(0, -90, 0);
                _state = TeleportState.Activated;
                z0 = _currentObject.transform.position.z;               
                //       Cat.transform.position = new Vector3(Target.transform.position.x, Target.transform.position.y, Cat.transform.position.z);               
            }
            if (_state== TeleportState.Activated)
            {
                if  (_currentObject.transform.position.z < transform.position.z)
                {
                    _currentObject.transform.position+= new Vector3(0, 0, (float)0.1);
                }
                else
                {
                    _currentObject.transform.position= new Vector3(Target.transform.position.x, Target.transform.position.y, _currentObject.transform.position.z);
                    _state = TeleportState.Teleported;
                    _currentObject.transform.Rotate(0, 180, 0);
                }

            }
            if (_state == TeleportState.Teleported)
            {
                if (_currentObject.transform.position.z>z0)
                {
                    _currentObject.transform.position -= new Vector3(0, 0, (float)0.1);
                }
                else
                {
                    _state = TeleportState.Inactive;
                    _currentObject.transform.Rotate(0, -90, 0);
                    _currentObject.transform.position = new Vector3(_currentObject.transform.position.x, _currentObject.transform.position.y, z0);
                    _currentObject = null;
                }

            }
            
        }

        void OnTriggerEnter(Collider col)
        {
            if(col.name == "Cat")
            {
                Debug.Log("Trigger entered", this);
                Tooltip.Instance.Show("Press E to enter hole");
                _inTrigger = true;
                _currentObject = col.gameObject;
            }
        }

        void OnTriggerExit(Collider col)
        {
            Debug.Log("Trigger exited", this);
            Tooltip.Instance.Hide();
            _inTrigger = false;
            if (_state == TeleportState.Inactive)
            {
                _currentObject = null;
            }
        }
    }
}
