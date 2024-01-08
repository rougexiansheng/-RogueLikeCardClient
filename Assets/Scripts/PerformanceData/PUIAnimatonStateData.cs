using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PUIAnimatonStateData: PerformanceData
{
    public UIBattle.UIAnimatonStateEnum stateEnum;
    public bool autoClose = true;
    [NonSerialized]
    public Sprite sprite;
}
