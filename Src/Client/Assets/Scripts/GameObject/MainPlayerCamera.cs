using Common.Data;
using Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayerCamera : MonoSingleton<MainPlayerCamera>
{
    public Camera camera;
    public Transform viewPoint;

    public GameObject player;

    public float follSpeed = 1f;
    public float rotateSpeed = 5f;
    Quaternion yaw = Quaternion.identity;

    private void LateUpdate()
    {
        if (player == null && User.Instance.CurrentCharacterObject != null)
        {
            player = User.Instance.CurrentCharacterObject.gameObject;
        }
    
        this.transform.gameObject.SetActive( User.Instance.CurrentMapData.Type != MapType.Arena);
      
        if (player == null)
            return;

        //this.transform.position = player.transform.position;
        //this.transform.rotation = player.transform.rotation;
        this.transform.position = Vector3.Lerp(this.transform.position, player.transform.position, Time.deltaTime * follSpeed);
        if (Input.GetMouseButton(1))
        {
            Vector3 anglebase = this.transform.localRotation.eulerAngles;
            this.transform.localRotation = Quaternion.Euler(anglebase.x - Input.GetAxis("Mouse Y") * rotateSpeed, anglebase.y + Input.GetAxis("Mouse X") * rotateSpeed, 0);
            Vector3 angle = this.transform.rotation.eulerAngles - player.transform.rotation.eulerAngles;
            angle.z = 0;
            yaw = Quaternion.Euler(angle);
        }
        else
        {
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, player.transform.rotation * yaw, Time.deltaTime * follSpeed );
        }
        if (Input.GetAxis("Vertical") > 0.01)
        {
            yaw = Quaternion.Lerp(yaw, Quaternion.identity, Time.deltaTime * follSpeed);
        }
    }
}
