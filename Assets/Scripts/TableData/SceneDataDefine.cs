using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneDataDefine
{
    public int id;
    public List<string> sceneNames;
    public List<GameObject> sceneObj = new List<GameObject>();
    public GameObject sceneBoss;
    public Sprite sceneNameSprite;
    public MAY.SceneSettings sceneSetting;
}
public class SceneDataOriginDefine : OriginDefineBase<SceneDataDefine>
{
    public string sceneNames;
    public string bossSceneName;
    public string prefix;
    public string textureName;
    public override SceneDataDefine ParseData()
    {
        var d =new SceneDataDefine();
        d.id = id;
        d.sceneNames = sceneNames.Split(',').ToList();
        return d;
    }
}
