using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class fadePlaceName : MonoBehaviour {

    public bool toggleGUI;
    public CanvasGroup canvas;
    public Text placeNameText;
    public string nameText;

    void Start()
    {
        toggleGUI = false;
    }


    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && other.GetComponent<CapsuleCollider>().GetType() == typeof(CapsuleCollider))
        {
            toggleGUI = true;
            SetPlaceName();
            fadeGUI();
        }
    }
  

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player" && other.GetComponent<CapsuleCollider>().GetType() == typeof(CapsuleCollider))
        {
            toggleGUI = false;
            fadeGUI();
        }
        
    }

    void fadeGUI()
    {
        //If the toggleGUI is true, fade the ui into the screen
        if (toggleGUI)
        {
            StartCoroutine(BeginFade());
        }else
        {
            //Fade the UI out, we no longer need it
            StartCoroutine(EndFade());
        }
        
    }


    //Slowly changes the alpha of the canvasGroup component from 1 to 0
    IEnumerator EndFade()
    {
        
        while(canvas.alpha > 0)
        {
            canvas.alpha -= Time.deltaTime;
            yield return null;
        }
    }

    //Slowly changes the alpha of the canvasGroup component from 0 to 1
    IEnumerator BeginFade()
    {
        while(canvas.alpha < 1)
        {
            canvas.alpha += Time.deltaTime * 2;
            yield return null;
        }
    }

    void SetPlaceName()
    {
        placeNameText.text = nameText;
    }
}
