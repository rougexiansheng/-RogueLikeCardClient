using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPlayerSkillDataMove : PerformanceData
{
    /// <summary>是否為前進或後退</summary>
    public bool isPush = false;
    /// <summary>前進或後退的技能</summary>
    public List<ActorSkill> pushSkills = new List<ActorSkill>();
}