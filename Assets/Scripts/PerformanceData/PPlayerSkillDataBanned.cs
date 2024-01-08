using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPlayerSkillDataBanned : PerformanceData
{
    public enum BannedTypeEnum
    {
        /// <summary>開始封印</summary>
        Start,
        /// <summary>結束封印</summary>
        End,
    }
    public BannedTypeEnum bannedEnum;
    public List<int> indexs = new List<int>();
}