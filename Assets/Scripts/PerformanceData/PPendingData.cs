using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPendingData: PerformanceData
{
    [NonSerialized]
    public Func<UniTask> taskFunc;
}
