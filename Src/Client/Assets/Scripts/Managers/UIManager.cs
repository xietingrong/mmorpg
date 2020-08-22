using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    class UIElement
    {
        public string Resources;
        public bool Cashe;
        public GameObject Instance;
    }
    private Dictionary<Type, UIElement> UIResources = new Dictionary<Type, UIElement>();
    public UIManager()
    {
        this.UIResources.Add(typeof(UIText), new UIElement() { Resources = "Ui/UIText", Cashe = true });
        this.UIResources.Add(typeof(UIBag), new UIElement() { Resources = "Ui/UIBag", Cashe = true });
        this.UIResources.Add(typeof(UIShop), new UIElement() { Resources = "Ui/UIShop", Cashe = true });
        this.UIResources.Add(typeof(UICharEquip), new UIElement() { Resources = "Ui/UICharEquip", Cashe = true });
        this.UIResources.Add(typeof(UIQuestSystem), new UIElement() { Resources = "Ui/UIQuestSystem", Cashe = true });
        this.UIResources.Add(typeof(UIQuestDialog), new UIElement() { Resources = "Ui/UIQuestDialog", Cashe = true });
        //this.UIResources.Add(typeof(UIRide), new UIElement() { Resources = "Ui/UIRide", Cashe = true });
    }

    ~UIManager()
    {

    }
    public T Show<T>()
    {
        SoundManager.Instance.PlaySound("ui_open");
        Type type = typeof(T);
        if(this.UIResources.ContainsKey(type))
        {
            UIElement info = this.UIResources[type];
            if(info.Instance!=null)
            {
                info.Instance.SetActive(true);
            }
            else
            {
                UnityEngine.Object prefab = Resources.Load(info.Resources);
                if(prefab ==null)
                {
                    return default(T);
                }
                info.Instance = (GameObject)GameObject.Instantiate(prefab);
            }
            return info.Instance.GetComponent<T>();
        }
        return default(T);
    }
    public void Close(Type type)
    {
        SoundManager.Instance.PlaySound("ui_close");
        if (this.UIResources.ContainsKey(type))
        {
            UIElement info = this.UIResources[type];
            if(info.Cashe)
            {
                info.Instance.SetActive(false);
            }
            else
            {
                GameObject.Destroy(info.Instance);
                info.Instance = null;
            }
        }
    }
    public void Close<T>()
    {
        this.Close(typeof(T));
    }
}


