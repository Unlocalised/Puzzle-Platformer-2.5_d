using UnityEngine;
using System.Collections;

public class teleportPlayer : MonoBehaviour {

    public Vector3 teleportTarget;

	void OnTriggerEnter(Collider other)
    {
        
        if(other.tag == "Player" && other.GetType() == typeof(CapsuleCollider))
        {
            //Teleport player back to where you want the target to be
            other.transform.localPosition = teleportTarget;
        }
        
    }
}
