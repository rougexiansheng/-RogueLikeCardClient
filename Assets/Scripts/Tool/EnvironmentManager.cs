using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

/// <summary>
/// 地圖關卡場景
/// </summary>
public class EnvironmentManager : IInitializable, ITickable
{
    [Inject]
    UIManager uIManager;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    AssetManager assetManager;


    List<GameObject> sceneGameObjects = new List<GameObject>();
    GameObject baseEmptyObj;
    float sceneDistance = 50f;
    GameObject skyBg;
    GameObject lightObj;
    int mapLayerNumber = -1;

    int sceneLayer = LayerMask.NameToLayer("Scene");
    int spineLayer = LayerMask.NameToLayer("Spine");
    public MonsterPoint[] monsterPoints;
    Camera mainCam, spineCam;
    /// <summary>中間特效的位置</summary>
    public Canvas centerCanvas;
    /// <summary>
    /// 固定數值
    /// </summary>
    readonly float originCamPosY = 6, originCamDis = 15;
    #region 相機相關 當前數值
    /// <summary>怪物兩側與中心的角度</summary>
    float monsterAngle = 30;
    /// <summary>相機X軸(垂直)旋轉</summary>
    float cameraAngleX = 32;
    /// <summary>相機Y軸(水平)旋轉</summary>
    float cameraAngleY = 0;
    /// <summary>相機Y軸高度/背景Y軸高度</summary>
    float cameraPositionY = 6, bgPosisitonY = 6;
    /// <summary>相機旋轉的中心位置</summary>
    Vector3 cameraCenterV3 = new Vector3(40, 0, 30);
    /// <summary>怪物與中心位置的距離</summary>
    float monsterDistance = 10;
    /// <summary>相機與怪物位置的距離</summary>
    float cameraDistance = 15;
    /// <summary>背景與相機距離</summary>
    Vector3 bgDistance = new(0, 6, 85);
    #endregion
    /// <summary>是否正在調整數值</summary>
    bool isModify = true;
    bool dragIng = true;
    BattleActor.MonsterPositionEnum currentPos = BattleActor.MonsterPositionEnum.None;
    CancellationTokenSource tokenSource = new CancellationTokenSource();
    public void Initialize()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        spineCam = GameObject.FindGameObjectWithTag("SpineCamera").GetComponent<Camera>();
        monsterPoints = GameObject.Find("MonsterPoints").GetComponentsInChildren<MonsterPoint>();
        centerCanvas = GameObject.Find("CenterCanvas").GetComponent<Canvas>();
        RxEventBus.Register(EventBusEnum.ScreenControlEnum.Began, MoveBegan, this);
        RxEventBus.Register(EventBusEnum.ScreenControlEnum.Left, MoveLeft, this);
        RxEventBus.Register(EventBusEnum.ScreenControlEnum.Right, MoveRight, this);
        RxEventBus.Register(EventBusEnum.ScreenControlEnum.End, MoveEnd, this);
    }
    void MoveBegan()
    {
        dragIng = false;
    }
    void MoveLeft()
    {
        CheckNextMonsterPoint(currentPos, true);
        dragIng = true;
    }
    void MoveRight()
    {
        CheckNextMonsterPoint(currentPos, false);
        dragIng = true;
    }

    void MoveEnd()
    {
        dragIng = false;
    }
    /// <summary>
    /// 移動到下一個怪物視角
    /// </summary>
    /// <param name="current"></param>
    /// <param name="isAdd"></param>
    void CheckNextMonsterPoint(BattleActor.MonsterPositionEnum current, bool isAdd)
    {
        if (dragIng) return;
        var i = (int)current;
        if (isAdd) i++;
        else i--;
        if (i > 2 || i < 0)
        {
            return;
        }
        if (monsterPoints[i].gameObject.activeSelf == false)
            CheckNextMonsterPoint((BattleActor.MonsterPositionEnum)i, isAdd);
        else
        {
            if (tokenSource != null) tokenSource.Cancel();
            tokenSource = new CancellationTokenSource();
            MoveCameraAngle((BattleActor.MonsterPositionEnum)i).AttachExternalCancellation(tokenSource.Token);
        }
    }

    public void RestScene()
    {
        GameObject.DestroyImmediate(baseEmptyObj);
        GameObject.DestroyImmediate(skyBg);
        GameObject.DestroyImmediate(lightObj);
        sceneGameObjects.Clear();
        mapLayerNumber = -1;
        currentPos = BattleActor.MonsterPositionEnum.None;
    }

    public async UniTask MoveScene()
    {
        if (baseEmptyObj != null)
        {
            await MoveCameraAngle(BattleActor.MonsterPositionEnum.Center);
            var posZ = baseEmptyObj.transform.position.z - sceneDistance;
            await baseEmptyObj.transform.DOMoveZ(posZ, 1).AsyncWaitForCompletion().AsUniTask();
        }
    }

    public void SetNextScene(SDKProtocol.DungeonLevelData data)
    {
        var doungeonDefine = dataTableManager.GetDungeonDataDefine(data.dungeonId);
        if (mapLayerNumber >= doungeonDefine.mapLayer) return;
        mapLayerNumber = doungeonDefine.mapLayer;
        var sceneDefine = dataTableManager.GetSceneDataDefine(doungeonDefine.sceneId);
        if (baseEmptyObj == null)
        {
            baseEmptyObj = new GameObject("baseSceneEmptyObj");
            baseEmptyObj.transform.position = Vector3.zero;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.skybox = sceneDefine.sceneSetting.skyBox;
            RenderSettings.fogColor = sceneDefine.sceneSetting.fogColor;
            RenderSettings.fogStartDistance = sceneDefine.sceneSetting.fogDensityStart;
            RenderSettings.fogEndDistance = sceneDefine.sceneSetting.fogDensityEnd;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            skyBg = GameObject.Instantiate(sceneDefine.sceneSetting.BG_Prefab, new Vector3(mainCam.transform.position.x, bgPosisitonY, mainCam.transform.position.z) + bgDistance, Quaternion.identity);
            UtilityHelper.SetChildLayer(skyBg.transform, sceneLayer);
            lightObj = GameObject.Instantiate(sceneDefine.sceneSetting.lightGroup_Prefab);
            //修正背景與相機位置
            isModify = true;
            UpdateSceneObjectsTransform();
            isModify = false;

            DynamicGI.UpdateEnvironment();
        }

        GameObject originObj = null;
        if (data.nodeEnum == MapNodeEnum.Boss)
        {
            originObj = sceneDefine.sceneBoss;
        }
        else
        {
            var r = UnityEngine.Random.Range(0, sceneDefine.sceneObj.Count);
            originObj = sceneDefine.sceneObj[r];
        }
        var posZ = sceneGameObjects.Count > 0 ? sceneGameObjects[sceneGameObjects.Count - 1].transform.position.z + sceneDistance : 0;
        var obj = GameObject.Instantiate(originObj, new Vector3(0, 0, posZ), Quaternion.identity, baseEmptyObj.transform);
        obj.layer = sceneLayer;
        UtilityHelper.SetChildLayer(obj.transform, sceneLayer);
        sceneGameObjects.Add(obj);
        if (sceneGameObjects.Count > 5)
        {
            GameObject.Destroy(sceneGameObjects[0]);
            sceneGameObjects.RemoveAt(0);
        }
    }

    void UpdateSceneObjectsTransform()
    {
        if (!isModify) return;
        //更新相機位置
        SetTransformPosition(mainCam.transform, monsterDistance + cameraDistance, cameraAngleY, cameraPositionY);
        SetTransformPosition(spineCam.transform, monsterDistance + cameraDistance, cameraAngleY, cameraPositionY);
        //更新相機旋轉
        SetCameraRotation(mainCam.transform);
        SetCameraRotation(spineCam.transform);
        //更新怪物位置 左(0) 中(1) 右(2)平行相機
        SetTransformPosition(monsterPoints[0].transform, monsterDistance, monsterAngle);
        SetTransformPosition(monsterPoints[1].transform, monsterDistance, 0);
        SetTransformPosition(monsterPoints[2].transform, monsterDistance, -monsterAngle);
        for (int i = 0; i < monsterPoints.Length; i++)
        {
            monsterPoints[i].transform.rotation = mainCam.transform.rotation;
        }
        // 更新中心特效位置跟平行相機
        SetTransformPosition(centerCanvas.transform, monsterDistance + 1.5f, 0);
        centerCanvas.transform.rotation = mainCam.transform.rotation;
        // 背景物件平行相機並保持一定距離
        if (skyBg != null)
        {
            skyBg.transform.position = new Vector3(mainCam.transform.position.x, bgPosisitonY, mainCam.transform.position.z) + bgDistance;
        }
    }

    float ultCamDis = 20, ultCamPosY = 8;
    bool isUp = false;
    /// <summary>
    /// 群體時像機移動
    /// </summary>
    /// <returns></returns>
    public async UniTask UltMove()
    {
        if (isUp) return;
        isUp = true;
        isModify = true;
        currentPos = BattleActor.MonsterPositionEnum.Center;
        RxEventBus.Send(EventBusEnum.PlayerDataEnum.UpdateSelectTarget, currentPos);
        SetSpineMonsterColor(currentPos);
        SetMonsterPointOrder(currentPos);
        var t = 0.4f;
        var t1 = DOTween.To(() => cameraAngleY, x => cameraAngleY = x, 0, t).SetEase(Ease.Linear).AsyncWaitForCompletion().AsUniTask();
        var t2 = DOTween.To(() => cameraDistance, x => cameraDistance = x, ultCamDis, t).SetEase(Ease.Linear).AsyncWaitForCompletion().AsUniTask();
        var t3 = DOTween.To(() => cameraPositionY, x => cameraPositionY = x, ultCamPosY, t).SetEase(Ease.Linear).AsyncWaitForCompletion().AsUniTask();
        await UniTask.WhenAll(t1, t2, t3);
        isModify = false;
    }

    public async UniTask BackCamera()
    {
        if (!isUp) return;
        isModify = true;
        var t = 0.3f;
        var t1 = DOTween.To(() => cameraDistance, x => cameraDistance = x, originCamDis, t).AsyncWaitForCompletion().AsUniTask();
        var t2 = DOTween.To(() => cameraPositionY, x => cameraPositionY = x, originCamPosY, t).AsyncWaitForCompletion().AsUniTask();
        await UniTask.WhenAll(t1, t2);
        isUp = false;
        isModify = false;
    }

    /// <summary>
    /// 指定位置 不管怪物是否存在
    /// </summary>
    /// <param name="positionEnum"></param>
    /// <returns></returns>
    public async UniTask MoveCameraAngle(BattleActor.MonsterPositionEnum positionEnum)
    {
        if (currentPos == positionEnum)
        {
            SetSpineMonsterColor(currentPos);
            SetMonsterPointOrder(currentPos);
            return;
        }
        isModify = true;
        float angle = 0;
        if (positionEnum == BattleActor.MonsterPositionEnum.Left) angle = monsterAngle;
        else if (positionEnum == BattleActor.MonsterPositionEnum.Right) angle = -monsterAngle;
        currentPos = positionEnum;
        RxEventBus.Send(EventBusEnum.PlayerDataEnum.UpdateSelectTarget, currentPos);
        SetSpineMonsterColor(currentPos);
        SetMonsterPointOrder(currentPos);
        await DOTween.To(() => cameraAngleY, x => cameraAngleY = x, angle, 0.2f).AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(tokenSource.Token);
        isModify = false;
    }

    void SetMonsterPointOrder(BattleActor.MonsterPositionEnum positionEnum)
    {
        for (int i = 0; i < monsterPoints.Length; i++)
        {
            var canvas = monsterPoints[i].canvas;
            if (positionEnum == (BattleActor.MonsterPositionEnum)i)
            {
                canvas.sortingOrder = 1;
            }
            else
            {
                canvas.sortingOrder = 0;
            }
        }
    }

    /// <summary>
    /// 修改怪物顏色 在後面會是些微壓灰
    /// None表示全部顯示原色
    /// </summary>
    void SetSpineMonsterColor(BattleActor.MonsterPositionEnum positionEnum)
    {
        for (int i = 0; i < monsterPoints.Length; i++)
        {
            var spineChar = monsterPoints[i].spineCharactor;
            if (spineChar != null)
            {
                if (positionEnum == (BattleActor.MonsterPositionEnum)i || positionEnum == BattleActor.MonsterPositionEnum.None)
                {
                    spineChar.spine.color = Color.white;
                }
                else
                {
                    spineChar.spine.color = Color.gray;
                }
            }
        }
    }

    void SetCameraRotation(Transform trans)
    {
        var dir = cameraCenterV3 - trans.position;
        dir = new Vector3(dir.x, 0, dir.z);
        trans.rotation.SetLookRotation(dir);
        trans.eulerAngles = new Vector3(cameraAngleX, cameraAngleY, trans.eulerAngles.z);
    }

    void SetTransformPosition(Transform trans, float radius, float rad, float posY = 0)
    {
        rad = rad * Mathf.PI / 180;
        var x = cameraCenterV3.x + -radius * Mathf.Sin(rad);
        var z = cameraCenterV3.z + -radius * Mathf.Cos(rad);
        trans.position = new Vector3(x, posY, z);
    }

    #region 場景UI控制
    public UniTask MonsterNextSkill(PMonsterNextSkillData nextSkillData)
    {
        var define = dataTableManager.GetSkillDefine(nextSkillData.skillId);
        return monsterPoints[(int)nextSkillData.positionEnum].hpBar.SetSkillIcon(define.icon);
    }

    public void SetMonsterPos(List<BattleActor> monsters, bool isElite)
    {
        for (int i = 0; i < monsterPoints.Length; i++)
        {
            monsterPoints[i].Clear();
            monsterPoints[i].hpBar.SetEdge(isElite);
        }
        for (int i = 0; i < monsters.Count; i++)
        {
            var m = monsters[i];
            SetMonsterPoint(monsterPoints[(int)m.monsterPos], m, i);
        }
    }

    public void SetMonsterPoint(MonsterPoint monsterPoint, BattleActor monster, int idx)
    {
        monsterPoint.Clear();
        if (idx < 0) return;
        passiveManager.GetCurrentActorAttribute(monster);
        monsterPoint.hpBar.SetHp(monster.currentHp, monster.maxHp);
        monsterPoint.monsterIndex = idx;
        var monsterDefine = dataTableManager.GetMonsterDefine(monster.monsterId);
        var obj = GameObject.Instantiate(monsterDefine.spineObj, monsterPoint.spinePoint);
        UtilityHelper.SetChildLayer(obj.transform, spineLayer);
        obj.name = monsterDefine.spineObj.name;
        monsterPoint.spineCharactor = obj.GetComponent<SpineCharacterCtrl>();
        monsterPoint.spineCharactor.PlayIdle(true);
    }
    public async UniTask MonsterRemove(PMonsterRemoveData removeData)
    {
        //移除怪物
        var tLs = new List<UniTask>();
        for (int i = 0; i < removeData.positions.Count; i++)
        {
            MonsterPoint mp = monsterPoints[(int)removeData.positions[i]];
            tLs.Add(mp.MonsterRemove());
        }
        await UniTask.WhenAll(tLs);

        //表演掉落
        var acqLs = new List<UniTask>();
        List<UIItemIcon> itemIcons = new List<UIItemIcon>();
        for (int i = 0; i < removeData.positions.Count; i++)
        {
            foreach (var icon in removeData.acquisitionList)
            {
                var uiDropItem = assetManager.GetObject<UIItemIcon>();

                uiDropItem.transform.SetParent(monsterPoints[(int)removeData.positions[i]].transform, false);
                uiDropItem.transform.localRotation = Quaternion.identity;
                uiDropItem.transform.position = monsterPoints[(int)removeData.positions[i]].spinePoint.position;
                uiDropItem.SetIcon(dataTableManager.GetItemDataDefine(icon.id).icon);
                itemIcons.Add(uiDropItem);
                acqLs.Add(uiDropItem.DropAnimation());
            }

        }
        await UniTask.WhenAll(acqLs);
        //回收 ItemIcon
        foreach (var icon in itemIcons)
        {
            assetManager.ReturnObjToPool(icon.gameObject);
        }
        //關閉 Monster Poiont 
        for (int i = 0; i < removeData.positions.Count; i++)
        {
            monsterPoints[(int)removeData.positions[i]].Active(false);
        }

        //沒有怪物相機回到正中間
        for (int i = 0; i < monsterPoints.Length; i++)
        {
            if (monsterPoints[i].gameObject.activeSelf)
            {
                await MoveCameraAngle((BattleActor.MonsterPositionEnum)i);
                break;
            }
        }
    }

    public async UniTask ShakeCamera(float time = 2)
    {
        mainCam.transform.DOShakePosition(time, new Vector3(0.2f, 0.5f, 0), 100);
        spineCam.transform.DOShakePosition(time, new Vector3(0.2f, 0.5f, 0), 100);
        await UniTask.Delay((int)(time * 1000));
    }

    public UniTask PassiveData(PPassiveData passiveData)
    {
        var mp = monsterPoints[(int)passiveData.monsterPosition];
        var define = dataTableManager.GetPassiveDefine(passiveData.passiveId);
        if (passiveData.isRemove)
        {
            var passiveItem = mp.hpBar.passiveIconItems.Find(p => p.passiveId == define.id);
            if (passiveItem != null)
            {
                passiveItem.passiveId = 0;
                passiveItem.DoRemove();
            }
        }
        else
        {
            var passiveItem = mp.hpBar.passiveIconItems.Find(p => p.passiveId == define.id);
            if (passiveItem == null)
            {
                var idx = mp.hpBar.passiveIconItems.FindIndex(p => p.passiveId == 0);
                if (idx == mp.hpBar.passiveIconItems.Count - 1)
                {
                    mp.hpBar.passiveIconItems[idx].DoAddAnimate();
                }
                else
                {
                    mp.hpBar.passiveIconItems[idx].passiveId = define.id;
                    mp.hpBar.passiveIconItems[idx].stackCount.text = passiveData.stackCount.ToString();
                    mp.hpBar.passiveIconItems[idx].icon.sprite = define.icon;
                    mp.hpBar.passiveIconItems[idx].DoAddAnimate();
                }
            }
            else
            {
                passiveItem.stackCount.text = passiveData.stackCount.ToString();
                passiveItem.DoUpdate();
            }
        }
        return UniTask.Delay(175);
    }
    #endregion

    public void Tick()
    {
        UpdateSceneObjectsTransform();
    }
}
