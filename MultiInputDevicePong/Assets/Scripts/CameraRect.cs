using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRect : MonoBehaviour 
{
    public static CameraRect camera_settings;

    public static float screenAspect;     // Aspect ratio, regardless of camera.
    public static float camWidth;   // In in-game units
    public static float camHeight;  // In in-game units

    public static Rect camera_rect;
    public static Rect arena_rect;

    public GameObject topright;
    public GameObject bottomleft;

    Camera cam;


    void Awake () 
	{
        cam = this.GetComponent<Camera>();

        CalculateScreenDimensions();
    }
    private void Start()
    {
        var bottomLeft = cam.ScreenToWorldPoint(Vector3.zero);
        var topRight = cam.ScreenToWorldPoint(new Vector3(
            cam.pixelWidth, cam.pixelHeight));

        camera_rect = new Rect(bottomLeft.x,
                                bottomLeft.y,
                                topRight.x - bottomLeft.x,
                                topRight.y - bottomLeft.y);

        arena_rect = new Rect(bottomleft.transform.position.x,
                                bottomleft.transform.position.y,
                                topright.transform.position.x - bottomleft.transform.position.x,
                                topright.transform.position.y - bottomleft.transform.position.y);
    }


    public void CalculateScreenDimensions()
    {
        screenAspect = (float)Screen.width / (float)Screen.height;
        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = screenAspect * camHalfHeight;
        camWidth = 2.0f * camHalfWidth;
        camHeight = camHalfHeight * 2;

        Debug.Log("Width: " + camWidth + " Height: " + camHeight);
    }
}
