using System.Collections;
using Assets.Scripts;
using Assets.Scripts.Gameplay.Darkness;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    public float Radius = 0.5f;
    private Vector3 _initialPoint;
    private Plane _gamePlane;
    private float _angle = 0f;
    private float _angleVelocity;

#if DEBUG
    public bool ShowDebugInfo = false;
#endif

    void Start ()
    {
        _initialPoint = transform.localPosition;
        _gamePlane = new Plane(-Vector3.forward, Vector3.zero);
    }

    void Update()
    {
        var mouseLook = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (mouseLook.magnitude > 0.1f)
        {
            float distance;
            
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (_gamePlane.Raycast(ray, out distance))
            {
                var hitpoint = ray.GetPoint(distance);
                //Debug.Log(hitpoint);

                transform.localPosition = _initialPoint;
                var lookDirection = hitpoint - transform.position;
                var angle = Mathf.Atan2(lookDirection.x, -lookDirection.y);

                var q = Quaternion.Euler(0, 0, angle*Mathf.Rad2Deg - 90);
                transform.localPosition += q * Vector3.right * Radius;
                transform.rotation = q;
            }
        }
        else
        {
            var lookDirection = new Vector2(Input.GetAxis("Look X"), Input.GetAxis("Look Y"));
            if (lookDirection.magnitude > 0.2f)
            {
                var angleRaw = Mathf.Atan2(lookDirection.x, lookDirection.y);
                _angle = Mathf.SmoothDamp(_angle, angleRaw, ref _angleVelocity, 0.1f);

                var q = Quaternion.Euler(0, 0, -_angle * Mathf.Rad2Deg - 270);
                transform.localPosition = _initialPoint + q * Vector3.right * Radius;
                transform.rotation = q;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var go = (GameObject)GameObject.Instantiate(gameObject, transform.position, transform.rotation);
            go.GetComponent<FlashLight>().enabled = false;
            go.layer = LayerMask.NameToLayer("Invisible");
            var interactor = go.GetComponent<DarknessInteractor>();
            interactor.enabled = true;
            DestroyObject(go, 3f);
        }
    }
#if DEBUG
    void OnGUI()
    {
        if (ShowDebugInfo)
        {
            GUI.TextField(new Rect(0, 100, 200, 20), string.Format("Look X: {0}", Input.GetAxis("Look X")));
            GUI.TextField(new Rect(0, 125, 200, 20), string.Format("Look Y: {0}", Input.GetAxis("Look Y")));
            GUI.TextField(new Rect(0, 150, 200, 20), string.Format("Angle: {0}", _angle));
        }
    }
#endif
}
