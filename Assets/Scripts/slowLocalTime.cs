using UnityEngine;
using System.Collections;

public class slowLocalTime : MonoBehaviour {
    //public Rigidbody r = null;
    //private float _localTimeScale = 1.0f;
    //public float localTimeScale
    //{
    //    get
    //    {
    //        return _localTimeScale;
    //    }
    //    set
    //    {
    //        if (r == null) r = GetComponent<Rigidbody>();
    //        if (r != null)
    //        {
    //            float multiplier = value / _localTimeScale;
    //            r.angularDrag *= multiplier;
    //            r.drag *= multiplier;
    //            r.mass /= multiplier;
    //            r.velocity *= multiplier;
    //            r.angularVelocity *= multiplier;
    //        }

    //        _localTimeScale = value;
    //    }
    //}
    //public float localDeltaTime
    //{
    //    get
    //    {
    //        return Time.deltaTime * Time.timeScale * _localTimeScale;
    //    }
    //}

    //void FixedUpdate()
    //{
    //    // Counter gravity
    //    if (r == null) r = GetComponent<Rigidbody>();
    //    if (r != null)
    //    {
    //        r.AddForce(-Physics.gravity + (Physics.gravity * (_localTimeScale * _localTimeScale)), ForceMode.Acceleration);
    //    }
    //}
   
    //If the player enters the sphere and stays
    void OnTriggerStay(Collider other)
    {
       
        //If anything enters the sphere collider body slow time scale
        Time.timeScale = 0.5f;

        //To make object independant timescales change any collider objects deltatime

        //USE FOR PUSHING BLOCKS AROUND
        //float translation = Time.deltaTime * 10;
        //transform.Translate(0, 0, translation);

    }
    void OnTriggerExit(Collider other)
    {
        //When the sphere is exited, set the timescale back to normal
        Time.timeScale = 1.0f;
    }



}
