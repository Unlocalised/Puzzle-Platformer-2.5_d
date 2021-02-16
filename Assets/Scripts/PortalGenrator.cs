using UnityEngine;
using System.Collections;

public class PortalGenrator : MonoBehaviour {

    //This class is used to spawn a portal after a certain task has been achieved trigger entered
    //Right now the script is only trigger based, but this could be adapted for when ceratin quest objectives have been completed

    //Gets the gameobject associated with the end portal and activates/ deactivates it
    public GameObject portalObject;

	
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && other.GetType() == typeof(CapsuleCollider))
        {
            //Player has got to the needed part of the level to unlock the next objective/exit/logicpath
            if(portalObject.activeSelf == false)
            {
                //Activate the portal game object
                portalObject.SetActive(true);

            }

        }
    }
}
