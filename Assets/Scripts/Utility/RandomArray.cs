using UnityEngine;
using System.Collections;

public class RandomArray : MonoBehaviour
{
    public GameObject Prefab;
    public int Count = 10;
    public Vector3 Size = new Vector3(1f, 1f, 0);

	void Start ()
    {
	    for (var i = 0; i < Count; i++)
	    {
            GameObject.Instantiate(Prefab, transform.position - Size * 0.5f + new Vector3(Random.value * Size.x, Random.value * Size.y, Random.value * Size.z), Quaternion.identity);
        }
	}
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Size);
    }
}
