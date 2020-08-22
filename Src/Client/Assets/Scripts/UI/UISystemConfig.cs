using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UISystemConfig:UIWindow
{
    public Image musicOff;
    public Image soundOff;
    public Toggle toggleMusic;
    public Toggle toggleSound;
    public Slider sliderMusic;
    public Slider sliderSound;
    private void Start()
    {
        this.toggleMusic.isOn = Config.MusicOn;
        this.toggleMusic.isOn = Config.SoundOn;
        this.sliderMusic.value = Config.MusicVolum;
        this.sliderSound.value = Config.SoundVolum;
    }
    public override void OnYesClick()
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
        PlayerPrefs.Save();
        base.OnYesClick();
    }

    public void MusicToggle(bool on)
    {
        musicOff.enabled = !on;
        Config.MusicOn = on;
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }
    public void SoundToggle(bool on)
    {
        soundOff.enabled = !on;
        Config.SoundOn = on;
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }

    public void MusicVolum(float vol)
    {
        Config.MusicVolum = (int)vol;
        PlaySound();
    }
    public void SoundVolum(float vol)
    {
        Config.SoundVolum = (int)vol;
        PlaySound();
    }
    float lastPlay;
    private void PlaySound()
    {
        if(Time.realtimeSinceStartup -lastPlay>0.1)
        {
            lastPlay = Time.realtimeSinceStartup;
            SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
        }
    }
}

