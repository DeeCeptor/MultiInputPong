using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeviceSpecificText : MonoBehaviour
{
    public string controller_text;
    public string mouse_text;
    public string drawing_tablet_text;
    public string touchscreen_text;
    Text text;

    void Start ()
    {
        text = this.GetComponent<Text>();

    }


    void Update ()
    {
		switch (GlobalSettings.current_input_device)
        {
            case GlobalSettings.Input_Device_Type.Controller:
                text.text = controller_text;
                break;
            case GlobalSettings.Input_Device_Type.Mouse:
                text.text = mouse_text;
                break;
            case GlobalSettings.Input_Device_Type.Drawing_Tablet:
                text.text = drawing_tablet_text;
                break;
            case GlobalSettings.Input_Device_Type.Touchscreen:
                text.text = touchscreen_text;
                break;
        }
	}
}
