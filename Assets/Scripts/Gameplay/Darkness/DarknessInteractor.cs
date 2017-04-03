using UnityEngine;

namespace Assets.Scripts.Gameplay.Darkness
{
    [RequireComponent(typeof(Renderer))]
    public class DarknessInteractor : MonoBehaviour
    {
        public static int PhysicsId = 0;

        public enum InteractionType
        {
            PhysicsOnly,
            Light,
            DarknessMask,
        }

        public enum ProcessingMode
        {
            MeshOnly,
            MeshWithColorData,
            MeshWithMask,
        }

        public Material InteractionMaterial { get { return _material; } }
        public Renderer Renderer { get { return _renderer; } }
        public int BodyId { get; private set; }

        public InteractionType Interaction;

        public Texture Mask;
        

        [Space]
        [Range(0f, 10f)]
        public float Depenetration = 1f;

        [Range(0f, 1f)]
        public float Restitution = 0f;

        [Range(0f, 1f)]
        public float DynamicFriction = 0.5f;

        [Range(0f, 1f)]
        public float StaticFriction = 0.5f;


        private Rigidbody _body;
        private MeshRenderer _renderer;
        private Material _material;

        
        public const string ShaderName = "Darkness/Interactor";

        void Start()
        {
            _body = GetComponent<Rigidbody>();
            _renderer = GetComponent<MeshRenderer>();
            _material = new Material(Shader.Find(ShaderName));

            //_renderer.material.SetTexture("_MaskTex", Mask);

            // _DarknessColor.r - daRkness state
            // _DarknessColor.g - liGht
            // _DarknessColor.b - oBject id
            // _DarknessColor.a - darkness mAsk

            const string key = "_Color";

            switch (Interaction)
            {
                case InteractionType.PhysicsOnly:
                {
                    BodyId = (DarknessInteractor.PhysicsId + 1) % 256;
                    DarknessInteractor.PhysicsId = BodyId;

                    _material.SetColor(key, new Color(0, 0, BodyId / 256f, 0));
                    break;
                }
                case InteractionType.DarknessMask:
                    _material.SetColor(key, new Color(0, 0, 0, 1));
                    break;
                case InteractionType.Light:
                    _material.SetColor(key, new Color(0, 1, 0, 0));
                    break;
            }

            if (Mask != null)
                _material.SetTexture("_Mask", Mask);
        }

        public Rigidbody GetRigidbody()
        {
            if(Interaction == InteractionType.PhysicsOnly)
                return _body;
            return null;
        }

