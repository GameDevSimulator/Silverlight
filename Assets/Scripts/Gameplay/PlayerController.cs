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

                UpdateFlashlightDirection();

                if (FlashLight != null)
                {
                    if (Input.GetButton(WellKnown.Buttons.Fire))
                        FlashLight.Charge();

                    if (Input.GetButtonUp(WellKnown.Buttons.Fire))
                        FlashLight.Release();
                }
            }
            else
            {
                if (_movement.AcceptInput)
                    _movement.AcceptInput = false;
            }
        }

        void UpdateFlashlightDirection()
        {
            // TODO: MOVE FLASHLIGHT ALIGNING LOGIC FROM FLASHLIGHT TO HERE
        }

        void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag(WellKnown.Tags.Spikes))
                Death();
        }

        void OnStuckInDarkness()
        {
            Death();
        }

        void Death()
        {
            GameManager.Instance.Respawn();
        }
    }
}
