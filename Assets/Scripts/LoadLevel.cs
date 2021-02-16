using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoadLevel : MonoBehaviour
{

    //Used for returning to the start menu
    public string levelSkip;

    //Handles the level selection on the main menu, loading the level chosen by the user
    public void OnClick(string levelToLoad)
    {
        SceneManager.LoadScene(levelToLoad);
    }

    //If button clicked with no arguments, quit the application
    public void OnClick()
    {
        Application.Quit();
    }

    //Function to allow scene transition in game, when a level is finished
    void OnTriggerEnter(Collider other)
    {

        //If the user wishes to attach a level skip to an object set to not null
        if (levelSkip != null || levelSkip == "")
        {
            if (other.tag == "Player" && other.GetType() == typeof(CapsuleCollider))
            {
                //Load the scene relevant to the skip
                SceneManager.LoadScene(levelSkip);
            }

        }
    }
}
