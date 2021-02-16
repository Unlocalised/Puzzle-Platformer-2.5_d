using UnityEngine;
using System.Collections.Generic;

public class gearPuzzleLogic : MonoBehaviour
{

    //Used to pull all objects towards the gear and check against a sequence
    //Stores the sequence of colours that will be checked and colours to parse
    private List<Color> sequenceList = new List<Color>();
    public List<string> colourConverts = new List<string>();
    public SphereCollider pullSphere;
    public float pullRadius;
    //Public variable to control how quickly the gear pulls objects
    public float pullForce = 5;

    //An optional gameobject event that could occur from the correct sequence
    public GameObject optionalEndTrigger;

    //Stores the current point in the sequence
    private int currentSeqIndex = 0;

    void Start()
    {
        //For each string in the list
        for (int i = 0; i < colourConverts.Count; i++)
        {
            //Try and parse to a colour type
            Color temp = Color.clear;
            ColorUtility.TryParseHtmlString(colourConverts[i], out temp);

            if (temp == Color.clear)
            {
                Debug.LogError("Colour not parsed correctly, revisit the string value of colour");
            }
            else
            {
                //Set the value in the sequence list to that colour
                sequenceList.Add(temp);
            }

        }

        //Set the pull area to the user defined value
        pullSphere.radius = pullRadius;
    }
    void Update()
    {
        //If all colours have been successfully matched in sequence
        if (currentSeqIndex == sequenceList.Count)
        {
            //Set the optional event to active
            if (optionalEndTrigger != null)
            {
                optionalEndTrigger.SetActive(true);
            }

            //Reset the sequence index count
            currentSeqIndex = 0;
        }
    }
    void OnTriggerStay(Collider sequenceCube)
    {
        //For all objects inside the spherecollider with the tag 'seqCube'
        if (sequenceCube.tag == "seqCube")
        {

            //Pull the cube towards your transform
            Vector3 directionToMove = transform.position - sequenceCube.transform.position;
            sequenceCube.transform.Translate(directionToMove * pullForce * Time.deltaTime);

            //Check the colour of the cube coming in against the sequeunce list, if its correct, accept and advance the sequence index 
            //But if it violates the sequence, reset it
            if (sequenceList[currentSeqIndex] == sequenceCube.gameObject.GetComponent<Renderer>().material.color && 
                gameObject.GetComponent<BoxCollider>().bounds.Intersects(sequenceCube.GetComponent<Collider>().bounds))
            {
                //This is the correct colour in the sequence, so destroy the given cube and advance the index
                GameObject.Destroy(sequenceCube.gameObject);
                currentSeqIndex++;

            }
            else if (sequenceList[currentSeqIndex] != sequenceCube.gameObject.GetComponent<Renderer>().material.color && 
                gameObject.GetComponent<BoxCollider>().bounds.Intersects(sequenceCube.GetComponent<Collider>().bounds))
            {
                //Incorrect sequence has been entered, so reset the sequence back to the start
                GameObject.Destroy(sequenceCube.gameObject);
                currentSeqIndex = 0;
            }

        }

    }

    public void setPullRadius(float value)
    {
        //Set a runtime value for the radius that the pull takes
        pullSphere.radius = value;
    }
}
