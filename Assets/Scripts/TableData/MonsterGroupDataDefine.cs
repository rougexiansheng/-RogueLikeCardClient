using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGroupDataDefine
{
    public int id;
    public List<MonsterGroupData> groupDatas = new List<MonsterGroupData>();
}

public struct MonsterGroupData
{
    public int monsterId;
    public int possibility;
}

public class MonsterGroupOriginDataDefine : OriginDefineBase<MonsterGroupDataDefine>
{
    public int monsterId1;
    public int possibility1;

    public int monsterId2;
    public int possibility2;

    public int monsterId3;
    public int possibility3;

    public int monsterId4;
    public int possibility4;

    public int monsterId5;
    public int possibility5;

    public int monsterId6;
    public int possibility6;

    public int monsterId7;
    public int possibility7;

    public int monsterId8;
    public int possibility8;

    public int monsterId9;
    public int possibility9;

    public int monsterId10;
    public int possibility10;

    public int monsterId11;
    public int possibility11;

    public int monsterId12;
    public int possibility12;

    public int monsterId13;
    public int possibility13;

    public int monsterId14;
    public int possibility14;
    public override MonsterGroupDataDefine ParseData()
    {
        var d = new MonsterGroupDataDefine();
        d.id = id;
        var t = typeof(MonsterGroupOriginDataDefine);
        for (int i = 1; i <= 100; i++)
        {
            var fmId = t.GetField("monsterId" + i);
            var fp = t.GetField("possibility" + i);
            if (fmId == null || fp == null)
            {
                //Debug.LogWarning($"id:{id} monsterId{i}:{fmId} possibility{i}:{fp} is null");
                break;
            }
            var gData = new MonsterGroupData();
            gData.monsterId = (int)fmId.GetValue(this);
            gData.possibility = (int)fp.GetValue(this);

            if (i == 1 && (gData.monsterId == 0 || gData.possibility == 0))
            {
                Debug.LogWarning($"id:{id} first monsterId{i}:{gData.monsterId} possibility{i}:{gData.possibility} is 0");
                break;
            }
            d.groupDatas.Add(gData);
        }
        return d;
    }
}
