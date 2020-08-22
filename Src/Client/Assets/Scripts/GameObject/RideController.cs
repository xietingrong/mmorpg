using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RideController: MonoBehaviour
{
    public Transform moutPoint;
    public EntityController rider;
    public Vector3 offset;
    private Animator anim;
    private void Start()
    {
        this.anim = this.GetComponent<Animator>();
    }
    private void Update()
    {
        if (this.moutPoint == null || this.rider == null) return;
        this.rider.SetRidePotision(this.moutPoint.position + this.moutPoint.TransformDirection(this.offset));
    }
    public void SetRider(EntityController rider)
    {
        this.rider = rider;
    }
    public void OnEntityEvent(EntityEvent entityEvent,int param)
    {
        switch(entityEvent)
        {
            case EntityEvent.Idle:
                anim.SetBool("Move", false);
                anim.SetTrigger("Idle");
                break;
            case EntityEvent.MoveFwd:
                anim.SetBool("Move", true);
                break;
            case EntityEvent.MoveBack:
                anim.SetBool("Move", true);
                break;
            case EntityEvent.Jump:
                anim.SetTrigger("Jump");
                break;
        }
    }

}

