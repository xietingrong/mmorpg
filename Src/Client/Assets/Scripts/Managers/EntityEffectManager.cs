using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EntityEffectManager : MonoBehaviour
{
    public Transform Root;
    private Dictionary<string, GameObject> Effects = new Dictionary<string, GameObject>();
    public Transform[] Props;
    private void Start()
    {
        this.Effects.Clear();
        if(this.Root!= null&&this.Root.childCount >0)
        {
            for(int i =0; i< this.Root.childCount;i++)
            {
                this.Effects[this.Root.GetChild(i).name] = this.Root.GetChild(i).gameObject;
            }
        }
        if(Props!= null)
        {
            for (int i= 0; i < this.Props.Length;i++)
            {
                this.Effects[this.Props[i].name] = this.Props[i].gameObject;
            }
        }
    }

    internal void PlayEffect(string name)
    {
        Debug.LogFormat("PlayEffec:{0}:{1}", this.name, name);
        if(this.Effects.ContainsKey(name))
        {
            this.Effects[name].SetActive(true);
        }
    }
    internal void PlayEffect(EffectType type, string name, Transform target, Vector3 offset, float duration)
    {
        Debug.LogFormat("PlayEffec:{0}:{1}", this.name, name);
        if (type == EffectType.Bullet)
        {
            if (target == null) return;
            EffectController effect = InstantiateEffect(name);
            effect.Init(type, this.transform, target, offset, duration);
            effect.gameObject.SetActive(true);
        }
        else
        {
            PlayEffect(name);
        }
    }
    EffectController InstantiateEffect(string name)
    {
        GameObject prefab;
        if(this.Effects.TryGetValue(name,out prefab))
        {
            GameObject go = Instantiate(prefab, GameObjectManager.Instance.transform, true);
            go.transform.position = prefab.transform.position;
            go.transform.rotation = prefab.transform.rotation;
            return go.GetComponent<EffectController>();
        }
        return null;
    }
}

