using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    public PlayerController Player;
    public CatController Cat;
    public GameObject PlayerSpawnPoint;
    public GameObject СatSpawnPoint;
    public GameObject Fader;

    public Character CurrentControllableCharacter;//{ get; private set; }

    void Awake()
    {
    }

    public void Respawn () //респаун
    {
        CurrentControllableCharacter = Character.Boy;
        Player.transform.position = PlayerSpawnPoint.transform.position;
        Cat.transform.position = СatSpawnPoint.transform.position;
        Fader.GetComponent<SceneFadeInOut>().EndScene();
    }

    public void SwitchCharacter()
    {
        switch (CurrentControllableCharacter)
        {
            case Character.Boy:
                CurrentControllableCharacter = Character.Cat;
                break;
            case Character.Cat:
                CurrentControllableCharacter = Character.Boy;
                break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("f"))//
        {
            SwitchCharacter();
            Debug.Log(string.Format("Character switched to {0}", CurrentControllableCharacter));
        }
    }
}
