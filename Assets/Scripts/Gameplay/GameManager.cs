using UnityEngine;
using System.Collections;
using Assets.Scripts;
using Assets.Scripts.Gameplay;

public class GameManager : Singleton<GameManager>
{
    public PlayerController Player;
    public CatController Cat;
    public GameObject PlayerSpawnPoint;
    public GameObject СatSpawnPoint;
    public GameObject Fader;

    public WellKnown.Character CurrentControllableCharacter;//{ get; private set; }

    void Awake()
    {
    }

    public void Respawn()
    {
        CurrentControllableCharacter = WellKnown.Character.Boy;
        Player.transform.position = PlayerSpawnPoint.transform.position;
        Cat.transform.position = СatSpawnPoint.transform.position;
        Fader.GetComponent<SceneFadeInOut>().EndScene();
    }

    public void SwitchCharacter()
    {
        switch (CurrentControllableCharacter)
        {
            case WellKnown.Character.Boy:
                CurrentControllableCharacter = WellKnown.Character.Cat;
                break;
            case WellKnown.Character.Cat:
                CurrentControllableCharacter = WellKnown.Character.Boy;
                break;
        }
    }

    void Update()
    {
        if (Input.GetButtonDown(WellKnown.Buttons.Switch))
        {
            SwitchCharacter();
            Debug.LogFormat("Character switched to {0}", CurrentControllableCharacter);
        }
    }
}
