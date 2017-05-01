using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    [RequireComponent(typeof(PhysicsCharacterController))]
    public class CatController : MonoBehaviour
    {
        public enum CatState
        {
            Active,
            InBag,
            ReturningToBoy,
            JumpingToBag,
        }

        public CatState State { get; private set; }

        private PhysicsCharacterController _controller;

        void Awake()
        {
            // Self propagate to GameManager
            GameManager.Instance.Cat = this;
        }

        void Start ()
        {
            _controller = GetComponent<PhysicsCharacterController>();
            State = CatState.Active;
        }

        void Update ()
        {
            if(GameManager.Instance.CurrentControllableCharacter == WellKnown.Character.Cat)
            {
                if (!_controller.AcceptInput)
                    _controller.AcceptInput = true;
            }
            else
            {
                if (_controller.AcceptInput)
                    _controller.AcceptInput = false;
            }

            // If cat is moving automatically 
            if (State == CatState.ReturningToBoy)
            {
                // If player is trying to control the cat then disable auto movement
                if (Input.GetAxis(WellKnown.Axis.Horizontal) > 0)
                {
                    State = CatState.Active;
                    return;
                }

                DoAutoMovement();
            }

            if (State == CatState.JumpingToBag)
            {
                // TODO: DO SOMETHING HERE!
                gameObject.SetActive(false);
                State = CatState.InBag;
            }
        }

        void DoAutoMovement()
        {
            var distanceToBoy = (transform.position - GameManager.Instance.Boy.transform.position).magnitude;

            // if boy is near
            if (distanceToBoy < 1f)
            {
                State = CatState.JumpingToBag;
                GameManager.Instance.MainCamera.ZoomIn();
            }
            else
            {
                // TODO: CHANGE IT! AUTOMOVEMENT IS NOT IMPLEMENTED!!!!
                // TODO: CREATE SOME WALKING AI COMPONENT
                State = CatState.Active;
            }
        }
    
        void OnTriggerEnter (Collider col) 
        {
            // If the cat collides with the spikes than call respawn
            if(col.CompareTag(WellKnown.Tags.Spikes))
            {
                GameManager.Instance.Respawn();
            }
        }

        public void ReturnToBoy()
        {
            if (State == CatState.Active)
            {
                State = CatState.ReturningToBoy;
            }
        }

        public void JumpOfTheBag()
        {
            if (State == CatState.InBag)
            {
                State = CatState.Active;
                transform.position = GameManager.Instance.Boy.transform.position;
                gameObject.SetActive(true);
                GameManager.Instance.MainCamera.ZoomIn();
            }
        }
    }
}
