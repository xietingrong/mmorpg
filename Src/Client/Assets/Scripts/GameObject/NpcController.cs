using Common.Data;
using Managers;
using Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public  class NpcController: MonoBehaviour
{
    public int npcID;
    SkinnedMeshRenderer render;
    Animator anim;
    NpcDefine npc;
    Color orignColor;
    private bool inInteractive = false;
    NpcQuestStatus questStatus;
    void Start()
    {
        render = this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        anim = this.gameObject.GetComponent<Animator>();
        npc = NpcManager.Instance.GetNpcDefine(npcID);
        NpcManager.Instance.UpdateNpcPostion(this.npcID, this.transform.position);
        orignColor = render.sharedMaterial.color;
        this.StartCoroutine(Actions());
        RefreshNpcStatus();
        QuestManager.Instance.onQuestStatusChanged += onQuestStatusChanged;
    }

    private void RefreshNpcStatus()
    {
        questStatus = QuestManager.Instance.GetQuestStatusByNpc(this.npcID);
        UIWorldElementManager.Instance.AddNpcQuestStatus(this.transform,questStatus);
    }
    void onQuestStatusChanged(Quest quest)
    {
        this.RefreshNpcStatus();
    }
    private void OnDestroy()
    {
        QuestManager.Instance.onQuestStatusChanged -= onQuestStatusChanged;
        if(UIWorldElementManager.Instance!= null)
           UIWorldElementManager.Instance.RemoveNpcQuestStatus(this.transform);
    }

    IEnumerator Actions()
    {
        while(true)
        {
            if (inInteractive)
                yield return new WaitForSeconds(2f);
            else
                yield return new WaitForSeconds(UnityEngine.Random.Range(5f,10f));
            this.Relax();
        }
    }
    void Interactive()
    {
        if (!inInteractive)
        {
            inInteractive = true;
            this.StartCoroutine(DoInteractive());
        }
    }

    IEnumerator DoInteractive()
    {
        yield return FaceTolayer();
        if( NpcManager.Instance.Interactive(npc))
        {
            anim.SetTrigger("Talk");
        }
        yield return new WaitForSeconds(3f);
        inInteractive = false;

    }

    IEnumerator FaceTolayer()
    {
        Vector3 faceTo = (User.Instance.CurrentCharacterObject.transform.position - this.transform.position).normalized;
        while(Mathf.Abs(Vector3.Angle(this.gameObject.transform.forward,faceTo))> 5)
        {
            this.gameObject.transform.forward = Vector3.Lerp(this.gameObject.transform.forward, faceTo, Time.deltaTime * 5f);
            yield return null;
        }
    }
    private void OnMouseDown()
    {
        if(Vector3.Distance(this.transform.position,User.Instance.CurrentCharacterObject.transform.position)>2f)
        {
            User.Instance.CurrentCharacterObject.StartNav(this.transform.position);
        }
        Interactive();
    }
    private void Relax()
    {
        anim.SetTrigger("Relax");
    }

    void Update()
    {
        
    }
  
    private void OnMouseOver()
    {
        Highlight(true);
    }
    private void OnMouseEnter()
    {
        Highlight(true);
    }
    private void OnMouseExit()
    {
        Highlight(false);
    }
    void Highlight(bool highlight)
    {
        if(highlight)
        {
            if (render.sharedMaterial.color != Color.white)
                render.sharedMaterial.color = Color.white;
        }
        else
        {
            if (render.sharedMaterial.color != orignColor)
                render.sharedMaterial.color = orignColor;
        }
    }
}
