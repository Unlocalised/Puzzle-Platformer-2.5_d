using UnityEngine;
using System.Collections;

public class platformMove : MonoBehaviour
{

    //Stores where the platform should bounce between
    public Vector3 startPos, endPos, currentPos;
    //Stores the rate at which the platform should move relative to time
    public float speed;
    //False is whether or not a target has been reached
    private bool isTargetReached = false;

    //Initialise the start position for the object
    void Start()
    {
        currentPos = startPos;
    }

    //Updates after each physics calculation (not each frame like update)
    void FixedUpdate()
    {

        //First check to see if we got to the target place (currentpos = endpos)
        if (currentPos == endPos)
        {
            //We have reached the target
            isTargetReached = true;

        }
        else if (currentPos == startPos)
        {
            //We need to go back to the end position
            isTargetReached = false;
        }

        //If we have reached the endPos Vector
        if (isTargetReached)
        {
            //Move towards the startPos Vector
            transform.position = Vector3.MoveTowards(currentPos, startPos, speed * Time.unscaledDeltaTime);
        }
        else
        {
            //Move towards the the end again
            transform.position = Vector3.MoveTowards(currentPos, endPos, speed * Time.unscaledDeltaTime);

        }

        //Update current position
        currentPos = transform.position;
    }

    //Controls the speed at which the platforms move
    public void setSpeed(float input)
    {
        speed = input;
    }



}

