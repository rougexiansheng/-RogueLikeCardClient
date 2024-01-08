using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTestScript : MonoBehaviour
{
    [SerializeField]
    GameObject[] monsters;
    [SerializeField]
    Camera sceneAndUICam, monsterCam;
    [SerializeField]
    float monsterDis, camAndMonsterDis;
    [SerializeField]
    float rightLeftRad;
    [SerializeField]
    float camRad, eulerAngleY;
    [SerializeField]
    Transform bgObj;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetPos(monsters[0].transform, monsterDis, rightLeftRad);
        GetPos(monsters[1].transform, monsterDis, 0);
        GetPos(monsters[2].transform, monsterDis, -rightLeftRad);
        GetPos(sceneAndUICam.transform, monsterDis + camAndMonsterDis, camRad);
        GetPos(monsterCam.transform, monsterDis + camAndMonsterDis, camRad);
        for (int i = 0; i < monsters.Length; i++)
        {
            monsters[i].transform.rotation = monsterCam.transform.rotation;
        }
        var v = transform.position;
        monsterCam.transform.LookAt(v);
        v = monsterCam.transform.eulerAngles;
        v.x = eulerAngleY;
        v.z = 0;
        monsterCam.transform.eulerAngles = v;
        sceneAndUICam.transform.eulerAngles = v;
        if (bgObj == null && sceneAndUICam.transform.childCount != 0)
        {
            bgObj = sceneAndUICam.transform.GetChild(0);
            bgObj.SetParent(null);
            bgObj.eulerAngles = Vector3.zero;
        }
        if (bgObj != null)
        {
            var pos = sceneAndUICam.transform.position;
            pos.z += 85;
            pos.y += 6;
            bgObj.transform.position = pos;
        }

    }

    void GetPos(Transform trans, float radius, float rad)
    {
        var center = transform.position;
        rad = rad * Mathf.PI / 180;
        var x = center.x + -radius * Mathf.Sin(rad);
        var z = center.z + -radius * Mathf.Cos(rad);
        trans.position = new Vector3(x, trans.position.y, z);
    }
}
