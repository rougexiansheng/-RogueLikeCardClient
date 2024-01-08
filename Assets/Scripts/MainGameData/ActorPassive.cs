using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActorPassive
{
    /// <summary>狀態ID</summary>
    public int passiveId;
    /// <summary>狀態層數</summary>
    public int currentStack;
    [NonSerialized]
    /// <summary>施放者</summary>
    public BattleActor sender;
    [NonSerialized]
    /// <summary>擁有者</summary>
    public BattleActor owner;
    public int keepCount = 0;
    /// <summary>被動效果異動前 才會知道 增加還是減少</summary>
    public bool isAdd;
    public ActorPassive Clone()
    {
        var p = new ActorPassive();
        p.passiveId = passiveId;
        p.currentStack = currentStack;
        p.sender = sender;
        p.owner = owner;
        return p;
    }
}
