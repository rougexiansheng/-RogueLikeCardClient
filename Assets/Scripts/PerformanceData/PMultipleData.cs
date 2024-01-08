using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 要同時表演的資料
/// </summary>
public class PMultipleData : PerformanceData
{
    /// <summary>擊中特效資料 不演出</summary>
    public PShowParticleData hitParticleData;
    /// <summary>裡面所有表演會同時演出</summary>
    public List<PerformanceData> performanceDatas = new List<PerformanceData>();
    public void AddPShowParticle()
    {
        if (hitParticleData != null) performanceDatas.Add(hitParticleData);
    }
    public int repeatCount = 0;
}
