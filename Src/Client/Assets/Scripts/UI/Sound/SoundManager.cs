using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoSingleton<SoundManager>
{
    public AudioMixer audioMixer;
    public AudioSource musicAudioSource;
    public AudioSource soundAudioSource;
    const string MusicPath = "Music/";
    const string SoundPath = "Sound/";
    private bool musicOn;
    public bool MusicOn
    {
        get { return musicOn; }
        set
        {
            musicOn = value;
            this.MusicMute(!musicOn);
        }
    }
    private bool soundOn;
    public bool SoundOn
    {
        get { return soundOn; }
        set
        {
            soundOn = value;
            this.SoundMute(!soundOn);
        }
    }

    private int musicVolum;
    public int MusicVolum
    {
        get { return musicVolum; }
        set
        {
            musicVolum = value;
            this.MusicMute(!soundOn);
        }
    }
    private int soundVolum;
    public int SoundVolum
    {
        get { return soundVolum; }
        set
        {
            soundVolum = value;
            this.SoundMute(!soundOn);
        }
    }


    private void MusicMute(bool mute)
    {
        this.SetVloum("MusicVolume", mute ? 0 : musicVolum);
    }

    private void SetVloum(string name, int value)
    {
        float volume = value * 0.5f - 50f;
        this.audioMixer.SetFloat(name, volume);
    }

    private void SoundMute(bool mute)
    {
        this.SetVloum("SoundVolume", mute ? 0 : soundVolum);
    }
    public void PlayMusic(string name)
    {
        AudioClip clip = Resloader.Load<AudioClip>(MusicPath + name);
        if(clip == null)
        {
            Debug.LogWarningFormat("PlayMusic:{0} not existed", name);
            return;
        }
        if(musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
        }
        musicAudioSource.clip = clip;
        musicAudioSource.Play();
    }
    public void PlaySound(string name)
    {
        AudioClip clip = Resloader.Load<AudioClip>(SoundPath + name);
        if (clip == null)
        {
            Debug.LogWarningFormat("PlaySound:{0} not existed", name);
            return;
        }

        soundAudioSource.PlayOneShot(clip);
       
    }
}

