using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEnemy : MonoBehaviour {
    public float fpsTargetDistance;
    public float enemyLookDistance;
    public float enemyMovementSpeed;
    public float damping;
    public Transform fpsTarget;
    Rigidbody theRigidBody;
    Renderer myRender;
    bool isDead=false;
    public float attackDistance;
    Vector3 startPosition;
    // Use this for initialization

    void Start () {
        myRender=GetComponent<Renderer>();
        theRigidBody = GetComponent<Rigidbody>();
        startPosition = transform.position;
	}
    void OnGUI()
    {
        if (isDead)
        {
            GUI.Label(new Rect(300, 300, 200, 200), "You're dead");
        }
    }
        // Update is called once per frame
        void FixedUpdate () {
        fpsTargetDistance = Vector3.Distance(fpsTarget.position, transform.position);
        if (fpsTargetDistance < enemyLookDistance)
        {
            myRender.material.color = Color.red;
            lookAtPlayer();
        }
        else
        {
            lookAtStart();
            myRender.material.color = Color.yellow;
        }
        if (fpsTargetDistance < attackDistance)
        {
            isDead = true;
        }
        else
        {
            isDead = false;
        }

    }
    void lookAtPlayer()
    {
          Quaternion rotation = Quaternion.LookRotation(fpsTarget.position - transform.position);
        //  transform.rotation = Quaternion.Slerp(transform.rotation,rotation,Time.deltaTime);
        transform.rotation= Quaternion.Slerp(transform.rotation, rotation, 1);
        theRigidBody.AddForce(transform.forward*enemyMovementSpeed);
       // transform.position = Vector3.MoveTowards(transform.position, fpsTarget.position, enemyMovementSpeed * Time.deltaTime);
    }
    void lookAtStart()
    {
        Quaternion rotation = Quaternion.LookRotation(startPosition - transform.position);
        //  transform.rotation = Quaternion.Slerp(transform.rotation,rotation,Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);
         theRigidBody.AddForce(transform.forward * enemyMovementSpeed/2);
      //  transform.position = Vector3.MoveTowards(transform.position, startPosition, enemyMovementSpeed*Time.deltaTime);
    }
}
