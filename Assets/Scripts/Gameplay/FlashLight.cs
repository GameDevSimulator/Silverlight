using Assets.Scripts.Gameplay.Darkness;
using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class FlashLight : MonoBehaviour
    {
        public float ChargeTime = 2f;
        public float Cooldown = 2f;
        public float Radius = 0.5f;
        public Gradient LightColorGradient;


        private Vector3 _initialPoint;
        private Plane _gamePlane;
        private float _angle = 0f;
        private float _angleVelocity;
        private float _chargingTime = 0;
        private bool _isCharging = false;

#if DEBUG
        public bool ShowDebugInfo = false;
#endif

        private LightBeam _lightBeam;

        void Start ()
        {
            _initialPoint = transform.localPosition;
            _gamePlane = new Plane(-Vector3.forward, Vector3.zero);
            _lightBeam = GetComponentInChildren<LightBeam>();
        }

        void Update()
        {
            if (!_isCharging && _chargingTime > 0f)
            {
                _chargingTime -= Time.deltaTime * 10f;
                if (_chargingTime < 0)
                    _chargingTime = 0;
            }

            var val = Mathf.Clamp01(_chargingTime / ChargeTime);

            if (_lightBeam != null)
                _lightBeam.LightColor = LightColorGradient.Evaluate(val);

            if (BatteryIndicator.Instance != null)
                BatteryIndicator.Instance.Value = val;

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

                    var q = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg - 90);
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
        }

        public void SetTarget(Vector3 position)
        {
            
        }

        public void Charge()
        {
            _chargingTime += Time.deltaTime;
           
            _isCharging = true;
        }

        public void Release()
        {
            if(_chargingTime > ChargeTime)
                Flash();

            _isCharging = false;
        }

        public void Flash()
        {
            var go = (GameObject)GameObject.Instantiate(gameObject, transform.position, transform.rotation);
            go.transform.localScale = transform.lossyScale;
            go.GetComponentInChildren<LightBeam>().FadeLight(3);
            go.GetComponent<FlashLight>().enabled = false;
            //go.layer = LayerMask.NameToLayer(WellKnown.LayerNames.Invisible);
            var interactor = go.GetComponent<DarknessInteractor>();
            interactor.enabled = true;
            DestroyObject(go, 3f);
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
}
