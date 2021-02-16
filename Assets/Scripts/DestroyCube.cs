using UnityEngine;
using System.Collections;

public class DestroyCube : MonoBehaviour {

    private float counter;

	void OnTriggerStay(Collider other)
    {
        counter++;
        //Destory all sequence cubes you come into contact with
        if (other.tag == "seqCube" && counter > 50)
        {
                GameObject.Destroy(other.gameObject);
            counter = 0;
        }
    }
}
