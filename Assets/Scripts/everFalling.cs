using UnityEngine;
using System.Collections;

public class everFalling : MonoBehaviour {

    public Transform myTransform;
    public float gravity = 1f;
	// Update is called once per frame
	void Update () {
        float xComponent = myTransform.position.x;
        float yComponent = myTransform.position.y;
        float zComponent = myTransform.position.z;
        if(myTransform.position.y < 2)
        {
            myTransform.position = new Vector3(xComponent, 20, yComponent);
        }else
        {
            myTransform.position = new Vector3(xComponent, yComponent - gravity, zComponent) ;
        }
	}
}
