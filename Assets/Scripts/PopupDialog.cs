using UnityEngine;
using System.Collections;

public class PopupDialog : MonoBehaviour
{

    //public boolean for developer if they wish to add their own size for a popup window
    public bool manualReSize = false;

    //Used for storing the width and length of the popup window
    public float popupWidth;
    public float popupHeight;


    public string popupText;
    private bool togglePopup = false;

    //On starting, work out the appropriate length for the text in the text field and set the width and height of the popup accordingly
    void Start()
    {
        if (manualReSize == false)
        {
            //Attempt to auto resize the box to the text
            //Assume average line length of 27 characters
            //So 150/27 = characters per unit rect width approx 5.55
            //15 is the min size for one line
            popupWidth = 150;
            //Work out the number of lines needed
            int numLines = Mathf.CeilToInt(popupText.Length / 27);
            if (numLines > 1)
            {
                popupHeight = 2f * (numLines * 16);
            }
            else
            {
                popupHeight = 20 * 2f;
            }

        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            togglePopup = true;
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            togglePopup = false;
        }

    }

    void OnGUI()
    {
        if (togglePopup)
        {

            //Reference http://answers.unity3d.com/questions/26676/can-you-make-a-guibox-follow-a-players-positionsol.html for the help
            Vector3 position = Camera.main.WorldToScreenPoint(transform.position);
            position.y = Screen.height - position.y;

            //Create a GUIBox and display it above where the attached gameobject is
            GUIStyle myBoxStyle = new GUIStyle(GUI.skin.box);
            myBoxStyle.normal.textColor = Color.white;
            myBoxStyle.wordWrap = true;
            myBoxStyle.fontSize = 15;

            GUI.Box(new Rect(position.x - 10, position.y - 40, popupWidth, popupHeight), popupText, myBoxStyle);
        }
    }


}
