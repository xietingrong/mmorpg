using Managers;
using SkillBridge.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIArenacs:MonoSingleton<UIArenacs>
{
    public Text roundText;
    public Text countDownText;
    protected override void OnStart()
    {
        countDownText.enabled = false;
        roundText.enabled = false;
        ArenaManager.Instance.SendReady();
    }
    public void ShowCountDown()
    {
        StartCoroutine(CountDown(10));
    }

    IEnumerator CountDown(int seconds)
    {
        int total = seconds;
        roundText.text = "ROUND" + ArenaManager.Instance.Round;
        roundText.enabled = true;
        countDownText.enabled = true;
        while(total >0)
        {
            SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_CountDown);
            countDownText.text = total.ToString();
            yield return new WaitForSeconds(1f);
            total--;
        }
        countDownText.text = "READY";
    }
     void Update()
    {
       
    }
    internal void ShowRoundStart(int round,ArenaInfo arenaInfo)
    {
        countDownText.text = "FIGHT";
    }
    internal void ShowRoundResult(int round, ArenaInfo arenaInfo)
    {
        countDownText.enabled = false;
        countDownText.text = "YOU WIN";
    }
}

