using UnityEngine;
using System.Collections;

public class CatController : MonoBehaviour {
    [SerializeField] private GameObject gamemanager;
    [SerializeField] private int playerselector;
    [SerializeField] private GameObject spawnPoint;
    public float speed;
    private bool grounded;
    void Start ()
    {
        grounded = true;
        speed = 10f;
        playerselector = gamemanager.GetComponent<GameManager>().PlayerSelector;
    }
    void Update ()
    {
        if(Input.GetKeyDown("f") & playerselector == 0) //переключение
        {
            gamemanager.GetComponent<GameManager>().PlayerSelector = 1;
        }
    }
    void FixedUpdate ()
    {
        playerselector = gamemanager.GetComponent<GameManager>().PlayerSelector;
        if(playerselector == 0)
        {
            transform.Translate(speed * Input.GetAxis("Horizontal") * Time.deltaTime, 0f, 0f); //движение вправо-влево
            if (Input.GetKeyDown("space") & grounded == true) //прыжок
            {
                GetComponent<Rigidbody>().AddForce(Vector3.up * 7, ForceMode.Impulse);
            }
        }
    }
    void OnCollisionEnter () //контроллер положения в воздухе, для избежания двойных прыжков
    {
        grounded = true;
        Debug.Log ("grounded");
    }

    void OnCollisionExit ()
    {
        grounded = false;
        Debug.Log ("in air");
    }
    void OnTriggerEnter (Collider col) //вызов метода респаун в GameManager
    {
        if(col.transform.tag == "Spikes")
        {
            gamemanager.GetComponent<GameManager>().PlayerRespawn();
        }
    }
}
