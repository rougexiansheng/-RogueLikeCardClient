using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class DungeonListDefine
{
    public int id;
    public MapType mapType;
    public int totalLayer;
    public string name;
    public Sprite icon;
    public int firstReward;

    public int passReward;

}

public class DungeonOriginListDefine : OriginDefineBase<DungeonListDefine>
{
    public int mapType;
    public int totalLayer;

    public string name;
    public string icon;
    public int firstReward;
    public int passReward;


    public override DungeonListDefine ParseData()
    {
        var d = new DungeonListDefine();
        d.id = id;
        d.totalLayer = totalLayer;
        d.name = name;
        d.mapType = (MapType)mapType;

        return d;
    }
}

