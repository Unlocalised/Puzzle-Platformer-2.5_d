using UnityEngine;
using System.Collections.Generic;

public class SpawnCube : MonoBehaviour {

    //Stores the list of currently existing objects that cannot exist together
    public List<string> checkList = new List<string>();
    //Stores the name of the to be created object
    public string currentName;
    //Stores the gameobject to be spawned
    private GameObject sequenceCubePrefab = null;
    //Used to determine what sort of box to spawn
    public string setColour;
    //Used to determine the location the box will spawn
    public Vector3 spawnPos;
    //Used to determine if an object has been spawned already
    private bool isSpawned = false;

    public float maxDistApart;
    void Start () {
	//If no colour has been selected, set the default to white
    if(setColour == null)
        {
            setColour = "white";
        }
	}
	
	// Update is called once per frame
	void Update () {

        
        //If the cube has strayed too far from its start position, destory it
        if(sequenceCubePrefab != null)
        {
           
            Vector3 dist = transform.position - sequenceCubePrefab.transform.position;
            Debug.Log(dist);
            //Find the distance between the objects transform and its maxDistance apart
            if (Mathf.Abs(dist.x) > maxDistApart || Mathf.Abs(dist.y) > maxDistApart || Mathf.Abs(dist.z) > maxDistApart)
            {
                Destroy(sequenceCubePrefab.gameObject);
                isSpawned = false;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(Input.GetKeyDown(KeyCode.Z) && other.tag == "Player" && other.GetType() == typeof(CapsuleCollider))
        {
            //If any object is found to be spawned, do not allow the spawning of another
            foreach(string objName in checkList)
            {
                //If you find one of the object names in the list
                if(GameObject.Find(objName) != null)
                {
                    //A cube already exists
                    isSpawned = true;
                    Debug.Log("Already Existing Cube");
                }else
                {
                    //No cube exists
                    isSpawned = false;
                }
            }
            if(isSpawned == false)
            {
                //Spawn an instance of your own cube colour
                sequenceCubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sequenceCubePrefab.transform.position = spawnPos;
                sequenceCubePrefab.transform.localScale = new Vector3(1, 1, 1);
                sequenceCubePrefab.transform.name = currentName;
                sequenceCubePrefab.tag = "seqCube";
                sequenceCubePrefab.AddComponent<Rigidbody>().useGravity = false;

                
                //Try to set the colour via string with the user specified one
                Color temp = Color.clear;
                ColorUtility.TryParseHtmlString(setColour, out temp);
                //Set the cube to that colour
                if(temp == Color.clear)
                {
                    Debug.LogError("Could not parse colour correctly");
                }else
                {
                    sequenceCubePrefab.GetComponent<Renderer>().material.color = temp;
                }
              
            }

        }
    }
}
