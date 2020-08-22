using Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public Collider minimapBoudingbox;
    private void Start()
    {
        MinimapManager.Instance.UpdateMinimap(minimapBoudingbox);
    }
    private void Update()
    {

    }
}
