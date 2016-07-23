using System.Collections;
using UnityEngine;

public class FlashLightRotate : MonoBehaviour {
    [SerializeField] private GameObject gamemanager;
    public float horizontalSpeed = 2.0F;
    public float verticalSpeed = 2.0F;
    private int playerselector;

    void Start () {
        playerselector = gamemanager.GetComponent<GameManager>().PlayerSelector;
    }

    void Update()
    {
        playerselector = gamemanager.GetComponent<GameManager>().PlayerSelector; //проверка активного игрока
        if(playerselector == 1)
        {
            Debug.Log("azaza");
            float h = horizontalSpeed * Input.GetAxis("Mouse X");
            float v = verticalSpeed * Input.GetAxis("Mouse Y");
            transform.Rotate(-v, h, 0);
        }
    }
}
