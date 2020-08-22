using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Config
{
    public static bool MusicOn
    {
        get
        {
            return PlayerPrefs.GetInt("Music", 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("Music", value?1:0) ;
            SoundManager.Instance.MusicOn = value;
        }
    }
    public static bool SoundOn
    {
        get
        {
            return PlayerPrefs.GetInt("Sound", 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("Sound", value ? 1 : 0);
            SoundManager.Instance.SoundOn = value;
        }
    }
    public static int MusicVolum
    {
        get
        {
            return PlayerPrefs.GetInt("MusicVolum", 100) ;
        }
        set
        {
            PlayerPrefs.SetInt("MusicVolum", value );
            SoundManager.Instance.MusicVolum = value;
        }
    }
    public static int SoundVolum
    {
        get
        {
            return PlayerPrefs.GetInt("SoundVolum", 100) ;
        }
        set
        {
            PlayerPrefs.SetInt("Sound", value);
            SoundManager.Instance.SoundVolum = value;
        }
    }

}


