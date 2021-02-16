using UnityEngine;
using System.Collections.Generic;


public class testu : MonoBehaviour
{

    //Use an Enum to store the possible states for our AI
    public enum state { START, PATROLLING, SEEK, CHASEANDATTACK, DEAD }

    //Variables used so enemy can detect the player through noise
    public SphereCollider myCollider;
    public MovingPlayer sensingPerson;
    public Vector3 initialSpawn;
    //Various variables for the AI
    public float health = 0;
    public float speed = 0.5f;
    public bool isDead = false, isHeard = false, isSeen = false;
    public float moveTime = 0;
    public float timeBetweenMoves = 4f;
    public float waitCounter = 0; //Controls how long the enemy waits after not seeing/hearing player
    public float maxWaitTime = 3f; //Controls the max amount of time the enemy will wait
    public float sightLength = 3f; //Controls how far the enemy can see
    public float maxJumpDistance = 3f; //Controls how many blocks AI can jump up or across
    public float currentJumpDistance = 3f; //Controls the current jump force of the AI
    public bool hasJumped = false;
    public int timeBetweenJumps = 100;
    //Stores where the player was last seen or heard - could add random fuzzyness for hearing
    public Vector3 lastHeardLocation;
    public Vector3 lastSeenLocation;

    //Stores the enemy's current state, defaults to start
    private state currentState = state.START;
    private List<Waypoint> pathFindArray = new List<Waypoint>();
    private Waypoint currentWaypoint;
    private Waypoint currentPosition;

    //Variables
    public float jumpSpeed = 5.0f;
    public float gravity = 1.0f;
    private float acceleration = 0.02f;
    private bool isFlipped = false;
    private Vector2 moveDirection = Vector2.zero;
    private int timeOnEdge = 0;
    //Get the enemys character controller component
    private CharacterController controller;

    public GameObject prefab;

    void Start()
    {
        //This test list vector will be what the enemy uses to pathfind
        pathFindArray = drawInitialMap();

        //Stores the initial tracking waypoint for the ai
        currentWaypoint = pathFindArray[Random.Range(0, pathFindArray.Count)];
        //Stores the ai's initial position in the array of walkable points and sets its position to that
        currentPosition = pathFindArray[Random.Range(0, pathFindArray.Count)];
        transform.position = currentPosition.walkablePoint;
        initialSpawn = transform.position;
        controller = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {

        switch (currentState)
        {

            //Initialises AI with health
            case state.START:
                {
                    health = 10;
                    //Transition state to patrolling
                    swapState(state.PATROLLING);
                    break;
                }
            case state.PATROLLING:
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.white;
                    //First check if the enemy is dead, if so set dead state and set isDead to true
                    if (health <= 0)
                    {
                        //Used to determine to the player if the enemy is dead or not
                        isDead = true;
                        swapState(state.DEAD);
                        break;
                    }


                    //If you look right and see the player, switch to Chase and Attack
                    if (Look(new Vector3(1, 0, 0)))
                    {
                        swapState(state.CHASEANDATTACK);
                        break;
                    }
                    else if (Look(new Vector3(-1, 0, 0)))
                    {
                        //If you can see the player to your left, transition to the chase and attack 
                        swapState(state.CHASEANDATTACK);
                        break;
                    }
                    else //You cant see the player, so listen for them
                    {
                        //If you can hear the player- could extend to more complex if triggers are found to work
                        if (myCollider.bounds.Intersects(sensingPerson.noiseCollider.bounds))
                        {
                            //Gives a direction vector based on the last heard location of the player
                            lastHeardLocation = sensingPerson.transform.position;
                            //Start seeking the player
                            swapState(state.SEEK);
                            break;
                        }

                    }

                    //If the moving in the random direction for x seconds has expired - might conflict with seeing, might have seen first
                    if (moveTime >= timeBetweenMoves)
                    {
                        //Pick a new direction -will be move with param args after testing basics
                        currentWaypoint = pathFindArray[Random.Range(0, pathFindArray.Count)];

                        //Reset the move time for this movement
                        moveTime = 0;

                        //Move towards the new target
                        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.walkablePoint, speed * Time.deltaTime);

                    }
                    else
                    {

                        //Move in a the same direction
                        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.walkablePoint, speed * Time.deltaTime);
                        //Increment moveTime
                        moveTime++;
                    }

                    //If none of the above conditions occur, skip to the next iteration of update (next frame)
                    break;
                }

            case state.SEEK:
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    //First check if the enemy is dead, as it could occur in any state
                    if (health <= 0)
                    {
                        //Used to determine to the player if the enemy is dead or not
                        isDead = true;
                        swapState(state.DEAD);
                        break;
                    }
                    //The player can now be heard
                    isHeard = true;

                    //If the enemy has not heard the enemy for maxWaitTime seconds
                    if (waitCounter >= maxWaitTime)
                    {
                        //The player cannot be heard
                        isHeard = false;
                        //Reset waitcounter
                        waitCounter = 0;
                        //Transition to patrolling
                        swapState(state.PATROLLING);
                        break;
                    }


