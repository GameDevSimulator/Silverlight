using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CatTeleport : MonoBehaviour
{
    public GameObject Target;
 //   public GameObject Cat;

    string _message = "";
    bool _inTrigger = false;
    GameObject _currentIbject;
    void Start()
    {
    }

    void Update()
    {
        if (Input.GetButtonDown("Activate") && _inTrigger == true && GameManager.Instance.CurrentControllableCharacter == Character.Cat) { 
            //       Cat.transform.position = new Vector3(Target.transform.position.x, Target.transform.position.y, Cat.transform.position.z);
            _currentIbject.transform.position = new Vector3(Target.transform.position.x, Target.transform.position.y, _currentIbject.transform.position.z);
            
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.name == "Cat")
    {    Debug.LogWarning("Trigger entered", this);
        _message = "Press E to enter hole";
        _inTrigger = true;
        _currentIbject = col.gameObject;
    }
    }

    void OnTriggerExit(Collider col)
    {
        Debug.LogWarning("Trigger exited", this);
        _message = "";
        _inTrigger = false;
        _currentIbject = null;
    }

    void OnGUI()
    {
        if (GameManager.Instance.CurrentControllableCharacter == Character.Cat) //сделать проверку на то, что камера центрирована относительно персонажа
        {
            GUI.Label(new Rect(300, 400, 200, 200), _message);
        }
    }
}
