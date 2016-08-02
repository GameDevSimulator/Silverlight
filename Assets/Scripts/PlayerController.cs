using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    [SerializeField] private GameObject gamemanager;
    [SerializeField] private int playerselector;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private GameObject flashlight;
    private bool grounded;
    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;
	void Start ()
    {
        grounded = true;
        speed = 5f;
        playerselector = gamemanager.GetComponent<GameManager>().PlayerSelector;
	}
    void Update ()
    {
        if(Input.GetKeyDown("f") & playerselector == 1) //переключение
        {
            gamemanager.GetComponent<GameManager>().PlayerSelector = 0;
        }
        if(Input.GetMouseButtonDown(0) & playerselector == 1) //фонарик
        {
            flashlight.SetActive(!flashlight.gameObject.activeSelf);
        }
    }
	void FixedUpdate ()
    {
        playerselector = gamemanager.GetComponent<GameManager>().PlayerSelector;
        if(playerselector == 1)
        {
//            transform.Translate(speed * Input.GetAxis("Horizontal") * Time.deltaTime, 0f, 0f); //движение вправо-влево
//            if (Input.GetKeyDown("space") & grounded == true) //прыжок
//            {
//                GetComponent<Rigidbody>().AddForce(Vector3.up * 5, ForceMode.Impulse);
//            }
            CharacterController controller = GetComponent<CharacterController>();
            if (controller.isGrounded) {
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
                moveDirection = transform.TransformDirection(moveDirection);
                moveDirection *= speed;
                if (Input.GetButton("Jump"))
                    moveDirection.y = jumpSpeed;

            }
            moveDirection.y -= gravity * Time.deltaTime;
            controller.Move(moveDirection * Time.deltaTime);

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
