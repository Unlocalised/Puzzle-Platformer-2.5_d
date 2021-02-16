using UnityEngine;
using System.Collections;

public class ToggleSingularity : MonoBehaviour {

    //Stores the reference for the sequencer object and the value that is to be used to effect said object
    public gearPuzzleLogic sequencer;
    public float singularityVal = 0;
    public ParticleSystem singularityFX;
    //Ensures the singularity is only activated once and cannot be toggled - could change this to allow toggling with more complex puzzles
    private bool isActivated = false;

    void OnTriggerStay(Collider other)
    {
        //If the player enters the trigger zone and presses Z
        if(other.tag == "Player" && other.GetType() == typeof(CapsuleCollider) && Input.GetKeyDown(KeyCode.Z))
        {
            //Set activated to false and alter the pull radius of the singularity
            if (isActivated == false)
            {
                //Enable the singularity effect
                singularityFX.gameObject.SetActive(true);
                sequencer.setPullRadius(singularityVal);
                isActivated = true;
            }
        }
    }
}
