using UnityEngine;
using System.Collections;

public class cameraFollowScript : MonoBehaviour
{

    //Attached game object to apply camera transform on
    public GameObject followTarget;
    public bool hasRotated = false;
    public float currentcamRotation = 0;

    //Store the offset to keep the camera from the target
    Vector3 camDelay;

    // Use this for initialization
    void Start()
    {
        camDelay = transform.position - followTarget.transform.position;
    }

   
    void LateUpdate()
    {
        if (hasRotated == false)
        {
            //Stores the position of the target and updates the cam position to camDelay offset value behind player
            Vector3 actualPosition = followTarget.transform.position + camDelay;
            transform.position = actualPosition;

        }
        if (hasRotated == true)
        {

            //Stores the position of the target and updates the cam position to camDelay offset value behind player
            Vector3 actualPosition = followTarget.transform.position + camDelay;
            transform.position = actualPosition;
        }


    }

    public void rotateCamera()
    {
        //Get the current value of rotation about the z axis of the object
        currentcamRotation = transform.rotation.z;

        //Define a target rotation of 180 degrees 
        Vector3 targetRotation = new Vector3(0, 0, 180);

        //If the world has not been rotated
        if (hasRotated == false)
        {
            //If the current rotation of the camera is equal to 180 degrees
            if (transform.eulerAngles == targetRotation)
            {
                //Set hasRotated to true
                hasRotated = true;
                //Find the parent gravity object and set its transform(rotating the world)
                transform.parent.gameObject.transform.Rotate(new Vector3(0, 0, 180));

                //Move the camera slightly so that it still centers the player
                transform.position = followTarget.transform.position + camDelay;
                camDelay = transform.position - followTarget.transform.position;

            }
            else
            {
                //Rotate the camera smoothly clockwise on each call
                transform.eulerAngles = Vector3.MoveTowards(transform.rotation.eulerAngles, new Vector3(0, 0, 180), 100 * Time.deltaTime);

            }

        }
        else if (hasRotated == true)
        {
            if (transform.eulerAngles == targetRotation)
            {
                //Set it to 0 degrees and set hasRotated to false
                hasRotated = false;
                //Find the parent gravity object and set its transform
                transform.parent.gameObject.transform.Rotate(new Vector3(0, 0, 180));

            }
            else
            {
                //Rotate the camera smoothly anti-clockwise on each call
                transform.eulerAngles = Vector3.MoveTowards(transform.rotation.eulerAngles, new Vector3(0, 0, 180), 100 * Time.deltaTime);
            }
        }
    }


}
