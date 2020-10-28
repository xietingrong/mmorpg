using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class MainUIActivation : ActivationControlPlayable
{
    public bool active;
    private bool activation;
    public override void OnGraphStart(Playable palyable)
    {
        Debug.Log("playable.GetBehaviour()");
        if (UIMain.Instance == null) return;
        this.activation = UIMain.Instance.Show;
    }
    public override void OnGraphStop(Playable playable)
    {
        Debug.Log("OnGraphStop");
    }
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        Debug.Log("OnBehaviourPlay");
        if (UIMain.Instance == null) return;
        UIMain.Instance.Show = active;
    }
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (UIMain.Instance == null) return;
        if (this.postPlayback == PostPlaybackState.Active)
            UIMain.Instance.Show = true;
        else if (this.postPlayback == PostPlaybackState.Inactive)
            UIMain.Instance.Show = false;
        else if (this.postPlayback == PostPlaybackState.Revert)
            UIMain.Instance.Show = this.activation;
    }
    public override void PrepareFrame(Playable playable, FrameData info)
    {

    }
}
