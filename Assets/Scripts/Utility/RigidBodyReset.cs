using UnityEngine;

namespace Assets.Scripts.Utility
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidBodyReset : MonoBehaviour
    {
        private Rigidbody _body;

        private Vector3 _initialPosistion;
        private Quaternion _initialRotation;

        void Start ()
        {
            _body = GetComponent<Rigidbody>();
            _initialPosistion = _body.transform.position;
            _initialRotation = _body.transform.rotation;
        }

        void ResetBody()
        {
            _body.transform.position = _initialPosistion;
            _body.transform.rotation = _initialRotation;
            _body.velocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
        }

        void Update ()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetBody();
            }
        }
    }
}
