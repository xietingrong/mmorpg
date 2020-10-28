using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AnimationEventController :MonoBehaviour 
{
    public EntityEffectManager EffectMgr;
    void PlayEffect(string name)
    {
        Debug.LogFormat(" AnimationEventController:PlayEffect:{0}:{1}", this.name, name);
        EffectMgr.PlayEffect(name);
    }
    void PlaySound(string name)
    {
        Debug.LogFormat(" AnimationEventController:PlaySound:{0}:{1}", this.name, name);
    }
}

