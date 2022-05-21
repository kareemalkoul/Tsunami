using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    bool mouse;
    // Start is called before the first frame update
    void Start()
    {
        mouse = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        if (mouse)
        {
            Cursor.visible = false;
            // Cursor.lockState = CursorLockMode.Confined;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        // keep confined to center of screen

        if (IsMouseOverGameWindow & mouse)
        {
            Vector3 rotation = Vector3.zero;
            float rotationScale = 15.0f;
            rotation.y = Input.GetAxis("Mouse X");
            rotation.x = -Input.GetAxis("Mouse Y");
            rotation.y = Mathf.Clamp(rotation.y, -60.0f, 60.0f);
            transform.eulerAngles += rotationScale * rotation / 2;
        }

        Vector3 translation = Vector3.zero;
        float translationScale = 5.0f;
        if (Input.GetKey("w"))
            translation += Vector3.forward * Time.deltaTime;
        if (Input.GetKey("s"))
            translation += Vector3.back * Time.deltaTime;
        if (Input.GetKey("a"))
            translation += Vector3.left * Time.deltaTime;
        if (Input.GetKey("d"))
            translation += Vector3.right * Time.deltaTime;

        transform.Translate(translationScale * translation);
    }


    bool IsMouseOverGameWindow
    {
        get
        {
            return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y
        || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y);
        }
    }
}
