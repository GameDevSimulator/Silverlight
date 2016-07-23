using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    [SerializeField] private GameObject gamemanager;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject cat;
    private int playerselector;
    private Vector3 offset;

	void Start ()
    {
        playerselector = gamemanager.GetComponent<GameManager>().PlayerSelector;
        if(playerselector == 1) {
            offset = transform.position - player.transform.position; //расстояние между камерой и игроком
        }
        if(playerselector == 0) {
            offset = transform.position - cat.transform.position; //расстояние между камерой и котом
        }
	}
	void LateUpdate ()
    {
        playerselector = gamemanager.GetComponent<GameManager>().PlayerSelector;
        if(playerselector == 1) {
            transform.position = player.transform.position + offset;
        }
        if(playerselector == 0) {
            transform.position = cat.transform.position + offset;
        }
	}
}
