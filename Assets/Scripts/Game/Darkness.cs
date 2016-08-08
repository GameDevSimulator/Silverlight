using UnityEngine;
using System.Collections;
using Assets.Scripts.Game;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class Darkness : MonoBehaviour
{
    private readonly string shaderArg = "_DissolveValue";

    private Collider _collider;
    private MeshRenderer _meshRenderer;
    private float _state = 1.0f;
    private bool _isLighted = false;

    [Range(0.05f, 3.0f)]
    public float DissolveSpeed = 1f;

    [Range(0.05f, 3.0f)]
    public float AppearSpeed = 0.5f;

    [Range(0.01f, 1.0f)]
    public float RayThreshold = 0.5f;

    void Start ()
	{
	    _collider = GetComponent<Collider>();
	    _meshRenderer = GetComponent<MeshRenderer>();
	}
	
	void Update ()
	{
	    if (_isLighted && _state > 0f)
	    {
	        _state -= DissolveSpeed * Time.deltaTime;

	        if (_state <= 0f)
	        {
	            AfterDissolve();
	            _state = 0f;
	        }

            _meshRenderer.material.SetFloat(shaderArg, 1f - _state);
        }

        if (!_isLighted && _state < 1f)
        {
            _state += AppearSpeed * Time.deltaTime;

            if (_state >= 1f)
            {
                AfterAppear();
                _state = 1.0f;
            }

            _meshRenderer.material.SetFloat(shaderArg, 1f -  _state);
        }
	    _isLighted = false;
	}

    void AfterDissolve()
    {
        _collider.isTrigger = true;
    }

    void AfterAppear()
    {
        _collider.isTrigger = false;
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
        return _state < RayThreshold;
    }
}
