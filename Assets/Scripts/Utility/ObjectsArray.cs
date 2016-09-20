using UnityEngine;
using System.Collections;

public class ObjectsArray : MonoBehaviour {

    public GameObject Prefab;
    public Vector3 Size = new Vector3(5f, 5f, 0);
    public int Columns = 10;
    public int Rows = 10;
    
    void Start ()
    {
        for (var col = 0; col < Columns; col++)
        {
            for (var row = 0; row < Rows; row++)
            {
                var offset = new Vector3((col / (float)Columns) * Size.x, (row / (float)Rows) * Size.y); 
                GameObject.Instantiate(Prefab, transform.position - Size * 0.5f + offset, Quaternion.identity);
            }
        }
	}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Size);
    }
}
