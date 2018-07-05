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

    public static Vector2 screen_center;   // Center of screen in pixel coordinates


    Camera cam;


    void Awake () 
	{
        cam = this.GetComponent<Camera>();
        camera_settings = this;
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

        arena_rect = camera_rect;

        screen_center = cam.WorldToScreenPoint(Vector2.zero);
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


    // Returns true if the point is within the game bounds
    public bool PointWithinCameraBounds(Vector2 point)
    {
        return camera_rect.Contains(point);
    }
}
