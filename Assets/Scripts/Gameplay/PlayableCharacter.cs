using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    [RequireComponent(typeof(PhysicsCharacterController))]
    public class PlayableCharacter : MonoBehaviour
    {
        public WellKnown.Character Character;
        public PhysicsCharacterController Controller { get { return _controller; } }
        private PhysicsCharacterController _controller;

        void Start()
        {
            _controller = GetComponent<PhysicsCharacterController>();
        }

        void Update()
        {
            if (GameManager.Instance.CurrentControllableCharacter == Character)
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

        public void ResetToLastCheckpoint()
        {
            
        }
    }
}
