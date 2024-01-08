using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfessionSkinDataDefine
{
    public int id;
    public ActorProfessionEnum professionId;
    public Sprite icon;
    public string skinName;
    public string comment;
}

public class ProfessionSkinOriginDataDefine : OriginDefineBase<ProfessionSkinDataDefine>
{
    public int professionId;
    public string icon;
    public string skinName;
    public string comment;
    public override ProfessionSkinDataDefine ParseData()
    {
        var d = new ProfessionSkinDataDefine();
        d.id = id;
        d.professionId = (ActorProfessionEnum)professionId;
        d.skinName = skinName;
        d.comment = comment;
        return d;
    }
}