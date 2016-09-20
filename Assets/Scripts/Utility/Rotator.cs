using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour
{
    public float Amount = 1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    transform.Rotate(0, 0, Amount);
	}
}
