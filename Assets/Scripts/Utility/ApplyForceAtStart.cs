using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    [RequireComponent(typeof(Rigidbody))]
    public class ApplyForceAtStart : MonoBehaviour
    {
        public Vector3 Force;
        public ForceMode Mode;

        [Range(0f, 10f)]
        public float Delay = 0;

        private Rigidbody _rigidbody;
	
        void Start ()
        {
            _rigidbody = GetComponent<Rigidbody>();

            if(Delay < 0.1f)
                Shoot();
            else
                StartCoroutine(ApplyForceCoroutine());
        }

        void Shoot()
        {
            _rigidbody.AddForce(Force, Mode);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Force);
        }

        IEnumerator ApplyForceCoroutine()
        {
            yield return new WaitForSeconds(Delay);
            Shoot();
        }
    }
}
