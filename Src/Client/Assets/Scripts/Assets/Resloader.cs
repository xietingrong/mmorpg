using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Resloader
{
    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);
    }
}