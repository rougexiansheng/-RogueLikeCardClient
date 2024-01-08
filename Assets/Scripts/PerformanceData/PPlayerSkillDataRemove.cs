using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPlayerSkillDataRemove : PerformanceData
{
    public List<int> indexs = new List<int>();
    /// <summary>Insert 只有insert 會使用到</summary>
    public List<ActorSkill> pushSkills = new List<ActorSkill>();
    public bool isOverFlow = false;
}