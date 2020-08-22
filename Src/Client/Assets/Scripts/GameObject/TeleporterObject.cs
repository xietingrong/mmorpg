using Common.Data;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TeleporterObject : MonoBehaviour
{
    public int ID;
    Mesh mesh= null;
    void Start()
    {
        this.mesh = this.GetComponent<MeshFilter>().sharedMesh;
    }
    private void Update()
    {
        
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if(this.mesh!= null)
        {
            Gizmos.DrawWireMesh(this.mesh, this.transform.position + Vector3.up * this.transform.localScale.y * .5f, this.transform.rotation, this.transform.localScale);
        }
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.ArrowHandleCap(0, this.transform.position, this.transform.rotation, 1f, EventType.Repaint);
    }
#endif
    void OnTriggerEnter(Collider other)
    {
        PlayerInputController playerInputController = other.GetComponent<PlayerInputController>();
        if(playerInputController!= null && playerInputController.isActiveAndEnabled)
        {
            TeleporterDefine td = DataManager.Instance.Teleporters[this.ID];
            if(td == null)
            {
                Debug.LogFormat("TeleporterObject: Character[{0}] Enter TeleporterObject[{1}],but TeleporterDefine not existed ", playerInputController.character.Info.Name, td.ID);
                return;
            }
            Debug.LogFormat("TeleporterObject: Character[{0}] Enter TeleporterObject[{1}:{2}]", playerInputController.character.Info.Name, td.ID, td.Name);
            if (td.LinkTo > 0)
            {
                if (DataManager.Instance.Teleporters.ContainsKey(td.LinkTo))
                    MapService.Instance.SendMapTeleport(this.ID);
                else
                    Debug.LogFormat("Teleporter ID:{0}:LinkID{1}error}", td.ID, td.LinkTo);
            }
        }
    }
}

