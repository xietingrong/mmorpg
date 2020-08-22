using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TextBoxEnter : MonoBehaviour
{
    private void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("触发碰撞");
    }
}