        void OnCollisionWithDarkness(DarknessArea.DarknessCollision collision)
        {
            if (_body == null)
                return;

            if(collision.IntersectedArea < 3)
                return;

            if (collision.IntersectedFactor > 0.7)
            {
                _body.isKinematic = true;
                return;
            }
            else
            {
                _body.isKinematic = false;
            }

            var intersectedMassFactor = 1f;
            if(collision.EdgeArea > 0)
                intersectedMassFactor = collision.IntersectedArea / (float)collision.EdgeArea;

            var v = _body.GetPointVelocity(collision.Contact);
            var r = collision.Contact - _body.worldCenterOfMass;
            var n = collision.Normal;

            // LINEAR MODEL
            //var jLinear = Mathf.Max(-(1 + e) * Vector3.Dot(_body.velocity * _body.mass, n), 0);
            //_body.AddForce(jLinear * n, ForceMode.Impulse);
            //continue;


            //var a2 = MultiplyComponents(Iwi, a1);

            var inertiatensor3 = _body.inertiaTensorRotation * _body.inertiaTensor;
            var inverseBodyTensor = new Matrix4x4
            {
                m00 = 1f / 1f,
                m11 = 1f / 1f,
                m22 = 1f / inertiatensor3.z,
                m33 = 1.0f
            };

            var rotation = new Matrix4x4();
            rotation.SetTRS(Vector3.zero, _body.rotation, Vector3.one);

            var inverseInertiaTensorWorld = rotation * inverseBodyTensor * rotation.transpose;
            // parallel axis theorem
            var inverseInertiaTensorWithOffset = new Matrix4x4
            {
                m00 = inverseInertiaTensorWorld.m00,
                m01 = inverseInertiaTensorWorld.m01 - r.x * r.y * _body.mass,
                m02 = inverseInertiaTensorWorld.m02 - r.x * r.z * _body.mass,

                m10 = inverseInertiaTensorWorld.m10 - r.y * r.x * _body.mass,
                m11 = inverseInertiaTensorWorld.m11,
                m12 = inverseInertiaTensorWorld.m12 - r.y * r.z * _body.mass,

                m20 = inverseInertiaTensorWorld.m20 - r.z * r.x * _body.mass,
                m21 = inverseInertiaTensorWorld.m21 - r.z * r.y * _body.mass,
                m22 = inverseInertiaTensorWorld.m22,
                m33 = 1f
            };

            //print(inverseInertiaTensorWithOffset);

            //var iIz = (_body.rotation*_body.inertiaTensorRotation*_body.inertiaTensor).z;
            //var iIz = (_body.rotation*_body.inertiaTensorRotation*_body.inertiaTensor).z + _body.mass * transform.InverseTransformVector(r).sqrMagnitude;
            //var iIz = (_body.inertiaTensorRotation * _body.inertiaTensor).z - _body.mass * r.sqrMagnitude;
            //var iIz = InverseComponents(_body.inertiaTensorRotation*_body.inertiaTensor);
            var iIz = r.sqrMagnitude * _body.mass;
            var a2 = Vector3.Cross(r, n) / iIz;

            //var a2 = inverseInertiaTensorWithOffset.MultiplyPoint(Vector3.Cross(r, n));
            //var a2 = inverseInertiaTensorWorld.MultiplyPoint(Vector3.Cross(r, n));
            //var a2 = MultiplyByInverseInertiaTensorAt(_body, contact, Vector3.Cross(r, n));

            //Debug.DrawRay(contact, a2 * 5, Color.blue);

            var a4 = Vector3.Dot(Vector3.Cross(a2, r), n);


            // PENETRATION CORRECTION
            //_body.transform.Translate(transform.TransformVector(-col.GetPenetrationNormal().normalized * col.GetPenetrationDepth()), Space.World);
            //_body.transform.Translate(n * intersectedMassFactor * Time.fixedDeltaTime * 100f, Space.World);

            if (collision.EdgeArea > 0)
                _body.transform.Translate(n * (Depenetration * intersectedMassFactor / 512f), Space.World);

            // reaction impulse magnitude
            var jr = Mathf.Max((-(1 + Restitution) * Vector3.Dot(v, n)) / (1f / _body.mass + a4), 0);
            _body.AddForceAtPosition(jr * n, collision.Contact, ForceMode.Impulse);

            // update particle velocity at contact
            //v = v + jr * n / _body.mass;


            // DYNAMIC FRICTION
            // tangent (based on velocity direction)
            var t = (v - Vector3.Dot(v, n) * n).normalized;

            // dynamic friction impulse magnitude
            var jd = DynamicFriction * jr;

            // static friction 
            var js = StaticFriction * jr;


            var dot = Vector3.Dot(_body.mass * v, t);
            if (Mathf.Abs(Vector3.Dot(v, t)) < Mathf.Epsilon && dot <= js)
            {
                // static friction impulse
                var jf = -dot * t;
                _body.AddForceAtPosition(jf, collision.Contact, ForceMode.Impulse);
                Debug.DrawRay(collision.Contact, jf, Color.red, 1f);
            }
            else
            {
                // dynamic friction impulse
                var jf = -jd * t;

                _body.AddForceAtPosition(jf, collision.Contact, ForceMode.Impulse);
                Debug.DrawRay(collision.Contact, jf, Color.cyan);
            }
        }

        void OnCollisionWithDarknessExit()
        {
            if (_body == null)
                return;

            _body.isKinematic = false;
        }
    }
}
