using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataDefine
{
    public int id;
    public string name;
    public Sprite icon;
    public ItemTpyeEnum itemType;
    public int arg;
    public int coinPrice;
    public int mallPrice;
    public string comment;
}

public class ItemOriginDataDefine : OriginDefineBase<ItemDataDefine>
{
    public string name;
    public string icon;
    public int itemType;
    public int arg;
    public int coinPrice;
    public int mallPrice;
    public string comment;
    public override ItemDataDefine ParseData()
    {
        var d = new ItemDataDefine();
        d.id = id;
        d.name = name;
        d.itemType = (ItemTpyeEnum)itemType;
        d.arg = arg;
        d.coinPrice = coinPrice;
        d.mallPrice = mallPrice;
        d.comment = comment;
        return d;
    }
}
