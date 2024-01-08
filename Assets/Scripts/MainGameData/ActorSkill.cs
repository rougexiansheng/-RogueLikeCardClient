using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActorSkill
{
    /// <summary>狀態ID</summary>
    public int skillId;
    /// <summary>是否已經使用過</summary>
    public bool isUsed;
    /// <summary>原始位置</summary>
    public int originIndex;
    /// <summary>是否已經使用過</summary>
    public bool isBanned;
}
