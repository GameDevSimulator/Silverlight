using System.Collections;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    public float Radius = 0.5f;
    private Vector3 _initialPoint;
    private Plane _gamePlane;

    void Start ()
    {
        _initialPoint = transform.localPosition;
        _gamePlane = new Plane(-Vector3.forward, Vector3.zero);
    }

    void Update()
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
        
        
        /*
        var v = new Vector3(Radius, 0, 0);
        transform.position = _initialPoint + 
        gameObject.transform.RotateAround(_initialPoint, Vector3.forward, 0.1f);
        */
    }
}
