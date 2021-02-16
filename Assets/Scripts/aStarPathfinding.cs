using UnityEngine;
using System.Collections.Generic;

public class aStarPathfinding : MonoBehaviour {
    //|||BASE FOR PSEUDOCODE FOR ALGORITHM|||
    //Adapted from Wikipedia


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    //Create a set of points equal to the number of spaces (cubes of 1x1x1) that
    //the player can jump onto
    //Simplify- create nodes just on the start and end of each platform
    //Enemy can move normally outside of these points.

    //Will pathfind using known nodes, to the shortest distance to the player
    void pathFind(Vector3 startPos, Vector3 endPos)
    {
        //The set of nodes evaluated-Empty set to start 
        List<Vector3> evalNodePoints = new List<Vector3>();
        //The set of nodes not currently evaluate
        List<Vector3> freeNodePoints = new List<Vector3>();
        //freeNodePoints + start position

        //Find the node with the least cost (closest distance) from start/current node
        //Note, only need do this in the x and y direction as restricted on the z-axis
        //If camera rotate, will have to take into account switched axis


        
    }
}
