using UnityEngine;
using System.Collections;
using Assets.Scripts.Game;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class Darkness : MonoBehaviour
{
    private readonly string shaderArg = "_DissolveValue";

    private Collider _collider;
    private MeshRenderer _meshRenderer;
    private float _state = 1.0f;
    private bool _isLighted = false;
    private bool _isColliderActive = true;

    [Range(0.05f, 10.0f)]
    public float DissolveTime = 0.2f;

    [Range(0.05f, 10.0f)]
    public float AppearTime = 2f;

    [Range(0.01f, 1.0f)]
    public float RayPassThreshold = 0.5f;

    [Range(0.0f, 1.0f)]
    public float ColliderDisableAt = 0.1f;

    [Range(0.0f, 1.0f)]
    public float ColliderEnableAt = 0.9f;

    public AnimationCurve AppearCurve = AnimationCurve.Linear(0, 0, 1f, 1f);
    public AnimationCurve DissolveCurve = AnimationCurve.Linear(0, 0, 1f, 1f);

    void Start ()
	{
	    _collider = GetComponent<Collider>();
	    _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material.SetFloat("_RandomValue", Random.value);
    }
	
	void Update ()
	{
	    if (_isLighted && _state > 0f)
	    {
            _state -= Time.deltaTime / DissolveTime;

            if (_isColliderActive && _state < ColliderDisableAt)
	            DisableCollider();

	        if (_state <= 0f)
	        {
                if (_isColliderActive)
                    DisableCollider();
                _state = 0f;
	        }


	        _meshRenderer.material.SetFloat(shaderArg, 1f - AppearCurve.Evaluate(_state));
        }

        if (!_isLighted && _state < 1f)
        {
            _state += Time.deltaTime / AppearTime;

            if (!_isColliderActive && _state > ColliderEnableAt)
                EnableCollider();

            if (_state >= 1f)
            {
                if(!_isColliderActive)
                    EnableCollider();
                _state = 1f;
            }

            _meshRenderer.material.SetFloat(shaderArg, 1f - AppearCurve.Evaluate(_state));
        }
        
	    _isLighted = false;
	}

    void DisableCollider()
    {
        _collider.isTrigger = true;
        _isColliderActive = false;
        //_meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    void EnableCollider()
    {
        _collider.isTrigger = false;
        _isColliderActive = true;
        //_meshRenderer.shadowCastingMode = ShadowCastingMode.On;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Tags.Light))
        {
            Debug.Log("Collision with light");
        }
    }

    public void OnLight()
    {
        //Debug.Log("On light");
        //_collider.enabled = false;
        _isLighted = true;
    }

    void OnTriggerEnter(Collider other)
    {
        //_isLighted = true;
        if (other.gameObject.CompareTag(Tags.Light))
        {
            Debug.Log("Collision with light");
        }
    }

    void OnTriggerExit(Collider other)
    {
        //_isLighted = false;
    }

    public bool IsRaysCanPass()
    {
        return _state < RayPassThreshold;
    }
}
