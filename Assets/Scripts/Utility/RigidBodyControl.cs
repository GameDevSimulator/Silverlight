using UnityEngine;

namespace Assets.Scripts.Utility
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidBodyControl : MonoBehaviour
    {
        [Range(0f, 100f)]
        public float Force = 1f;

        private Rigidbody _body;

        void Start ()
        {
            _body = GetComponent<Rigidbody>();
        }
        
        void Update ()
        {
		    var input = new Vector3(Input.GetAxis(WellKnown.Axis.Horizontal), Input.GetAxis(WellKnown.Axis.Vertical));
            _body.AddForce(input, ForceMode.Acceleration);
        }
    }
}
