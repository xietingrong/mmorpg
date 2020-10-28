using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    public float liftTime = 1f;
    float time = 0;
    EffectType type;
    Transform target;
    Vector3 targetPos;
    Vector3 startPos;
    Vector3 offset;
    void OnEnable()
    {
        if (type != EffectType.Bullet)
        {
            StartCoroutine(Run());
        }
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(this.liftTime);
        this.gameObject.SetActive(false);
    }
    internal void Init(EffectType type, Transform source, Transform target,Vector3 offset, float duration)
    {
        this.type = type;
        this.target = target;
        if (duration>0)
            this.liftTime = duration;
        this.time = 0;
        if (type == EffectType.Bullet)
        {
            this.startPos = this.transform.position;
            this.offset = offset;
            this.targetPos = target.position + offset;
        }
        else if( type == EffectType.Hit)
        {
            this.transform.position = target.position + offset;
        }
        
    }
    void Update()
    {
        if(type == EffectType.Bullet)
        {
            this.time += Time.deltaTime;
            if(this.target!= null)
            {
                this.targetPos = this.target.position + this.offset;

            }
            this.transform.LookAt(this.targetPos);
            if (Vector3.Distance(this.targetPos,this.transform.position)<0.5f)
            {
                Destroy(this.gameObject);
                return;
            }
            if(this.liftTime >0 && this.time >= this.liftTime)
            {
                Destroy(this.gameObject);
                return;
            }
            this.transform.position = Vector3.Lerp(this.transform.position, this.targetPos, Time.deltaTime / (this.liftTime - this.time));
        }
    }
}

