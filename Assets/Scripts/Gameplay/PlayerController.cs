using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    [RequireComponent(typeof(PhysicsCharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public FlashLight FlashLight { get; private set; }
        private PhysicsCharacterController _movement;

        void Awake()
        {
            // Selfpropagate to GameManager
            GameManager.Instance.Boy = this;
        }

        void Start()
        {
            _movement = GetComponent<PhysicsCharacterController>();
            FlashLight = GetComponentInChildren<FlashLight>();
        }

        void Update()
        {
            if (GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Boy)
            {
                if (!_movement.AcceptInput)
                    _movement.AcceptInput = true;
            }
            else
            {
                if (_movement.AcceptInput)
                    _movement.AcceptInput = false;
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if(col.CompareTag(WellKnown.Tags.Spikes))
                GameManager.Instance.Respawn();
        }
    }
}
