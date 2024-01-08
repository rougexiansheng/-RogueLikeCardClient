using System.Collections.Generic;
using UnityEngine;

public class MonsterAIOriginDefine : OriginDefineBase<MonsterAIDefine>
{
    public int consideration1;
    public int arg1;
    public int skillId1;

    public int consideration2;
    public int arg2;
    public int skillId2;

    public int consideration3;
    public int arg3;
    public int skillId3;

    public int consideration4;
    public int arg4;
    public int skillId4;

    public int consideration5;
    public int arg5;
    public int skillId5;

    public int consideration6;
    public int arg6;
    public int skillId6;

    public int consideration7;
    public int arg7;
    public int skillId7;

    public int consideration8;
    public int arg8;
    public int skillId8;

    public int consideration9;
    public int arg9;
    public int skillId9;

    public int consideration10;
    public int arg10;
    public int skillId10;

    public override MonsterAIDefine ParseData()
    {
        var d = new MonsterAIDefine();
        d.id = id;
        var t = GetType();
        for (int i = 1; i <= 100; i++)
        {
            var fC = t.GetField("consideration" + i);
            var fArg = t.GetField("arg" + i);
            var fSkill = t.GetField("skillId" + i);
            if (fC == null || fArg == null || fSkill == null)
            {
                //Debug.LogWarning($"id:{id} consideration{i}:{fC} arg{i}:{fArg} skillId{i}:{fSkill} is null");
                break;
            }
            var conData = new AIConsideration();
            conData.consideration = (AIConsiderationEnum)fC.GetValue(this);
            conData.arg = (int)fArg.GetValue(this);
            conData.skillId = (int)fSkill.GetValue(this);
            if (i == 1 && (conData.consideration == AIConsiderationEnum.None))
            {
                Debug.LogWarning($"id:{id} consideration{i} is None");
                break;
            }
            d.aIConsiderations.Add(conData);
        }
        return d;
    }
}

public struct AIConsideration
{
    public AIConsiderationEnum consideration;
    public int arg;
    public int skillId;
}

public class MonsterAIDefine
{
    public int id;
    public List<AIConsideration> aIConsiderations = new List<AIConsideration>();
}
