using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[NotKeyable]
public class MainUIActive:PlayableAsset
{
    public bool active = true;
    public ActivationControlPlayable.PostPlaybackState postPlayback;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var playable = ScriptPlayable<MainUIActivation>.Create(graph);
        playable.GetBehaviour().active = active;
        playable.GetBehaviour().postPlayback = postPlayback;
        return playable;
    }
}

