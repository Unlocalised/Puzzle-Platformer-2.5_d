using UnityEngine;
using System.Collections;

public class MovingPlayer : MonoBehaviour
{
    //Variables for the player 
    public float speed = 5.0f;
    public float jumpSpeed = 5.0f;
    public float gravity = 1.0f;
    public float minY = 0, maxY = 0;
    public float slowAmount = 1;
    public float pushPower = 2.0f;
    private Vector2 moveDirection = Vector2.zero;


    //Variables for the players noise sphere
    private float originObjSpeed = 1;
    private float moveTime = 1.0f;
    private float maxNoise = 5.0f;
    private float minNoise = 1.0f;
    private float acceleration = 0.02f;
    private bool isFlipped = false;
    public SphereCollider noiseCollider;

    //Current position in world space the player can respawn at
    private Vector3 currentSpawnPoint;
    //Debug for detailing where other platform is
    public Transform startPlatform;

    //Set ip the initial noise radius of the player and set the initial spawn point
    void Start()
    {
        //Initialise Variables
        noiseCollider.radius = 1.0f;

        //Set initial spawn
        currentSpawnPoint = transform.position;
    }


    void Update()
    {
        //At any point, if the player is outside the map bounds as defined by max and min y, then respawn the player
        if (transform.position.y < minY || transform.position.y > maxY)
        {
            //Respawns at the latest checkpoint triggered by the player
            Respawn();
        }


        //Get the main camera's rotation variable so we can see what current position the camera is at
        GameObject playerCam = GameObject.FindGameObjectWithTag("MainCamera");
        cameraFollowScript camMovement = playerCam.GetComponent<cameraFollowScript>();
        CharacterController controller = GetComponent<CharacterController>();

        //Check if the controller is on the ground and the camera has not been rotated
        if (controller.isGrounded && camMovement.hasRotated == false)
        {

            //Get horizontal movement input from the player
            moveDirection = new Vector2(Input.GetAxis("Horizontal"), 0);
            //Relate the move direction to the objects transform (for movement)
            moveDirection = transform.TransformDirection(moveDirection);

            //Multiply the movement transformation by the objects speed 
            moveDirection *= speed;

            //Handles vertical movement using a the Jump button (space)
            if (Input.GetButton("Jump"))
            {
                //Affects the y component of the players Vector2 movment
                moveDirection.y = jumpSpeed;
            }

            //If the camera has rotated, that means the player is upside down
        }
        else if (controller.isGrounded && camMovement.hasRotated == true)
        {

            //Get the input from the player and negate it so that the movement when flipped is the same
            moveDirection = new Vector2(Input.GetAxis("Horizontal"), 0);
            moveDirection = transform.TransformDirection(-moveDirection);
            moveDirection *= speed;

            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }

        //Check if the player is holding down the move buttons and if they are, update the noiseSphere
        updateNoiseRadius();

        //Calculate the gravity effect on the controller by subtracting from its y component over time
        moveDirection.y -= gravity * Time.deltaTime;

        //Move in the direction as supplied by the user over time
        controller.Move(moveDirection * Time.deltaTime);



        //When X is pressed
        if (Input.GetKeyDown(KeyCode.X))
        {
            //Toggle whether the world is flipped or not
            if (isFlipped == true)
            {
                isFlipped = false;
            }
            else
            {
                isFlipped = true;
            }

        }

        if (isFlipped)
        {

            //If the camera on the player has not rotated (inverted), call rotate
            if (camMovement.hasRotated == false)
            {
                camMovement.rotateCamera();
            }


        }
        if (!isFlipped)
        {

            //If the camera on the player is currently inverted
            if (camMovement.hasRotated == true)
            {
                //Rotate back to normal
                camMovement.rotateCamera();
            }

        }


        //If the user presses the H key
        if (Input.GetKeyDown(KeyCode.H))
        {
            //Find all instances in the world where an object is tagged with 'isSlowable'
            GameObject[] myArray = GameObject.FindGameObjectsWithTag("isSlowable");

            foreach (GameObject g in myArray)
            {
                //Get the objects movement component to access its speed
                platformMove myobject = g.GetComponent<platformMove>();

                //Get the speed before you modify and store in a var
                originObjSpeed = myobject.speed;
                myobject.setSpeed(slowAmount);
            }
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            //When J is pressed, set the object speed back to its original value
            GameObject[] myArray = GameObject.FindGameObjectsWithTag("isSlowable");
            foreach (GameObject g in myArray)
            {
                platformMove myobject = g.GetComponent<platformMove>();
                myobject.setSpeed(originObjSpeed);
            }
        }
    }


    private void updateNoiseRadius()
    {
        //If the player is moving, track how long they are holding the button down
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.Space))
        {

            //If the moveTime is greater than the maxNoise stop incrementing
            if (moveTime >= maxNoise)
            {
                moveTime = maxNoise;
            }
            else
            {
                //Gradually increase the time moving by the acceleration of the player upto a maximum
                moveTime += acceleration;
            }

        }
        else
        {
            //If the player is not moving, reduce the players noiseSphere down to the minNoise value
            if (moveTime <= minNoise)
            {
                moveTime = minNoise;
            }
            else
            {
                moveTime -= acceleration;
            }
        }
        //Update the radius of the players spherecollider to represent the noise
        noiseCollider.radius = moveTime;


    }

    //Handles the collisions between the object and any other object it connects with that has a Rigidbody
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //If the body that is collided with has no rididbody or isKinematic (no interaction with physics)
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic)
        {
            return;
        }


        if (hit.moveDirection.y < -0.3F)
        {
            return;
        }

        //Define a new vector that the hit object is to travel along 
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        //Apply velocity to the collided objects rigidbody to enable a pushing behaviour based on pushpower
        body.velocity = pushDir * pushPower;
    }

    public void SetRespawn(Vector3 newSpawn)
    {
        //Set this targets transform to the newspawn value
        currentSpawnPoint = newSpawn;
    }

    //Create function so if you stray too far from the platform you respawn
    //Requires there to always be a spawnPoint with that name and all to be tagged respawn
    public void Respawn()
    {
        //Get the new location of the latest spawnpoint object and set it to that
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("Respawn");
        if (isFlipped)
        {
            //If the world is flipped and you need to respawn, do so at an inverted spawnpoint
            foreach (GameObject g in spawns)
            {
                if (g.name == "spawnPoint 1")
                {
                    transform.position = g.transform.position;
                }
            }
        }
        else
        {
            transform.position = currentSpawnPoint;
        }

    }

    //Handles when the player comes into contact with the enemy
    void OnTriggerStay(Collider other)
    {
        //If the collided object has a tag of enemy and you are sufficiently close to that object
        if (other.tag == "enemy" && Vector3.Distance(transform.position, other.gameObject.transform.position) < 1.2)
        {
            //Handles the case of a specific enemy type, requiring change of that particular enemy's state
            if (other.name == "testEnemy")
            {
                //Reset the enemy's position
                other.GetComponent<testu>().transform.position = other.GetComponent<testu>().initialSpawn;
                other.GetComponent<testu>().swapState(testu.state.PATROLLING);
            }
            //Then respawn
            Respawn();
        }
    }


}
