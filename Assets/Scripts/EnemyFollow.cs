using UnityEngine;
using System.Collections.Generic;


//Used to store walkable points in the map
public class Waypoint
{

    //This class will store 2 variables, one for if the object is an edge, and the Vector3 position of that block

    public bool onEdge;
    public Vector3 walkablePoint;


    //Constructor for the creation of a waypoint

    public Waypoint(float xComp, float yComp, float zComp, bool edge)
    {
        this.onEdge = edge;
        this.walkablePoint = new Vector3(xComp, yComp, zComp);
    }



}

public class EnemyFollow : MonoBehaviour
{

    //Use an Enum to store the possible states for our AI
    private enum state { START, PATROLLING, SEEK, CHASEANDATTACK, DEAD }

    //Variables used so enemy can detect the player through noise
    public SphereCollider myCollider;
    public MovingPlayer sensingPerson;

    //Various variables for the AI
    public float health = 0;
    public float speed = 0.5f;
    public bool isDead = false, isHeard = false, isSeen = false;
    public float moveTime = 0;
    public float timeBetweenMoves = 4f;
    //Controls how long the enemy waits after not seeing/hearing player
    public float waitCounter = 0;
    //Controls the max amount of time the enemy will wait
    public float maxWaitTime = 3f;
    //Controls how far the enemy can see
    public float sightLength = 3f;
    public bool hasJumped = false;
    public int timeBetweenJumps = 100;
    public float jumpSpeed = 5.0f;
    public float gravity = 1.0f;
    private float acceleration = 0.02f;
    private bool isFlipped = false;
    private Vector2 moveDirection = Vector2.zero;
    private int timeOnEdge = 0;



    //Stores where the player was last seen or heard
    public Vector3 lastHeardLocation;
    public Vector3 lastSeenLocation;


    //Stores the enemy's current state, defaults to start
    private state currentState = state.START;

    //Waypoints of walkable map points, current target and current position 
    private List<Waypoint> pathFindArray = new List<Waypoint>();
    private Waypoint currentWaypoint;
    private Waypoint currentPosition;

    //Get the enemys character controller component
    private CharacterController controller;

    public GameObject prefab;

