using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UICreatureInfo : MonoBehaviour
{
    public Text Name;
    public Slider Hpbar;
    public Slider Mpbar;
    public Text HPText;
    public Text MPText;
    public UIBuffIcons buffIcons;
    void Start()
    {
        
    }
    private Creature target;
    public Creature Target
    {
        get { return target; }
        set
        {
            this.target = value;
            buffIcons.SetOwner(value);
            this.UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (this.target == null) return;
        this.Name.text = string.Format("{0} lV.{1}", target.Name, target.Info.Level);

        this.Hpbar.maxValue = target.Attributes.MaxHP;
        this.Hpbar.value = target.Attributes.HP;
        this.HPText.text = string.Format("{0}/{1}", target.Attributes.HP, target.Attributes.MaxHP);

        this.Mpbar.maxValue = target.Attributes.MaxMp;
        this.Mpbar.value = target.Attributes.MP;
        this.MPText.text = string.Format("{0}/{1}", target.Attributes.MP, target.Attributes.MaxMp);

    }
    void Update()
    {
        this.UpdateUI();
    }
}

