using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class CameraController : MonoBehaviour
    {
        public Transform TargetTransorm;
        [Range(0f, 100f)] public float ZFlyAwayCoeff = 5f;
        [Range(0f, 100f)] public float LookAheadCoeff = 5f;
        public float MaxFlyaway = 10f;
        [Range(1f, 10f)] public float ZoomAmount = 2f;
        public Vector3 Offset = new Vector3(0, 2f, -10f);
        [Range(0f, 10f)] public float ZoomSpeed = 1f;

        private float _zoomTime;
        private Vector3 _lastTargetPosition;


        void Awake()
        {
            // Self propagate to GameManager
            GameManager.Instance.MainCamera = this;
        }

        void Update ()
        {
            // If no target - do nothing
            if(TargetTransorm == null)
                return;

            var target = TargetTransorm.position;
            var targetVelocity = target - _lastTargetPosition;
            var direction = target - transform.position;
            var flyaway = Vector3.ClampMagnitude(Vector3.back * (targetVelocity.magnitude * ZFlyAwayCoeff), MaxFlyaway);

            var zoom = _zoomTime * ZoomAmount * direction;


            transform.position = Vector3.Lerp(transform.position, target + Offset + flyaway + targetVelocity * LookAheadCoeff, Time.deltaTime);
            _lastTargetPosition = target;
        }

        public void ZoomIn(float time = 1f)
        {
            _zoomTime = time;
        }
    }
}
