using UnityEngine;
using System.Collections.Generic;

public class navPoint
{
    public bool isEdge;
    public Vector3 walkablePoint;

    //Constructor for creation of a point
    public navPoint(float xComp,float yComp, float zComp, bool edge)
    {
        this.isEdge = edge;
        this.walkablePoint = new Vector3(xComp, yComp, zComp);
    }
}

public class basicEnemy : MonoBehaviour {
    //Stores the possible states for the AI
    private enum state { START, PATROLLING, SEEK, CHASEANDATTACK, DEAD }

    //Variables used for noise detection
    public SphereCollider myCollider;
    public MovingPlayer sensingPerson;

    //Stores information about the AI
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
   
    //Stores where the player was last seen or heard - could add random fuzzyness for hearing
    public Vector3 lastHeardLocation;
    public Vector3 lastSeenLocation;

    //Stores the enemy's current state, defaults to start
    private state currentState = state.START;
    private List<navPoint> pathFindArray = new List<navPoint>();
    private navPoint currentWaypoint;
    private navPoint currentPosition;

    //Variables
    public float jumpSpeed = 5.0f;
    public float gravity = 1.0f;
    private float acceleration = 0.02f;
    private bool isFlipped = false;
    private Vector2 moveDirection = Vector2.zero;

    public GameObject prefab;
    void Start()
    {
        //This test list vector will be what the enemy uses to pathfind
        pathFindArray = drawInitialMap();
        for (int i = 0; i < pathFindArray.Count; i++)
        {
            Debug.Log("Walkable point " + i + " is " + pathFindArray[i].walkablePoint);
            Debug.Log("Is the object an Edge? " + pathFindArray[i].isEdge);
            //If the platform is an edge, turn the prefab red
            if (pathFindArray[i].isEdge == true)
            {
                //Debug.Log("Setting colour to red");
                //prefab.GetComponent<Renderer>().material.color = Color.red;
                //Instantiate(prefab, pathFindArray[i].walkablePoint, Quaternion.identity);
            }
            if(pathFindArray[i].isEdge == false)
            {
                //Debug.Log("Setting colour to grey");
                //prefab.GetComponent<Renderer>().material.color = Color.grey;
                //Instantiate(prefab, pathFindArray[i].walkablePoint, Quaternion.identity);
            }
        }
        //Stores the initial tracking waypoint for the ai
        currentWaypoint = pathFindArray[Random.Range(0,pathFindArray.Count)];
        //Stores the ai's initial position in the array of walkable points and sets its position to that
        currentPosition = pathFindArray[Random.Range(0, pathFindArray.Count)];
        transform.position = currentPosition.walkablePoint;
    }
	void FixedUpdate () {

        
        switch (currentState) {

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
                    if(health <= 0)
                    {
                        //Used to determine to the player if the enemy is dead or not
                        isDead = true;
                        swapState(state.DEAD);
                        break;
                    }
                   
                    
                    //Check if you can see the player, if you can, swap to chase and attack sight checked first, primary sense
                    RaycastHit connect;
                    Ray sightRayRight= new Ray(transform.position, Vector3.right);
                    Ray sightRayLeft = new Ray(transform.position, Vector3.left);
                    
                    //Check right
                    if (Physics.Raycast(sightRayRight, out connect, sightLength))
                    {
                        //Player has been seen to the right
                        if(connect.collider.tag == "Player")
                        {
                            swapState(state.CHASEANDATTACK);
                            break;
                        }
                        //Check left
                    }else if (Physics.Raycast(sightRayLeft, out connect, sightLength))
                    {
                        //Player has been seen to the right
                        if (connect.collider.tag == "Player")
                        {
                            swapState(state.CHASEANDATTACK);
                            break;
                        }
                    }
                    else
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
                        move(state.PATROLLING, currentWaypoint);

                    }
                    else
                    {
                        //Move in a the same direction
                        move(state.PATROLLING, currentWaypoint);
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
                    if(waitCounter >= maxWaitTime)
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
                    currentWaypoint = findClosestNavPoint(lastHeardLocation);
                    move(state.SEEK, currentWaypoint);
                    //Lookup list of possible navigation points to where heard/seen
                    //Move to the one closest to the player, one navpoint at a time (update what navpoint in list the enemy is currently on
                    //transform.position = Vector3.MoveTowards(transform.position, lastHeardLocation, speed * Time.deltaTime);

                    //Make a check if you can see player
                    //If you can transfer to chase and attack
                    RaycastHit connect;
                    Ray sightRayRight = new Ray(transform.position, Vector3.right);
                    Ray sightRayLeft = new Ray(transform.position, Vector3.left);

                    //Check right
                    if (Physics.Raycast(sightRayRight, out connect, sightLength))
                    {
                        //Player has been seen
                        if (connect.collider.tag == "Player")
                        {
                            lastSeenLocation = sensingPerson.transform.position;
                            swapState(state.CHASEANDATTACK);
                            break;
                        }
                        //Check left
                    } else if (Physics.Raycast(sightRayLeft, out connect, sightLength))
                    {
                        //Player has been seen
                        if (connect.collider.tag == "Player")
                        {
                            lastSeenLocation = sensingPerson.transform.position;
                            swapState(state.CHASEANDATTACK);
                            break;
                        }
                    }
    
                        //Can you still hear the player?
                    if (myCollider.bounds.Intersects(sensingPerson.noiseCollider.bounds))
                    {
                        //Reupdate the last heard location of the player by using bounds
                        lastHeardLocation = sensingPerson.transform.position;
                    }else
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
                    //Chase player with twice speed- change this to move function call after and also consider lerp if want to slow before player
                    transform.position = Vector3.MoveTowards(transform.position, lastHeardLocation, 2 * speed * Time.deltaTime);

                    //Make a check if you can still see the player
                    //If you can, reupdate last seen and heard positions

                    RaycastHit connect;
                    Ray sightRayRight = new Ray(transform.position, Vector3.right);
                    Ray sightRayLeft = new Ray(transform.position, Vector3.right);
                    
                    //Check right
                    if (Physics.Raycast(sightRayRight, out connect, sightLength))
                    {
                        //Player has been seen
                        if (connect.collider.tag == "Player")
                        {
                            lastSeenLocation = sensingPerson.transform.position;
                            
                        }
                        //Check left
                    }else if(Physics.Raycast(sightRayLeft, out connect, sightLength))
                    {
                        //Player has been seen
                        if (connect.collider.tag == "Player")
                        {
                            lastSeenLocation = sensingPerson.transform.position;
                            
                        }
                    }else
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

    void swapState(state switchStateVal)
    {
        currentState = switchStateVal;
    }

    //Stores the current state, the node in world space to move to, and the place with which the ai is starting
    void move(state myState, navPoint target)
    {

        //Switch on how to pathfind based on 
        switch (myState)
        {
            //Must navigate to the closest point first, check if its an edge, if it is, jump, if not etc as described below
            case state.PATROLLING:
                //Move towards the navPoint Target
                //TODO: Replace the Vector 3 navigation with character controller move afterwards
                transform.position = Vector3.Lerp(transform.position, target.walkablePoint, Time.deltaTime * speed);

                //If the current tile you are on is an edge, jump with your jumpforce in the direction of travel

                return;
            case state.SEEK:
                //Set the target walkable point to the closest navPoint in the list to the player
                transform.position = Vector3.Lerp(transform.position, target.walkablePoint, Time.deltaTime * speed);
                break;
            // Eg perform binary search on walkable points to find the point closest to the actual heard location


            default:
                return;
        }

        //Use some sort of random val to pick a direction (weighted directions for more complex AI) and move that direction for x seconds
        //Will also need to access whether the player is above them or not, and if so and there is a platform above, try and jump


        //Move needs firstly to use the target waypoint as the end destination
        //First find the closest navpoint to the enemy that is in the direction of the end destination
        //Move towards that navpoint and if it is an edge, calculate jump force and attempt to jump whilst still moving in that direction
        //If the target is above the enemys current position, find the current edge that is closest to the player and jump to it (Maybe)
        //Potentially add bool for when jumping, and when jumping, ignore collisions so that the character can jump up and down without an issue
    }

  
    
    //Function will take the positions of the game objects in the level, calculate nodes above them for walkable tiles
    //Expensive so need to limit the number of redraws if we can
    //TODO:: Note may need to redraw the map when the world has been rotated or it will mess up the waypoints
    List<navPoint> drawInitialMap()
    {
        List<navPoint> pointList = new List<navPoint>();
        
        GameObject[] platformArray  = GameObject.FindGameObjectsWithTag("platform");
        foreach (GameObject g in platformArray)
        {
            //Find the platforms position and its length-has to be of int scale to create aproximate grid
            //Note: The transform position gives the center of the game object, so translate it appropriately
            //First calculate how far above and below the center we need to go based on the platform length

            float platformLength = g.transform.lossyScale.x; //Eg platform scale 5 so -> 5/2 = 2.5 Round(2.5) = 3 - 1 = 2 so add points +-2 from startPoint
            //Debug.Log(platformLength);
            float scaleLength = Mathf.CeilToInt(platformLength / 2) -1;
            //Debug.Log(scaleLength);


            //Store points into pointList for the graph for the set = {position, pos +1 ... pos+ length}
            //This will give a set of walkable tiles in the map
           
            
            //Then if the scaled length is 1 (platform length 3)
            if(scaleLength == 1)
            {
                //Just add the single point to walkable spaces (the center point)
                pointList.Add(new navPoint(g.transform.position.x,g.transform.position.y+1,g.transform.position.z,false));
                //Also add the values to the pointlist above and below the center value
                pointList.Add(new navPoint(g.transform.position.x + 1, g.transform.position.y + 1, g.transform.position.z,true));
                pointList.Add(new navPoint(g.transform.position.x - 1, g.transform.position.y + 1, g.transform.position.z,true));
            }
            else
            {
                //For the length of the platform
                //IMPORTANT NOTE: The scales of the objects must be of integer length for this to efficiently calculate above the tiles

                //First handle platform length of size 1
                if (platformLength == 1)
                {
                    //Just add the 1 square to the walkable points (it is an edge point)
                    pointList.Add(new navPoint(g.transform.position.x, g.transform.position.y + 1, g.transform.position.z,true));
                }else
                {
                    //Add the center point
                    pointList.Add(new navPoint(g.transform.position.x, g.transform.position.y + 1, g.transform.position.z, false));

                    //For the scaled length, place walkable navpoints either side of the center until the edge of the platform
                    for (int i = 1; i <= scaleLength; i++)
                    {
                        //If the i counter equals the scale length, we have an edge
                        if(i == scaleLength)
                        {
                            //Add the points in the normal way, but tag them as edges
                            //Add the point in center (i==0) and the points +- platformLength
                            pointList.Add(new navPoint(g.transform.position.x + i, g.transform.position.y + 1, g.transform.position.z,true));
                            pointList.Add(new navPoint(g.transform.position.x - i, g.transform.position.y + 1, g.transform.position.z,true));
                        }
                        else
                        {
                            //Otherwise, you are not on an edge, so add them to the list normally
                            pointList.Add(new navPoint(g.transform.position.x + i, g.transform.position.y + 1, g.transform.position.z, false));
                            pointList.Add(new navPoint(g.transform.position.x - i, g.transform.position.y + 1, g.transform.position.z, false));

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
    private navPoint findClosestNavPoint(Vector3 targetPoint)
    {
        float minDistanceToTarget = Mathf.Infinity;
        float distance = Mathf.Infinity;
        int index = -1;

        //TODO: If have time replace this linear search with binary search and put in report for efficiency increase
        for(int i = 0; i < pathFindArray.Count; i++)
        {
            distance = Vector3.Distance(pathFindArray[i].walkablePoint, targetPoint);

            if(distance < minDistanceToTarget)
            {
                minDistanceToTarget = distance;
                index = i;
            }
        }
        Debug.Log("Navigating to point " + pathFindArray[index].walkablePoint);
        return pathFindArray[index];
    }

}


