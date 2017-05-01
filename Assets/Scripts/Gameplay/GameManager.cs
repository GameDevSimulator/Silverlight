using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class GameManager : Singleton<GameManager>
    {
        public PlayerController Boy;
        public CatController Cat;
        public CameraController MainCamera;
        public GameObject PlayerSpawnPoint;
        public GameObject СatSpawnPoint;
        public GameObject Fader;
        

        public WellKnown.Character CurrentControllableCharacter { get; private set; }

        public void Start()
        {
            if(Boy != null)
                ActivateBoy();
        }

        public void Respawn()
        {
            CurrentControllableCharacter = WellKnown.Character.Boy;
            Boy.transform.position = PlayerSpawnPoint.transform.position;
            Cat.transform.position = СatSpawnPoint.transform.position;
            Fader.GetComponent<SceneFadeInOut>().EndScene();
        }

        public void ActivateBoy()
        {
            // Boy is already activated
            if(CurrentControllableCharacter == WellKnown.Character.Boy)
                return;

            Debug.LogFormat("Activating Boy");
            CurrentControllableCharacter = WellKnown.Character.Boy;
            MainCamera.TargetTransorm = Boy.transform;
        }

        public void ActivateCat()
        {
            // Cat is already activated
            if (CurrentControllableCharacter == WellKnown.Character.Cat)
                return;

            if (Cat == null)
            {
                Debug.LogWarning("Cat is not set");
                return;
            }

            if (Cat.State != CatController.CatState.Active)
            {
                Debug.LogWarning("Cannot switch to cat. Cat is not in active state");
                return;
            }

            CurrentControllableCharacter = WellKnown.Character.Cat;
            MainCamera.TargetTransorm = Cat.transform;
        }

        void Update()
        {
            if (Input.GetButtonDown(WellKnown.Buttons.Switch))
            {
                if(CurrentControllableCharacter != WellKnown.Character.Cat)
                    ActivateCat();
                else if (CurrentControllableCharacter != WellKnown.Character.Boy)
                    ActivateBoy();
            }

            if (Input.GetButtonDown(WellKnown.Buttons.CallCat) && CurrentControllableCharacter == WellKnown.Character.Boy)
            {
                if(Cat == null)
                    return;

                if (Cat.State != CatController.CatState.InBag)
                {
                    Cat.ReturnToBoy();
                    Debug.LogFormat("Call for cat to go the bag");
                }
                else
                {
                    Cat.JumpOfTheBag();
                    Debug.LogFormat("Call for cat to jump of the bag");
                }
            }
        }
    }
}
