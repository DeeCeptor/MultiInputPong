using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Menu : MonoBehaviour 
{
    public Button button_to_enable;


    private void Start()
    {
        Cursor.visible = true;
    }


    // Item 1 is 0
    public void DeviceOrderChanged(int item_selected)
    {
        GlobalSettings.order_of_device_types.Clear();

        Debug.Log("Changing device order to: " + item_selected);

        switch (item_selected)
        {
            case 0:
                return;
            case 1:
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Mouse);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Drawing_Tablet);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Touchscreen);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Controller);
                break;
            case 2:
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Controller);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Mouse);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Drawing_Tablet);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Touchscreen);
                break;
            case 3:
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Touchscreen);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Controller);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Mouse);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Drawing_Tablet);
                break;
            case 4:
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Drawing_Tablet);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Touchscreen);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Controller);
                GlobalSettings.order_of_device_types.Enqueue(GlobalSettings.Input_Device_Type.Mouse);
                break;
        }

        button_to_enable.interactable = true;
    }


    public void LoadScene(string scene)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }


    public void Quit()
    {
        Application.Quit();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Quit();
    }
}