                    //Move towards the player in that direction (involves calling the move function with specified direction)-Closest navpoint to that location
                    currentWaypoint.walkablePoint = lastHeardLocation;

                    transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.walkablePoint, speed * Time.deltaTime);
                    //Lookup list of possible navigation points to where heard/seen
                    //Move to the one closest to the player, one navpoint at a time (update what navpoint in list the enemy is currently on
                    //transform.position = Vector3.MoveTowards(transform.position, lastHeardLocation, speed * Time.deltaTime);

                    //Make a check if you can see player - Code duplication, consider placing in another routine
                    //If you can transfer to chase and attack
                    //TODO: replace the Vector3.Right ray with the way in which the cube is moving, if not moving assign random direction

                    //Look left and right for the player
                    if (Look(new Vector3(1, 0, 0)))
                    {
                        lastSeenLocation = sensingPerson.transform.position;
                        swapState(state.CHASEANDATTACK);
                        break;
                    }
                    else if (Look(new Vector3(-1, 0, 0)))
                    {
                        lastSeenLocation = sensingPerson.transform.position;
                        swapState(state.CHASEANDATTACK);
                        break;
                    }


                    //Can you still hear the player?
                    if (myCollider.bounds.Intersects(sensingPerson.noiseCollider.bounds))
                    {
                        //Reupdate the last heard location of the player by using bounds
                        lastHeardLocation = sensingPerson.transform.position;
                    }
                    else
                    {
                        //This could cause errors but is meant as a final else
                        //increment wait counter
                        waitCounter++;

                    }
                    //The final break will act as the refresh for still being in seek

                    break;
                }

            case state.CHASEANDATTACK:
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.red;
                    //First check if the enemy is dead, as it could occur in any state
                    if (health <= 0)
                    {
                        //Used to determine to the player if the enemy is dead or not
                        isDead = true;
                        swapState(state.DEAD);
                        break;
                    }

                    //The player can now be seen!
                    isSeen = true;

                    //If the enemy has not seen the enemy for maxWaitTime seconds
                    if (waitCounter >= maxWaitTime)
                    {
                        //The player cannot be seen
                        isSeen = false;
                        //Reset waitcounter
                        waitCounter = 0;
                        //Now try to hear the player
                        swapState(state.SEEK);
                        break;
                    }

                    //Get the last seen location of the player(the raycast hit)
                    //Move towards the player in that direction (twice speed)?
                    //Set the seen and heard locations to the same vector in case of sight loss
                    lastHeardLocation = lastSeenLocation;

                    //Set the enemys speed to a higher value and reset when you leave this state 
                    //Chase player with twice speed- change this to move function call after and also consider lerp if want to slow before player
                    //Move towards the player in that direction (involves calling the move function with specified direction)-Closest navpoint to that location
                    currentWaypoint.walkablePoint = lastSeenLocation;
                    //Chase player with increased speed

                    transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.walkablePoint, 1.5f * speed * Time.deltaTime);


                    //Make a check if you can still see the player
                    //If you can, reupdate last seen and heard positions
                    //Code duplication- place this can see player script segment in own routine! and replace vector3.right

                    //Look left and right for the player - Might need to remove the breaks;
                    if (Look(new Vector3(1, 0, 0)))
                    {
                        lastSeenLocation = sensingPerson.transform.position;
                        break;
                    }
                    else if (Look(new Vector3(-1, 0, 0)))
                    {
                        lastSeenLocation = sensingPerson.transform.position;
                        break;
                    }
                    else
                    {
                        //else you have tried everything and cannot see the player
                        //Increment the wait counter for the next pass
                        waitCounter++;
                    }

                    //Break will allow for the ai to continue to move in that direction on the next pass
                    break;
                }

            case state.DEAD:
                {
                    //Destroy the gameObject, we no longer need it
                    Destroy(this.gameObject, 2f);
                    break;
                }

            default:
                {
                    //If all else fails, just patrol 
                    currentState = state.PATROLLING;
                    break;
                }
        }


    }

    public void swapState(state switchStateVal)
    {
        currentState = switchStateVal;
    }


    //Function to determine whether the object is to the left or right of its target
    public bool LeftOrRight(Waypoint target)
    {
        //Variable to determine whether the target is to the left or right of the enemy, this is done first
        Vector3 leftOrRight = transform.InverseTransformPoint(target.walkablePoint);


        //Ref this or unity documentation: http://answers.unity3d.com/questions/13033/how-to-determine-if-enemy-is-on-my-left-or-right-h.html
        if (transform.position.x > target.walkablePoint.x)
        {
            return true;
        }
        else if (transform.position.x < target.walkablePoint.x)
        {
            return false;
        }
        else
        {
            return false;
        }


    }


    //Function that casts a ray allowing the AI to 'look' in different directions
    public bool Look(Vector3 lookDirection)
    {
        //Cast a ray in the specified direction and see if you can see the player
        //Check if you can see the player, if you can, swap to chase and attack sight checked first, primary sense
        RaycastHit connect;
        Ray sightRay = new Ray(transform.position, lookDirection);

        Debug.DrawRay(transform.position, Vector3.right, Color.blue, sightLength);
        if (Physics.Raycast(sightRay, out connect, sightLength))
        {
            //Player has been seen
            if (connect.collider.tag == "Player")
            {
                //You can see the player, return true
                return true;
            }
        }
        else
        {
            //You cannot see the enemy in this direction
            return false;
        }

        return false;

    }

    public void setSpeed(float movespeed)
    {
        speed = movespeed;
    }

    //Function will take the positions of the game objects in the level, calculate nodes above them for walkable tiles
    //Expensive so need to limit the number of redraws if we can
    //TODO:: Note may need to redraw the map when the world has been rotated or it will mess up the waypoints
    List<Waypoint> drawInitialMap()
    {
        List<Waypoint> pointList = new List<Waypoint>();

        GameObject[] platformArray = GameObject.FindGameObjectsWithTag("platform");
        foreach (GameObject g in platformArray)
        {
            //Find the platforms position and its length-has to be of int scale to create aproximate grid
            //Note: The transform position gives the center of the game object, so translate it appropriately
            //First calculate how far above and below the center we need to go based on the platform length

            float platformLength = g.transform.lossyScale.x; //Eg platform scale 5 so -> 5/2 = 2.5 Round(2.5) = 3 - 1 = 2 so add points +-2 from startPoint
            //Debug.Log(platformLength);
            float scaleLength = Mathf.CeilToInt(platformLength / 2) - 1;
            //Debug.Log(scaleLength);


            //Store points into pointList for the graph for the set = {position, pos +1 ... pos+ length}
            //This will give a set of walkable tiles in the map


            //Then if the scaled length is 1 (platform length 3)
            if (scaleLength == 1)
            {
                //Just add the single point to walkable spaces (the center point)
                pointList.Add(new Waypoint(g.transform.position.x, g.transform.position.y + 1, g.transform.position.z, false));
                //Also add the values to the pointlist above and below the center value
                pointList.Add(new Waypoint(g.transform.position.x + 1, g.transform.position.y + 1, g.transform.position.z, true));
                pointList.Add(new Waypoint(g.transform.position.x - 1, g.transform.position.y + 1, g.transform.position.z, true));
            }
            else
            {
                //For the length of the platform
                //IMPORTANT NOTE: The scales of the objects must be of integer length for this to efficiently calculate above the tiles

                //First handle platform length of size 1
                if (platformLength == 1)
                {
                    //Just add the 1 square to the walkable points (it is an edge point)
                    pointList.Add(new Waypoint(g.transform.position.x, g.transform.position.y + 1, g.transform.position.z, true));
                }
                else
                {
                    //Add the center point
                    pointList.Add(new Waypoint(g.transform.position.x, g.transform.position.y + 1, g.transform.position.z, false));

                    //For the scaled length, place walkable navpoints either side of the center until the edge of the platform
                    for (int i = 1; i <= scaleLength; i++)
                    {
                        //If the i counter equals the scale length, we have an edge
                        if (i == scaleLength)
                        {
                            //Add the points in the normal way, but tag them as edges
                            //Add the point in center (i==0) and the points +- platformLength
                            pointList.Add(new Waypoint(g.transform.position.x + i, g.transform.position.y + 1, g.transform.position.z, true));
                            pointList.Add(new Waypoint(g.transform.position.x - i, g.transform.position.y + 1, g.transform.position.z, true));
                        }
                        else
                        {
                            //Otherwise, you are not on an edge, so add them to the list normally
                            pointList.Add(new Waypoint(g.transform.position.x + i, g.transform.position.y + 1, g.transform.position.z, false));
                            pointList.Add(new Waypoint(g.transform.position.x - i, g.transform.position.y + 1, g.transform.position.z, false));

                        }

                    }
                }



            }

        }
        return pointList;

    }

    //Might need a redrawMap function for all platforms with the slowable (i.e moveable tiles)
    //Either that or an adaptation for the inclusion of slowable tiles in the normal mapping when the player reaches the edge

    //Finds the closest point in the walkable list to the player and returns it
    private Waypoint findClosestNavPoint(Vector3 targetPoint)
    {
        float minDistanceToTarget = Mathf.Infinity;
        float distance = Mathf.Infinity;
        int index = -1;

        //TODO: If have time replace this linear search with binary search and put in report for efficiency increase
        for (int i = 0; i < pathFindArray.Count; i++)
        {
            distance = Vector3.Distance(pathFindArray[i].walkablePoint, targetPoint);

            if (distance < minDistanceToTarget)
            {
                minDistanceToTarget = distance;
                index = i;
            }
        }
        Debug.Log("Navigating to point " + pathFindArray[index].walkablePoint);
        return pathFindArray[index];
    }



}
