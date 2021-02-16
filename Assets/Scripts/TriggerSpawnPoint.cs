using UnityEngine;
using System.Collections;

public class TriggerSpawnPoint : MonoBehaviour {

	void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            //Set the players new spawn point to this one
            other.GetComponent<MovingPlayer>().SetRespawn(transform.position);
        }
    }
}
