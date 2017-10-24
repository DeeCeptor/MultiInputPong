using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAfter : MonoBehaviour
{
    public GameObject object_to_activate;

    public bool acivate_on_start = false;
    public float activate_after_x_seconds = 0;



    void Start ()
    {
        if (acivate_on_start)
            ActivateObject();
    }


    void Update ()
    {
        activate_after_x_seconds -= Time.deltaTime;
        if (activate_after_x_seconds <= 0)
            ActivateObject();
    }

    public void ActivateObject()
    {
        object_to_activate.SetActive(true);
        Destroy(this);
    }
}