    void Start()
    {
        //This array stores walkable points for the AI
        pathFindArray = drawInitialMap();

        //Stores the initial tracking waypoint for the AI
        currentWaypoint = pathFindArray[Random.Range(0, pathFindArray.Count)];
        //Stores the ai's initial position in the array of walkable points and sets its position to that
        currentPosition = pathFindArray[Random.Range(0, pathFindArray.Count)];
        transform.position = currentPosition.walkablePoint;
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

                    //If the moving in the random direction for a certain duration
                    if (moveTime >= timeBetweenMoves)
                    {
                        //Pick a new direction
                        currentWaypoint = pathFindArray[Random.Range(0, pathFindArray.Count)];

                        //Reset the move time for this movement
                        moveTime = 0;

                        setSpeed(1);
                        //Move towards the new target
                        move(state.PATROLLING, currentWaypoint);

                    }
                    else
                    {
                        setSpeed(1);
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


                    //Set your target to the closest point in your list to the heard location
                    currentWaypoint = findClosestNavPoint(lastHeardLocation);

                    //Set to normal movement speed and move in that direction
                    setSpeed(1);
                    move(state.SEEK, currentWaypoint);

                    //Look left and right for the player, if you can see them, transfer state
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

                    //Set the seen and heard locations to the same vector in case of sight loss
                    lastHeardLocation = lastSeenLocation;

                    //Set the current target to the closest waypoint in the AI's list to where player was seen
                    currentWaypoint = findClosestNavPoint(lastSeenLocation);

                    //Chase player with increased speed
                    setSpeed(2);
                    move(state.CHASEANDATTACK, currentWaypoint);


                    //Make a check if you can still see the player
                    //If you can, reupdate last seen and heard positions
                    //Look left and right for the player
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

                    //Break will allow for the AI to continue to move in that direction on the next pass
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

    //Swaps the AI from one state to a specified other state
    void swapState(state switchStateVal)
    {
        currentState = switchStateVal;
    }

    //Stores the current state and the node in world space to move to
    void move(state myState, Waypoint target)
    {
        //If patrolling, target will be exact, if Seeking or Chase and Attack, target will be the result of ClosestnavPoint function
        //If current poistion is waypoint position you have reached your target and need an update
        if (currentPosition.walkablePoint == target.walkablePoint)
        {
            switch (myState)
            {

                case state.PATROLLING:
                    //Pick a new random position
                    currentWaypoint = pathFindArray[Random.Range(0, pathFindArray.Count)];
                    break;
                case state.SEEK:
                    //You have reached the players last heard location, player death handled in player script so just return to FixedUpdate()
                    return;

                case state.CHASEANDATTACK:
                    //You have reached the players last seen location, player death handled in player script so just return to FixedUpdate()
                    return;

                default:
                    return;


            };


        }
        else
        {
            //First and foremost, check if you are already on an edge
            if (currentPosition.onEdge == true)
            {
                //Is the target above or below you
                //To do this, get the current position transform point and compare the Y components
                float myElevation = currentPosition.walkablePoint.y;
                float targetElevation = target.walkablePoint.y;

                if (myElevation < targetElevation)
                {
                    //Jump to the target above you
                    Jump(true);
                }
                else
                {
                    //The target is below you so jump to below
                    Jump(false);
                }
            }
            else
            {
                //If the target is to the left of you (func returns true)
                if (LeftOrRight(target))
                {
                    //If they are on the left of you, move 1 unit to your left and update your current position
                    controller.Move(Vector3.left.normalized * speed * Time.deltaTime);
                    //Update your current position (Approximated through the closestNavPoint Function)
                    currentPosition = findClosestNavPoint(transform.position);
                }
                else
                {
                    //If they are on the left of you, move 1 unit to your left and update your current position
                    controller.Move(Vector3.right.normalized * speed * Time.deltaTime);
                    //Update your current position (Approximated through the closestNavPoint Function)
                    currentPosition = findClosestNavPoint(transform.position);

                }

            }

        }



    }

    //Function to determine whether the object is to the left or right of its target
    public bool LeftOrRight(Waypoint target)
    {

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

    //Function to determine which way the AI should move on a platform following a jump
    public void PlatformMove()
    {
        //Cast 2 rays from your position diagonally left and right
        RaycastHit swHit, seHit;
        Ray swRay, seRay;
        swRay = new Ray(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), new Vector3(-1, -1, 0));
        seRay = new Ray(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), new Vector3(1, -1, 0));

        //Check if both of them collide with something
        if (Physics.Raycast(swRay, out swHit) && Physics.Raycast(seRay, out seHit))
        {
            //Get both the rays distances before the object they hit, the closest intersection of the ray is the direction to move
            if (swHit.distance < seHit.distance)
            {

                //The platform is on the left of you, so move that way off the edge and update your position
                transform.position = new Vector3(transform.position.x - 2, transform.position.y, transform.position.z);
                //Update your position
                currentPosition = findClosestNavPoint(transform.position);

            }
            else if (swHit.distance > seHit.distance)
            {

                //The platform is on the right of you, so move that way off the edge and update your position
                transform.position = new Vector3(transform.position.x + 2, transform.position.y, transform.position.z);
                //Update your position
                currentPosition = findClosestNavPoint(transform.position);
            }
        }
        else if (Physics.Raycast(swRay, out swHit))
        {

            //The south west ray cast hit something
            //Move to the west to get off the edge
            transform.position = new Vector3(transform.position.x - 2, transform.position.y, transform.position.z);
            //Update your position
            currentPosition = findClosestNavPoint(transform.position);
        }
        else
        {
            //The south east ray cast hit something or neither did, meaning weve failed
            //Move to the east to get off the edge 
            transform.position = new Vector3(transform.position.x + 2, transform.position.y, transform.position.z);
            //Update your position
            currentPosition = findClosestNavPoint(transform.position);

        }

    }

    //Jumps to a newly calculated platform if able
    public void Jump(bool isAbove)
    {
        if (hasJumped == true)
        {
            //You cannot jump just yet, so don't
            //Subtract from the jumpCounter
            timeBetweenJumps--;
            //Check if the jump count is 0
            if (timeBetweenJumps <= 0)
            {
                //Time between jumps has hit zero again
                timeOnEdge++;
                //You can jump again
                hasJumped = false;
                //Reset the timebetween jumps variable
                timeBetweenJumps = 200;
            }
        }
        else
        {
            //Check whether you're above or below the target
            if (isAbove == true)
            {
                //The target is above you
                //Calculate distance between you and all edge nodes in PathfindArray that have a higher y component than yours, and pick the closest one
                //Variable to store closestEdge
                float lowestDist = Mathf.Infinity;
                int totalCount = 0;
                int neededIndex = 0;
                foreach (Waypoint way in pathFindArray)
                {
                    float tmpDist;
                    //If the y component is bigger and its an edge tile,consider it
                    if (way.walkablePoint.y > currentPosition.walkablePoint.y && way.onEdge == true)
                    {
                        //Calculate the distance between them
                        tmpDist = Vector3.Magnitude(way.walkablePoint) - Vector3.Magnitude(currentPosition.walkablePoint);
                        //If the new distance is smaller than the current lowest one
                        if (Mathf.Abs(tmpDist) < lowestDist)
                        {
                            //Set lowest to the current distance 
                            lowestDist = tmpDist;
                            //Store the current index of the smallest one
                            neededIndex = totalCount;
                        }
                    }
                    totalCount++;
                }
                //After this you have the closest edge to your player, so teleport to that edge
                transform.position = pathFindArray[neededIndex].walkablePoint;
                //Set hasJumped to true
                hasJumped = true;

                //You've just jumped so check which way you need to move based on the platform youre on
                PlatformMove();

            }
            else
            {
                //Target is below you
                //Calculate distance between you and all edge nodes in PathfindArray that have a higher y component than yours, and pick the closest one
                //Variable to store closestEdge
                float lowestDist = Mathf.Infinity;
                int totalCount = 0;
                int neededIndex = 0;
                foreach (Waypoint way in pathFindArray)
                {
                    float tmpDist;
                    //If the y component is smaller and its an edge tile,consider it
                    if (way.walkablePoint.y < currentPosition.walkablePoint.y && way.onEdge == true)
                    {
                        //Calculate the distance between them
                        tmpDist = Vector3.Magnitude(way.walkablePoint) - Vector3.Magnitude(currentPosition.walkablePoint);
                        //If the new distance is smaller than the current lowest one
                        if (Mathf.Abs(tmpDist) < lowestDist)
                        {
                            //Set lowest to the current distance 
                            lowestDist = tmpDist;
                            //Store the current index of the smallest one
                            neededIndex = totalCount;
                        }
                    }
                    totalCount++;
                }

                //After this you have the closest edge to your player, so telelport to that edge 
                transform.position = pathFindArray[neededIndex].walkablePoint;
                hasJumped = true;

                //You've just jumped so check which way you need to move based on the platform youre on
                PlatformMove();

            }
        }
    }

    //Function that casts a ray allowing the AI to 'look' in different directions
    public bool Look(Vector3 lookDirection)
    {
        //Cast a ray in the specified direction and see if you can see the player
        RaycastHit connect;
        Ray sightRay = new Ray(transform.position, lookDirection);

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

    //Set the movement speed of the AI
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
            float scaleLength = Mathf.CeilToInt(platformLength / 2) - 1;

            //Store points into pointList for the graph for the set = {position, pos +1 ... pos+ length}
            //This will give a set of walkable tiles in the map

            //Then if the scaled length is 1 (platform length 3)
            if (scaleLength == 1)
            {
                //Just add the single point to walkable spaces (the center point)
                pointList.Add(new Waypoint(g.transform.position.x, g.transform.position.y + 1, g.transform.position.z, false));
                //Also add the values to the pointlist above and below the center value, these are edges
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


    //Finds the closest point in the walkable list to the player and returns it
    private Waypoint findClosestNavPoint(Vector3 targetPoint)
    {
        float minDistanceToTarget = Mathf.Infinity;
        float distance = Mathf.Infinity;
        int index = -1;

        //Search for the closest point in your list to the point supplied by the function parameter
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
