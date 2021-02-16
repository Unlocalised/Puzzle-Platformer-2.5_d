using UnityEngine;
using System.Collections.Generic;

public class SpawnpointController : MonoBehaviour {

    public struct SpawnPoint {

        GameObject spawnCapsule;
        bool hasVisited;


    }

    //List to store a dynamic amount of spawning points through the world
    //Stored as Empty Gameobjects- each having a collider, when player passes them, it updates that as the most recent checkpoint
    public List<SpawnPoint> respawnPointList = new List<SpawnPoint>();

    public SpawnPoint currentSpawnPoint;

    private int checkpointCount;

	// Use this for initialization
	void Start () {
	
        //If the list contains no elements, spawn locations still need to be set for the level
        if( respawnPointList.Count <= 0)
        {
            Debug.LogError("No Respawn Values for player set, check for existing spawnmarkers");
        }else
        {
            //If spawnpoints do exist, the initial start point of the player will be the first spawnPoint in the list
            checkpointCount = 0;
            currentSpawnPoint = respawnPointList[checkpointCount];
            
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //Function Called to update the value of the current spawn point the player is on
    void updateSpawnPoint()
    {
        
        //Increment checkpoint counter, and set the current checkpoint
        checkpointCount++;
        currentSpawnPoint = respawnPointList[checkpointCount];
    }
}
