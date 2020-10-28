using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class FXManager : MonoSingleton<FXManager>
{
    public GameObject[] prefabs;
    private Dictionary<string, GameObject> Effects = new Dictionary<string, GameObject>();
    protected override void OnStart()
    {
        for(int i=0; i<prefabs.Length;i++)
        {
            prefabs[i].SetActive(false);
            this.Effects[this.prefabs[i].name] = this.prefabs[i];
        }
    }
    EffectController CreateEffect(string name,Vector3 pos)
    {
        GameObject prefab;
        if(this.Effects.TryGetValue(name,out prefab))
        {
            GameObject go = Instantiate(prefab, FXManager.Instance.transform, true);
            go.transform.position = pos;
            return go.GetComponent<EffectController>();
        }
        return null;
    }
    internal void PlayEffect(EffectType type,string name,Transform target,Vector3 pos,float duration)
    {
        EffectController effect = FXManager.Instance.CreateEffect(name, pos);
        if(effect == null)
        {
            Debug.LogErrorFormat("Effect：{0} not Found",name);
        }
        effect.Init(type, this.transform, target, pos, duration);
        effect.gameObject.SetActive(true);
    }
}

