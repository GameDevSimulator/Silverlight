using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    [RequireComponent(typeof(CharacterMovement))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterMovement _movement;

        void Awake()
        {
            GameManager.Instance.Player = this;
        }

        void Start()
        {
            _movement = GetComponent<CharacterMovement>();
        }

        void Update()
        {
            if (GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Boy)
            {
                if (!_movement.IsControllable)
                    _movement.Activate();
            }
            else
            {
                if (_movement.IsControllable)
                    _movement.Deactivate();
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if(col.CompareTag(WellKnown.Tags.Spikes))
                GameManager.Instance.Respawn();
        }
    }
}
