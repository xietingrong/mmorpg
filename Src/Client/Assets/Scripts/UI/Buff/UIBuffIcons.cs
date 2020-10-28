using Battle;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIBuffIcons : MonoBehaviour
{
    Creature owner;
    public GameObject prefabBuff;
    public Dictionary<int,GameObject> buffs = new Dictionary<int, GameObject>();
    void Awake()
    {
        prefabBuff.SetActive(false);
    }
    void OnDestroy()
    {
        this.Clear();
    }
    public void SetOwner(Creature owner)
    {
        if(this.owner != null && this.owner!= owner)
        {
            this.Clear();
        }
        this.owner = owner;
        this.owner.OnBuffAdd += OnBuffAdd;
        this.owner.OnBuffRemove += OnBuffRemove;
        this.InitBuffs();
    }

    private void InitBuffs()
    {
        foreach (var buff in this.owner.BuffMgr.Buffs)
        {
            this.OnBuffAdd(buff.Value);
        }
    }

    private void Clear()
    {
       if(this.owner!= null)
        {
            this.owner.OnBuffAdd -= OnBuffAdd;
            this.owner.OnBuffRemove -= OnBuffRemove;
        }
       foreach (var buff in this.buffs)
        {
            Destroy(buff.Value);
        }
        this.buffs.Clear();
    }

    private void OnBuffRemove(Buff buff)
    {
        GameObject go;
        if(this.buffs.TryGetValue(buff.BuffId,out go))
        {
            this.buffs.Remove(buff.BuffId);
            Destroy(go);
        }
    }

    private void OnBuffAdd(Buff buff)
    {
        GameObject go = Instantiate(prefabBuff, this.transform);
        go.name = buff.Define.ID.ToString();
        UIBuffItem item = go.GetComponent<UIBuffItem>();
        item.SetItem(buff);
        go.SetActive(true);
        this.buffs[buff.BuffId] = go;
    }
}

