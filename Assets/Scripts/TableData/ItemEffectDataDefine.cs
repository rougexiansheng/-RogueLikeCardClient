using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEffectDataDefine
{
    public int id;
    public ItemEffectTypeDefine effect;
}


public class ItemEffectTypeDefine
{
    public ItemEffectTypeEnum type;

    public int Arg1;
    public int Arg2;
    public int Arg3;
}



public class ItemEffectOriginalDefine : OriginDefineBase<ItemEffectDataDefine>
{
    public int effectType;

    public int arg1;
    public int arg2;
    public int arg3;

    public override ItemEffectDataDefine ParseData()
    {
        var data = new ItemEffectDataDefine();
        data.id = id;
        data.effect = new ItemEffectTypeDefine()
        {
            type = (ItemEffectTypeEnum)effectType,
            Arg1 = arg1,
            Arg2 = arg2,
            Arg3 = arg3,
        };
        return data;
    }
}

