using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public GameObject body;
    public float sensitivityX;
    public float sensitivityY;
    public Transform cameraPoint;

    float rotX;
    float rotY;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        var mouseX = Input.GetAxisRaw("Mouse X") * 100 * sensitivityY;
        var mouseY = Input.GetAxisRaw("Mouse Y") * 100 * sensitivityX;

        rotX -= mouseY * Time.deltaTime;
        rotY += mouseX * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -90f, 90f);
    }

    private void LateUpdate()
    {
        body.transform.eulerAngles = new Vector3(0, rotY, 0);
        cameraPoint.eulerAngles = new Vector3(rotX, rotY, 0);
    }
}
