using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Zenject;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class UIBattle : UIBase, ILoopParticleContainer
{
    [Inject]
    AssetManager assetManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    DataManager dataManager;
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    NetworkSaveManager saveManager;

    [Header("Player")]
    [SerializeField]
    ButtonLongPress playerLongPressArea;
    [SerializeField]
    RectTransform charactorPoint;
    public SpineCharacterCtrl spineCharactor;
    [SerializeField]
    Image playerHpImg, shieldImg;
    [SerializeField]
    TMP_Text maxHpText, currentHpText, shieldText;
    [SerializeField]
    Button skillsInfoBtn, endTurnBtn;
    [SerializeField]
    ParticleItem sealingEffect, removeEffect;
    [SerializeField]
    List<UISkillBattleItem> uiSkillItems = new List<UISkillBattleItem>();
    [SerializeField]
    UIColorCostItem redItem, greenItem, blueItem;
    [SerializeField]
    Transform[] skillPositions;
    [SerializeField]
    GameObject skillObjectBg, skillObject;
    [SerializeField]
    List<UIPassiveIconItem> passiveIconItems;
    [SerializeField]
    List<Image> antiqueImgs;
    [Header("Monster")]
    [SerializeField]
    public List<UIMiniMonsterHpInfo> miniMonsterInfos;
    [Header("Show")]
    /// <summary>爆衣表演</summary>
    public UIDressBreak uIDressBreak;
    /// <summary>大招表演</summary>
    public UIUltimate uIUltimate;
    [SerializeField]
    Image hit;
    Tween hitTween;
    public SpriteAnimation speedLineAnimate;
    [SerializeField]
    List<Animator> uiAnimators = new List<Animator>();
    [Header("System")]
    [SerializeField]
    RectTransform switchTargetArea, blockImg;
    [SerializeField]
    Button loseReviewBtn, reviveBtn, winReviewBtn;
    [SerializeField]
    ParticleItem shieldGainParticle, shieldBreakParticle;
    public RectTransform UICenterPoint, hpJumpPoint;
    public Transform effectFrontPoint, effectBackPoint;
    /// <summary>點擊怪物位置</summary>
    [SerializeField]
    ButtonLongPress monsterButton;
    readonly int skillmax = 13;
    int skillRange = 0;
    [SerializeField]
    CanvasGroup canvasGroup;
    [SerializeField]
    TMPro.TMP_Text titleText;
    [SerializeField]
    TMPro.TMP_Text levelText;

    /// <summary>跳寫跟隨的位置點</summary>
    public List<UIFollowObject> monsterScreenPoint;
    /// <summary> 儲存掛在身上的 Loop Particle 特效。 key: passiveId,  value:ParticleObj </summary>
    private Dictionary<int, GameObject> loopParticleObj = new Dictionary<int, GameObject>();

    public enum UIAnimatonStateEnum
    {
        UIBattlePhaseBegin,
        UIBattlePhaseGameOver,
        UIGameWin,
        YourTurn,
        YourTurnEnd,
        EnimyTurn,
        EnimyTurnEnd,
        ActionPhase,
        SceneName
    }
    public override UniTask OnOpen()
    {
        ClearPassive();
        ClearPlayerColorCost();
        endTurnBtn.onClick.AddListener(() => RxEventBus.Send(EventBusEnum.UIBattleEnum.OnClickPlayerRoundEnd));
        loseReviewBtn.onClick.AddListener(() => RxEventBus.Send(EventBusEnum.UIBattleEnum.OnClickReview));
        winReviewBtn.onClick.AddListener(() => RxEventBus.Send(EventBusEnum.UIBattleEnum.OnClickReview));
        reviveBtn.onClick.AddListener(() => RxEventBus.Send(EventBusEnum.UIBattleEnum.OnClickRevive));
        monsterButton.onLongPress.AddListener(() => RxEventBus.Send(EventBusEnum.UIBattleEnum.OnLongPressMonster));
        skillsInfoBtn.OnClickAsObservable().Subscribe(_ => RxEventBus.Send(EventBusEnum.UIBattleEnum.OnClickSkillInfo));
        ShowSkillItme(false);
        SetBlock(true);
        return base.OnOpen();
    }

    public void UpdateAntiqueImages(List<ActorPassive> actorPassives)
    {
        List<int> passives = new List<int>();
        for (int i = 0; i < actorPassives.Count; i++)
        {
            var define = dataTableManager.GetPassiveDefine(actorPassives[i].passiveId);
            if (define.passiveType == PassiveTypeEnum.Antique)
            {
                passives.Add(actorPassives[i].passiveId);
            }
        }
        for (int i = 0; i < antiqueImgs.Count; i++)
        {
            if (i < actorPassives.Count)
            {
                var define = dataTableManager.GetPassiveDefine(actorPassives[i].passiveId);
                antiqueImgs[i].enabled = true;
                antiqueImgs[i].sprite = define.icon;
            }
            else
            {
                antiqueImgs[i].enabled = false;
            }
        }
    }

    public void ClearPassive()
    {
        for (int i = 0; i < passiveIconItems.Count; i++)
        {
            passiveIconItems[i].Clear();
        }
    }

    public void SetTitle(int id)
    {
        var dDefine = dataTableManager.GetDungeonDataDefine(id);
        levelText.text = dDefine.mapLayer.ToString() + "-" + dDefine.UIPosition.ToString();
        switch (dDefine.sceneId)
        {
            case 1:
                titleText.text = "青青草原";
                break;
            case 2:
                titleText.text = "哥布林巢穴";
                break;
            case 3:
                break;
            case 4:
                break;
            default:
                break;
        }
    }

    public void SetBlock(bool torf)
    {
        blockImg.gameObject.SetActive(torf);
    }
    public void Init(BattleActor player)
    {
        var professionEnum = saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().SelectProfession;
        var define = dataTableManager.GetProfessionDataDefine(professionEnum);
        var obj = GameObject.Instantiate(define.spineCharacter.gameObject, charactorPoint);
        obj.name = define.spineCharacter.gameObject.name;
        spineCharactor = obj.GetComponent<SpineCharacterCtrl>();
        spineCharactor.SetSkin(SpineCharacterCtrl.SpineSkinEnum.Origin);
        spineCharactor.PlayIdle(true);
        playerLongPressArea.onLongPress.AsObservable().Subscribe(_ => RxEventBus.Send(EventBusEnum.UIBattleEnum.OnLongPressPlayer));
        // 初始化 大招表演
        uIUltimate.Init(define.spineCharacter.gameObject, define.ultimateSound, assetManager);
        uIDressBreak.Init(define.spineCharacter.gameObject, define.damageSound, assetManager);
        passiveManager.GetCurrentActorAttribute(player);
        var max = player.currentActorBaseAttribute.maxHp.GetValue();
        currentHpText.text = player.currentHp.ToString();
        maxHpText.text = max.ToString();
        playerHpImg.fillAmount = player.currentHp / (float)max;
        if (player.shield == 0)
        {
            shieldText.color = new Color(1, 1, 1, 0);
            shieldImg.color = new Color(1, 1, 1, 0);
        }
        for (int i = 0; i < passiveIconItems.Count; i++)
        {
            passiveIconItems[i].Clear();
        }
        for (int i = 0; i < miniMonsterInfos.Count; i++)
        {
            miniMonsterInfos[i].gameObject.SetActive(false);
        }
    }
    #region 技能輪盤表演
    float moveTime = 0.2f;
    async UniTask SkillItemMove(UISkillBattleItem skillItem, Vector3 pos, float sec, bool isRemove = false)
    {
        await skillItem.transform.DOMove(pos, sec).SetEase(Ease.Linear).AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(destroyCancellationToken);
        if (isRemove)
        {
            await skillItem.canvasGroup.DOFade(0, 0.2f).AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(destroyCancellationToken);
            assetManager.ReturnObjToPool(skillItem.gameObject);
            uiSkillItems.Remove(skillItem);
        }
    }

    public void ShowSkillItme(bool isActive)
    {
        skillObject.SetActive(isActive);
        skillObjectBg.SetActive(isActive);
    }

    public void InitSkillItem(PPlayerSkillDataInit initData)
    {
        skillRange = initData.skills.Count;
        var startCount = skillmax - skillRange;
        for (int i = 0; i < uiSkillItems.Count; i++)
        {
            assetManager.ReturnObjToPool(uiSkillItems[i].gameObject);
        }
        uiSkillItems.Clear();
        for (int i = 0; i < initData.skills.Count; i++)
        {
            var item = GainSkillItem(initData.skills[i]);
            item.skillIndex = i;
            item.transform.position = skillPositions[startCount + i * 2].position;
            uiSkillItems.Add(item);
        }
    }

    UISkillBattleItem GainSkillItem(ActorSkill skill)
    {

        var skillItem = assetManager.GetObject<UISkillBattleItem>();
        skillItem.transform.SetParent(skillObject.transform);
        ((RectTransform)skillItem.transform).anchoredPosition3D = new Vector3(0, 500, 0);
        skillItem.transform.localScale = Vector3.one;
        skillItem.gameObject.SetActive(true);
        var define = dataTableManager.GetSkillDefine(skill.skillId);
        skillItem.SetSkillItem(define);
        skillItem.skillId = define.id;
        skillItem.longPressBtn.onLongPress.AsObservable().Subscribe(_ => RxEventBus.Send(EventBusEnum.UIBattleEnum.OnPressedSkillItem, skillItem.skillIndex));
        skillItem.button.OnClickAsObservable().Subscribe(_ => RxEventBus.Send(EventBusEnum.UIBattleEnum.OnClickSkillItem, skillItem.skillIndex));
        skillItem.canvasGroup.alpha = 1;
        skillItem.DoSealed(skill.isBanned);
        return skillItem;
    }

    public async UniTask InsertSkillItem(PPlayerSkillDataInsert updateData)
    {
        bool isShow = skillObjectBg.activeSelf;
        ShowSkillItme(true);
        var ls = new List<UniTask>();
        var startCount = skillmax - skillRange;
        for (int i = 0; i < updateData.skills.Count; i++)
        {
            ls.Clear();
            var item = GainSkillItem(updateData.skills[i]);
            if (updateData.isOverFlow)
            {
                var t = new UniTaskCompletionSource();
                item.canvasGroup.DOFade(0, moveTime);
                item.PlayAddFailure(() =>
                {
                    assetManager.ReturnObjToPool(item.gameObject);
                    t.TrySetResult();
                });
                ls.Add(t.Task);
            }
            else
            {
                var firstItem = uiSkillItems[0];
                uiSkillItems.Remove(firstItem);
                ls.Add(SkillItemMove(firstItem, skillPositions[startCount - 2].position, moveTime, true));
                if (updateData.indexs[i] >= skillRange)
                {
                    ls.Add(SkillItemMove(item, skillsInfoBtn.transform.position, moveTime * 2, true));
                    var gainIndex = startCount + skillRange * 2 + 1;
                    var skillId = updateData.pushSkills[i];
                    var pushItem = GainSkillItem(updateData.pushSkills[i]);
                    pushItem.transform.position = skillPositions[gainIndex].position;
                    uiSkillItems.Add(pushItem);
                }
                else
                {
                    uiSkillItems.Insert(updateData.indexs[i], item);
                }
                for (int j = 0; j < uiSkillItems.Count; j++)
                {
                    item = uiSkillItems[j];
                    var pos = skillPositions[startCount + j * 2].position;
                    ls.Add(SkillItemMove(item, pos, moveTime * 2));
                }
            }
            await UniTask.WhenAll(ls);
            await UniTask.Delay((int)(moveTime * 1000));
        }
        for (int j = 0; j < uiSkillItems.Count; j++)
        {
            uiSkillItems[j].skillIndex = j;
        }
        ShowSkillItme(isShow);
    }

    async public UniTask RemoveSkillItem(PPlayerSkillDataRemove updateData)
    {
        bool isShow = skillObjectBg.activeSelf;
        ShowSkillItme(true);
        var ls = new List<UniTask>();
        var startCount = skillmax - skillRange;
        for (int i = 0; i < updateData.indexs.Count; i++)
        {
            if (updateData.indexs[i] >= skillRange)
            {
                removeEffect.Play();
            }
            else
            {
                ls.Clear();
                var removeItem = uiSkillItems[updateData.indexs[i]];
                var t = new UniTaskCompletionSource();
                removeItem.canvasGroup.DOFade(0, moveTime);
                removeItem.PlayRemoveEffect(() =>
                {
                    assetManager.ReturnObjToPool(removeItem.gameObject);
                    t.TrySetResult();
                });
                uiSkillItems.RemoveAt(updateData.indexs[i]);
                await t.Task;
                await UniTask.Delay(200);
                var gainIndex = startCount + skillRange * 2 + 1;
                var item = GainSkillItem(updateData.pushSkills[i]);
                item.transform.position = skillPositions[gainIndex].position;
                uiSkillItems.Add(item);
                for (int j = 0; j < uiSkillItems.Count; j++)
                {
                    var itme = uiSkillItems[j];
                    item.skillIndex = j;
                    ls.Add(SkillItemMove(itme, skillPositions[startCount + j * 2].position, 0.2f));
                }
                await UniTask.WhenAll(ls);
            }
        }
        for (int j = 0; j < uiSkillItems.Count; j++)
        {
            uiSkillItems[j].skillIndex = j;
        }
        await UniTask.Delay((int)(moveTime * 2000));
        ShowSkillItme(isShow);
    }

    /// <summary>
    /// 推進技能
    /// </summary>
    /// <param name="pushData"></param>
    /// <returns></returns>
    public async UniTask PushSkillItem(PPlayerSkillDataMove pushData)
    {
        bool isShow = skillObjectBg.activeSelf;
        ShowSkillItme(true);
        for (int j = 0; j < pushData.pushSkills.Count; j++)
        {
            var startCount = skillmax - skillRange;
            var gainIndex = pushData.isPush ?
                startCount + skillRange * 2 + 1 : startCount - 2;
            var skill = pushData.pushSkills[j];

            var item = GainSkillItem(skill);
            item.transform.position = skillPositions[gainIndex].position;
            if (pushData.isPush) uiSkillItems.Add(item);
            else
                uiSkillItems.Insert(0, item);
            var ls = new List<UniTask>();
            for (int i = 0; i < uiSkillItems.Count; i++)
            {
                var lastIndex = pushData.isPush ? 0 : uiSkillItems.Count - 1;
                var idx = startCount + i * 2 + (pushData.isPush ? -2 : 0);
                var pos = skillPositions[idx].position;
                item = uiSkillItems[i];
                item.skillIndex = i;
                if (i == lastIndex)
                {
                    ls.Add(SkillItemMove(item, pos, 0.2f, true));
                }
                else ls.Add(SkillItemMove(item, pos, 0.2f));
            }
            await UniTask.WhenAll(ls);
            for (int i = 0; i < uiSkillItems.Count; i++)
            {
                uiSkillItems[i].skillIndex = i;
            }
        }
        await UniTask.Delay((int)(moveTime * 2000));
        ShowSkillItme(isShow);
    }

    public async UniTask UpdateSkillItemSealState(PPlayerSkillDataBanned dataBanned)
    {
        bool isShow = skillObjectBg.activeSelf;
        ShowSkillItme(true);
        if (dataBanned.bannedEnum == PPlayerSkillDataBanned.BannedTypeEnum.End)
        {
            await UniTask.Delay(300);
            for (int i = 0; i < dataBanned.indexs.Count; i++)
            {
                var idx = dataBanned.indexs[i];
                if (idx < uiSkillItems.Count) uiSkillItems[idx].EndSeal();
                else sealingEffect.Play();
            }
        }
        else if (dataBanned.bannedEnum == PPlayerSkillDataBanned.BannedTypeEnum.Start)
        {
            for (int i = 0; i < dataBanned.indexs.Count; i++)
            {
                var idx = dataBanned.indexs[i];
                if (idx < uiSkillItems.Count) uiSkillItems[idx].StartSeal();
                else sealingEffect.Play();
            }
            await UniTask.Delay(300);
        }
        ShowSkillItme(isShow);
    }

    public UniTask UpdateSkillItem(PPlayerSkillDataUpdate updateData)
    {
        bool isShow = skillObjectBg.activeSelf;
        ShowSkillItme(true);
        if (updateData.updateEnum == PPlayerSkillDataUpdate.UpdateTypeEnum.Use)
        {
            for (int i = 0; i < uiSkillItems.Count; i++)
            {
                uiSkillItems[i].Used(updateData.indexs.Exists(idx => idx == i));
            }
        }
        else if (updateData.updateEnum == PPlayerSkillDataUpdate.UpdateTypeEnum.Cost)
        {
            for (int i = 0; i < uiSkillItems.Count; i++)
            {
                uiSkillItems[i].ReadyCost(updateData.indexs.Exists(idx => idx == i));
            }
        }
        ShowSkillItme(isShow);
        return default;
    }
    #endregion


    public override void OnClose()
    {
        for (int i = 0; i < uiSkillItems.Count; i++)
        {
            assetManager.ReturnObjToPool(uiSkillItems[i].gameObject);
        }
        uiSkillItems.Clear();
        ResetLoopParticle();

        RxEventBus.UnRegister(this);
        base.OnClose();
    }

    #region 畫面表演相關
    public async UniTask DoUIAnimator(UIAnimatonStateEnum stateEnum, bool autoClose = true, Sprite sprite = null)
    {
        var animator = uiAnimators[(int)stateEnum];
        var stateName = stateEnum.ToString();
        switch (stateEnum)
        {
            case UIAnimatonStateEnum.YourTurn:
            case UIAnimatonStateEnum.YourTurnEnd:
            case UIAnimatonStateEnum.EnimyTurn:
            case UIAnimatonStateEnum.EnimyTurnEnd:
            case UIAnimatonStateEnum.ActionPhase:
            case UIAnimatonStateEnum.SceneName:
                stateName = "TextAnime";
                break;
            default:
                break;
        }
        if (UIAnimatonStateEnum.SceneName == stateEnum && sprite != null)
        {
            var img = animator.transform.Find("TargetTextImage").GetComponent<Image>();
            img.sprite = sprite;
        }
        animator.gameObject.SetActive(true);
        animator.Play(stateName, 0, 0f);
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) == false);
        if (autoClose) animator.gameObject.SetActive(false);
    }

    public async UniTask SpeedLine()
    {
        speedLineAnimate.gameObject.SetActive(true);
        speedLineAnimate.Play();
        await UniTask.Delay(2000);
        speedLineAnimate.Stop();
        speedLineAnimate.gameObject.SetActive(false);
    }

    public void Hit()
    {
        if (hitTween != null && hitTween.IsActive()) hitTween.Kill();
        hit.color = new Color(1, 1, 1, 0);
        hitTween = hit.DOFade(1, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    public async UniTask UpdatePlayerHp(int current, int max)
    {
        passiveManager.GetCurrentActorAttribute(battleManager.player);
        currentHpText.text = current.ToString();
        maxHpText.text = max.ToString();
        var value = current / (float)max;
        playerHpImg.DOFillAmount(value, 0.2f);
        if (value < 0.6f && playerHpImg.fillAmount > 0.6f && spineCharactor.currentSkin == SpineCharacterCtrl.SpineSkinEnum.Origin)
        {
            // 表演爆衣
            spineCharactor.SetSkin(SpineCharacterCtrl.SpineSkinEnum.Damage);
            await uIDressBreak.Show(SpineCharacterCtrl.SpineSkinEnum.Origin);
        }
        else if (value < 0.3f && playerHpImg.fillAmount > 0.3f && spineCharactor.currentSkin == SpineCharacterCtrl.SpineSkinEnum.Damage)
        {
            // 表演爆衣
            spineCharactor.SetSkin(SpineCharacterCtrl.SpineSkinEnum.Damage02);
            await uIDressBreak.Show(SpineCharacterCtrl.SpineSkinEnum.Damage);
        }
    }
    /// <summary>
    /// 玩家能量演出
    /// </summary>
    /// <param name="colorData"></param>

    public void UpdatePlayerColorCost(PModifyColorData colorData)
    {
        colorData.costColorCount.TryGetValue(SkillCostColorEnum.Red, out int redValue);
        redItem.SetValue(redValue);
        colorData.costColorCount.TryGetValue(SkillCostColorEnum.Green, out int greenValue);
        greenItem.SetValue(greenValue);
        colorData.costColorCount.TryGetValue(SkillCostColorEnum.Blue, out int blueValue);
        blueItem.SetValue(blueValue);
        for (int i = 0; i < colorData.effectDatas.Count; i++)
        {
            var effectData = colorData.effectDatas[i];
            switch (effectData.color)
            {
                case SkillCostColorEnum.Red:
                    redItem.ActiveParticle(effectData.effectEnum);
                    break;
                case SkillCostColorEnum.Green:
                    greenItem.ActiveParticle(effectData.effectEnum);
                    break;
                case SkillCostColorEnum.Blue:
                    blueItem.ActiveParticle(effectData.effectEnum);
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// 玩家被動演出
    /// </summary>
    /// <param name="passiveData"></param>
    /// <returns></returns>
    public UniTask SetPassive(PPassiveData passiveData)
    {
        var define = dataTableManager.GetPassiveDefine(passiveData.passiveId);
        if (passiveData.isRemove)
        {
            var passiveItem = passiveIconItems.Find(p => p.passiveId == define.id);
            if (passiveItem != null)
            {
                passiveItem.passiveId = 0;
                passiveItem.DoRemove();
            }
        }
        else
        {
            var passiveItem = passiveIconItems.Find(p => p.passiveId == define.id);
            if (passiveItem == null)
            {
                var idx = passiveIconItems.FindIndex(p => p.passiveId == 0);
                if (idx == passiveIconItems.Count - 1)
                {
                    passiveIconItems[idx].DoAddAnimate();
                }
                else
                {
                    passiveIconItems[idx].passiveId = define.id;
                    passiveIconItems[idx].stackCount.text = passiveData.stackCount.ToString();
                    passiveIconItems[idx].icon.sprite = define.icon;
                    passiveIconItems[idx].DoAddAnimate();
                }
            }
            else
            {
                passiveItem.DoUpdate();
                passiveItem.stackCount.text = passiveData.stackCount.ToString();
            }
        }
        return UniTask.Delay(175);
    }
    public void UpdatePlayerShield(PModifyShieldData data)
    {
        var t = 0.5f;
        //數值沒有異動 什麼都不做
        shieldText.text = data.shieldValue.ToString();
        if (data.beforeValue == data.shieldValue) return;
        // 特效數值增加表演
        if (data.shieldValue > 0)
        {
            //初次獲得護盾
            if (data.beforeValue == 0)
            {
                shieldImg.DOFade(1, t);
                shieldText.DOFade(1, t);
            }
            shieldGainParticle.Play();
        }

        // 數值修改就放大縮小
        shieldText.transform.localScale = Vector3.one * 1.5f;
        shieldText.transform.DOScale(1, t);
        // 減少 紅變白
        if (data.beforeValue > data.shieldValue)
        {
            shieldText.color = Color.red;
            // 減少變紅轉白
            var color = Color.white;
            // 結果為0 淡出
            if (data.shieldValue == 0)
            {
                color.a = 0;
                shieldImg.DOFade(0, t);
                shieldBreakParticle.Play();
            }
            shieldText.DOColor(color, t);
        }
    }

    public bool UpdateLoopParticle(int passiveId, GameObject particle)
    {
        var updateSuccess = false;
        if (passiveId > 0)
        {
            if (particle != null && !loopParticleObj.ContainsKey(passiveId))
            {
                loopParticleObj.Add(passiveId, particle);
                updateSuccess = true;
            }
            else if (particle == null && loopParticleObj.ContainsKey(passiveId))
            {
                var removeParticle = loopParticleObj[passiveId];
                removeParticle.SetActive(false);
                GameObject.DestroyImmediate(removeParticle);
                loopParticleObj.Remove(passiveId);
                updateSuccess = true;
            }
        }

        return updateSuccess;
    }

    public void ResetLoopParticle()
    {
        foreach (var particle in loopParticleObj.Values)
        {
            if (particle == null) continue;
            particle.gameObject.SetActive(false);
            GameObject.DestroyImmediate(particle.gameObject);
        }
        loopParticleObj.Clear();
    }
    #endregion
    void ClearPlayerColorCost()
    {
        redItem.SetValue(0);
        greenItem.SetValue(0);
        blueItem.SetValue(0);
        redItem.Select(false);
        greenItem.Select(false);
        blueItem.Select(false);
    }

    #region 畫面滑動控制
    Vector2 beginV2, currentV2;
    bool isInRect = false;
    /// <summary>開關滑動畫面</summary>
    public bool isActiveControl = false;
    void ScreenControl()
    {
        if (!isActiveControl) return;
        if (Touch.activeTouches.Count >= 1)
        {
            if (Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                //判斷手指起始點是在允許範圍內
                isInRect = UtilityHelper.IsInRect((RectTransform)switchTargetArea.transform);
                if (isInRect)
                {
                    beginV2 = Touch.activeTouches[0].screenPosition;
                    RxEventBus.Send(EventBusEnum.ScreenControlEnum.Began);
                }
            }
            else if (isInRect && Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                currentV2 = Touch.activeTouches[0].screenPosition;
                // 判斷水平滑動距離是否小於垂直
                if (Mathf.Abs(beginV2.x - currentV2.x) > Mathf.Abs(beginV2.y - currentV2.y))
                {
                    // 滑動距離為 寬度的5% 就會判定確定滑動
                    if (Mathf.Abs(beginV2.x - currentV2.x) >= Screen.width * 0.05f)
                    {
                        if (beginV2.x > currentV2.x)
                            RxEventBus.Send(EventBusEnum.ScreenControlEnum.Left);
                        else
                            RxEventBus.Send(EventBusEnum.ScreenControlEnum.Right);
                    }
                }
            }
            else if (Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                isInRect = false;
                RxEventBus.Send(EventBusEnum.ScreenControlEnum.End);
            }
        }
    }
    public void Update()
    {
        ScreenControl();
    }
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }
    #endregion

    /// <summary>
    /// 播放恢復特效
    /// </summary>
    /// <returns></returns>
    public async UniTask OnStartRecover()
    {
        // TODO: 執行恢復特效 (SystemEffect_LevelHeal)
        // else? ...


        // 此行補上 執行恢復特效 後可刪除
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
    }
}
