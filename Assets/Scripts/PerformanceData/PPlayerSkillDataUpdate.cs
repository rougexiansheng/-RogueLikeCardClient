using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPlayerSkillDataUpdate : PerformanceData
{
    public enum UpdateTypeEnum
    {
        /// <summary>更新是否使用過</summary>
        Use,
        /// <summary>更新能量是否充足</summary>
        Cost,
    }
    public UpdateTypeEnum updateEnum;
    public List<int> skillIds = new List<int>();
    public List<int> indexs = new List<int>();
}