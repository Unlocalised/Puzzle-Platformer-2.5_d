using UnityEngine;
using System.Collections;

public class boxMove : MonoBehaviour {

    //Stores the transforms for objects that will be effected by this scripts behaviour
    public Transform object1;
    public Transform object2;
    public Transform object3;

    //Stores the shifts for the transform of the objects
    public float shiftobj1, shiftobj2, shiftobj3;

    //Booleans to prevent user from multiple button presses in a short space of time
    private bool isPressed = false;
    private bool hasMoved = false;

    void OnTriggerStay(Collider other)
    {
        if(isPressed == false)
        {
            //If the player enters the button and presses Z
            if (other.tag == "Player" && Input.GetKeyDown(KeyCode.Z) && other.GetType() == typeof(CapsuleCollider))
            {
                isPressed = true;

                //If hasMoved is false, then they are moving from origin to position
                if (hasMoved == false)
                {
                    object1.position = Vector3.Lerp(object1.position, new Vector3(object1.position.x, object1.position.y + shiftobj1, object1.position.z), 1);
                    object2.position = Vector3.Lerp(object2.position, new Vector3(object2.position.x, object2.position.y + shiftobj2, object2.position.z), 1);
                    object3.position = Vector3.Lerp(object3.position, new Vector3(object3.position.x, object3.position.y + shiftobj3, object3.position.z), 1);
                    hasMoved = true;
                }else if(hasMoved == true)
                {
                    //Then we should move them from position back to the origin
                    object1.position = Vector3.Lerp(object1.position, new Vector3(object1.position.x, object1.position.y - shiftobj1, object1.position.z), 1);
                    object2.position = Vector3.Lerp(object2.position, new Vector3(object2.position.x, object2.position.y - shiftobj2, object2.position.z), 1);
                    object3.position = Vector3.Lerp(object3.position, new Vector3(object3.position.x, object3.position.y - shiftobj3, object3.position.z), 1);
                    hasMoved = false;
                }
               
            }
        }else
        {
            //isPressed is true, so set it back to false
            isPressed = false;
        }
        
    }
	
}
