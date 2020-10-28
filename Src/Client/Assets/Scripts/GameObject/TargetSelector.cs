using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TargetSelector:MonoSingleton<TargetSelector>
{
    Projector projector;
    bool actived = false;
    Vector3 center;
    private float range;
    private float size;
    Vector3 offset = new Vector3(0f, 2f, 0f);
    protected Action<Vector3> selectPoint;
    protected override void OnStart()
    {
        projector = this.GetComponentInChildren<Projector>();
        projector.gameObject.SetActive(actived);
    }
    public void Active(bool active)
    {
        this.actived = active;
        if (projector == null) return;
        projector.gameObject.SetActive(this.actived);
        projector.orthographicSize = this.size * 0.5f;
    }
     void Update()
    {
        if (!actived) return;
        if (this.projector == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if(Physics.Raycast(ray,out hitInfo,100f,LayerMask.GetMask("Terrain")))
        {
            Vector3 hitPoint = hitInfo.point;
            Vector3 dist = hitPoint - this.center;
            if(dist.magnitude > this.range)
            {
                hitPoint = this.center + dist.normalized * this.range;
            }
            this.projector.gameObject.transform.position = hitPoint + offset;
            if(Input.GetMouseButtonDown(0))
            {
                this.selectPoint(hitPoint);
                this.Active(false);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            this.Active(false);
        }
      
    }
    public static void ShowSelector(Vector3Int center, float range, float size, Action<Vector3> onPostionSelected)
    {
        if (TargetSelector.Instance == null) return;
        TargetSelector.Instance.center = GameObjectTool.LogicToWorld(center);
        TargetSelector.Instance.range = GameObjectTool.LogicToWorld(range);
        TargetSelector.Instance.size = GameObjectTool.LogicToWorld(size);
        TargetSelector.Instance.selectPoint = onPostionSelected;
        TargetSelector.Instance.Active(true);

    }
}

