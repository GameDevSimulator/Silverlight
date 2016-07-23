using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
    [SerializeField] private GameObject PlayerSpawnPoint;
    [SerializeField] private GameObject СatSpawnPoint;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject cat;
    [SerializeField] private GameObject fader;
//    Vector3 playerinactive = new Vector3(0f,0f,0.156f);
    public int PlayerSelector;
    public void PlayerRespawn () //респаун
    {
        player.transform.position = PlayerSpawnPoint.transform.position;
        cat.transform.position = СatSpawnPoint.transform.position;
        fader.GetComponent<SceneFadeInOut>().EndScene();
    }
//    void FixedUpdate () {
//        if (PlayerSelector == 0)
//        {
//            player.transform.position = Vector3.Lerp(player.transform.position, player.transform.position + playerinactive, 1);
//        }
//    }
}
