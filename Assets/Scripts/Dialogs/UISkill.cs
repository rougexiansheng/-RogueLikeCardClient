using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UISkill : UIBase, LoopScrollPrefabSource, LoopScrollDataSource
{
    [Inject]
    SDKProtocol.IProtocolBridge sdk;

    [Inject]
    NetworkSaveManager saveManager;

    [Inject]
    AssetManager assetManager;
    [Inject]
    BattleManager battleManager;
    [Inject]
    SkillManager skillManager;
    [Inject]
    PassiveManager passiveManager;
    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    UIManager uIManager;
    [SerializeField]
    private Image displayprefab;
    [SerializeField]
    private UISkillSwipeItem swipePrefab;
    [SerializeField]
    private UISkillEquipment chooseSkillSetPage;
    [SerializeField]
    private UISkillChangePage chagneSkillPage;
    [SerializeField]
    private UISkillOverview SkillInfoPage;
    [SerializeField]
    private GameObject displayIcon;
    [SerializeField]
    private GameObject displayIconContent;
    [SerializeField]
    private UINowLocationMark nowMark;
    [SerializeField]
    private LoopScrollRect SwipeScrollRect;
    [SerializeField]
    private EventTrigger swipeEventTrigger;
    private EventTrigger.Entry swipeEndDragEntry;
    private EventTrigger.Entry swipeDragEntry;
    [SerializeField]
    private CanvasGroup commonCanvas;
    [SerializeField]
    private RectTransform layoutRoot;
    [SerializeField]
    private float fadeInTime = 0.4f;
    private int scrollRectCount;
    private int groupHeadIndex = 0;
    private List<ActorSkill> skillIDList = new List<ActorSkill>();
    private List<Vector3> displayPositionList = new List<Vector3>();
    private int changeID;

    private bool CanLongPress = false;
    private List<SDKProtocol.SkillGroup> currentGroup;
    private List<UISkillStateItem> skillItemsList = new List<UISkillStateItem>();
    List<SDKProtocol.SkillGroup> totalSkillGroups;


    private UniTaskCompletionSource<bool> changeResultTask;
    private object _changeLock = new object();

    private UniTaskCompletionSource<int> chooseResultTask;
    private object _chooseLock = new object();

    private int chooseGroupIndex = 0;
    private object _lock = new object();
    public override UniTask OnOpen()
    {
        return base.OnOpen();
    }

    /// <summary>
    /// For Check Skill Page
    /// </summary>
    /// <param name="actorSkills"></param>
    public async UniTask OpenCheckSkillPage(List<ActorSkill> actorSkills)
    {
        CanLongPress = false;
        //測試用
        actorSkills = battleManager.player.skills;
        var skillList = ChangeToSkillIDList(actorSkills);
        CommonPageFadeOut();
        SwipeScrollRectInit();
        endDragEventTriggerInit();

        SkillInfoPage.gameObject.SetActive(true);
        passiveManager.GetCurrentActorAttribute(battleManager.player);
        SkillInfoPage.SetNumText(battleManager.player.currentActorBaseAttribute.currentMove.ToString());
        SkillInfoPage.AddBackButtonListener(() =>
        {
            uIManager.RemoveUI<UISkill>();
        });

        CommonPageFadeIn();

        await createSkillBar(actorSkills);
        addEndDragListener(showCenterSkillInfo, SkillInfoPage.infoItem);
        dragEventTriggerInit();
        addDragListener(UpdateMarkPosition);
        SwipeScrollRect.ScrollToCell((skillIDList.Count / 2) + 1, -1);
        SwipeScrollRect.ToCenter();
        showTargetIndexSkill(SkillInfoPage.infoItem, 0);
        UpdateMarkPosition();
    }


    /// <summary>
    /// Change Skill Page
    /// </summary>
    /// <param name="skills">目前的技能組</param>
    /// <param name="changeSkillID">更換的技能ID</param>
    /// <returns></returns>
    public async UniTask OpenChangeSkillPage(List<ActorSkill> skills, int changeSkillID)
    {
        CanLongPress = false;
        var IDList = ChangeToSkillIDList(skills);
        //skills = battleManager.player.baseSkills;
        CommonPageFadeOut();
        SwipeScrollRectInit();
        endDragEventTriggerInit();
        chagneSkillPage.Init();
        chagneSkillPage.gameObject.SetActive(true);
        changeID = changeSkillID;
        chagneSkillPage.AddCancelButtonListener(() =>
        {
            SetChangeResult(false);
            uIManager.RemoveUI<UISkill>();
        });

        CommonPageFadeIn();
        await createSkillBar(skills);
        SetupUpdateSkillBlock(chagneSkillPage.UpdateSkillInfo);
        addEndDragListener(showCenterSkillInfo, chagneSkillPage.CurrentInfoItem);
        addEndDragListener(SetupUpdateSkillBlock, chagneSkillPage.UpdateSkillInfo);
        dragEventTriggerInit();
        addDragListener(UpdateMarkPosition);
        SwipeScrollRect.ScrollToCell((skillIDList.Count / 2) + 1, -1);
        SwipeScrollRect.ToCenter();
        showTargetIndexSkill(chagneSkillPage.CurrentInfoItem, 0);
    }

    /// <summary>
    /// Equipment Skill Page
    /// </summary>
    /// <param name="skillGroups"> Character skill groups</param>
    /// <param name="unlockSkill"> Character unlock skill</param>
    /// <param name="prevGroupsIndex"> last time player choose skill group index </param>
    public void OpenEquipmentSkillPage(List<SDKProtocol.SkillGroup> skillGroups, List<int> unlockSkill, int prevGroupsIndex)
    {
        chooseGroupIndex = prevGroupsIndex;
        groupHeadIndex = (chooseGroupIndex / 4) * 4;
        totalSkillGroups = skillGroups;
        CanLongPress = true;
        ResetPlayerOwnSKill();
        CommonPageFadeOut();
        endDragEventTriggerInit();
        dragEventTriggerInit();
        chooseSkillSetPage.gameObject.SetActive(true);
        chooseSkillSetPage.Init();
        currentGroup = GetCurrentPageSkillGroups(totalSkillGroups);
        SwipeScrollRectInit();
        createPlayerOwnSkill(unlockSkill);
        SetupChangePageButton();
        SetSetBar(currentGroup);
        chooseSkillSetPage.backButton.onClick.AddListener(() =>
        {
            SetGroupResult(chooseGroupIndex);
        });
        chooseSkillSetPage.ClickTargetSetButton(chooseGroupIndex % 4);
    }

    #region  Performance
    private void CommonPageFadeIn()
    {
        commonCanvas.DOFade(1, fadeInTime);
    }

    private void CommonPageFadeOut()
    {
        commonCanvas.DOFade(0, 0);
    }

    private async UniTask thisPageFadeOut()
    {
        await this.GetComponent<CanvasGroup>().DOFade(0, fadeInTime).AsyncWaitForCompletion();
    }
    #endregion

    #region  Skill Bar

    /// <summary>
    /// Create right side skill bar (Swipe scroll rect & display icon)
    /// </summary>
    /// <param name="skills"></param>
    /// <returns></returns>
    private async UniTask createSkillBar(List<ActorSkill> skills)
    {
        clearDisplayIcon();
        setScrollRectData(skills);
        //setDisplayIconSize(skills.Count);
        await createDisplayIcon(skills);
        SwipeScrollRect.RefillCells();
        SwipeScrollRect.ToCenter();
        UpdateMarkPosition();

    }
    #endregion

    #region  DisplayIcon

    private void setDisplayIconSize(int iconCount)
    {
        var rectTransform = displayIcon.transform as RectTransform;
        float height = (iconCount * 40 + (iconCount - 1) * 10) / 2f;
        float top = rectTransform.offsetMax.y + height;
        float bottom = rectTransform.offsetMin.y - height;
        rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, bottom);
        rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, top);
    }

    private async UniTask createDisplayIcon(List<ActorSkill> skills)
    {
        List<Transform> iconTransforms = new List<Transform>();
        foreach (var skill in skills)
        {
            var skillData = dataTableManager.GetSkillDefine(skill.skillId);
            var icon = Instantiate(displayprefab).GetComponent<UISkillDisplayItem>();
            icon.transform.SetParent(displayIconContent.transform, false);
            icon.Init(skill.skillId, skillData.icon);
            iconTransforms.Add(icon.transform);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        await UniTask.WaitForEndOfFrame(this);
        foreach (var transform in iconTransforms)
        {
            displayPositionList.Add(transform.position);
        }

    }

    private void clearDisplayIcon()
    {
        if (displayPositionList.Count == 0)
            return;
        foreach (Transform child in displayIconContent.transform)
        {
            Destroy(child.gameObject);
        }
        displayPositionList.Clear();
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
    }
    #endregion
    #region LoopScrollRect

    private void SwipeScrollRectInit()
    {
        assetManager.SetDefaultObject<UISkillSwipeItem>(swipePrefab);
        SwipeScrollRect.prefabSource = this;
        SwipeScrollRect.dataSource = this;
        SwipeScrollRect.totalCount = -1;
    }

    private void setScrollRectData(List<ActorSkill> skills)
    {
        skillIDList = skills;
        scrollRectCount = skills.Count;
    }

    public GameObject GetObject(int index)
    {
        var image = assetManager.GetObject<UISkillSwipeItem>();
        image.transform.localScale = Vector3.one;
        return image.gameObject;
    }
    public void ReturnObject(Transform trans)
    {
        assetManager.ReturnObjToPool(trans.gameObject);
    }
    public void ProvideData(Transform transform, int idx)
    {
        int index = transformIndexNumber(idx);
        int skillID = skillIDList[index].skillId;
        var skillData = dataTableManager.GetSkillDefine(skillID);
        var item = transform.GetComponent<UISkillSwipeItem>();
        item.Init(index, skillData.icon);
        if (CanLongPress)
        {
            UISkillPopupInfoPage popUpPage;
            item.ButtonLongPress.onLongPress.RemoveAllListeners();
            item.ButtonLongPress.onLongPress.AddListener(async () =>
            {
                popUpPage = await uIManager.OpenUI<UISkillPopupInfoPage>();
                popUpPage.Init(skillID);
                popUpPage.CancelButton.onClick.AddListener(() =>
                {
                    Destroy(popUpPage.gameObject);
                });
            });
        }
    }

    private void UpdateMarkPosition()
    {
        int currentIndex = transformIndexNumber(SwipeScrollRect.FindClosestIndexToCenter());
        Vector3 targetLocation = displayPositionList[currentIndex];
        nowMark.MoveTo(targetLocation);
    }


    private int transformIndexNumber(int realIndex)
    {
        int result = Mathf.Abs(realIndex);
        result %= scrollRectCount;
        if (realIndex < 0 && result != 0)
            result = scrollRectCount - result;
        return result;
    }

    private void dragEventTriggerInit()
    {
        swipeDragEntry = new EventTrigger.Entry();
        swipeDragEntry.eventID = EventTriggerType.Drag;
        swipeEventTrigger.triggers.Add(swipeDragEntry);
    }

    /// <summary>
    /// Add drag listener
    /// </summary>
    /// <param name="action"></param>
    /// <param name="target"></param>
    private void addDragListener(Action action)
    {
        swipeDragEntry.callback.AddListener((eventData) => { action(); });
    }

    private void RemoveAllDragEventTrigger()
    {
        swipeDragEntry.callback.RemoveAllListeners();
    }


    private void endDragEventTriggerInit()
    {
        swipeEndDragEntry = new EventTrigger.Entry();
        swipeEndDragEntry.eventID = EventTriggerType.EndDrag;
        swipeEventTrigger.triggers.Add(swipeEndDragEntry);
    }

    /// <summary>
    /// Add end drag listener
    /// </summary>
    /// <param name="action"></param>
    /// <param name="target"></param>
    private void addEndDragListener(Action<UISkillInfo> action, UISkillInfo target)
    {
        swipeEndDragEntry.callback.AddListener((eventData) => { action(target); });
    }
    #endregion
    #region  Skill Information
    /// <summary>
    /// 顯示現在選擇的Skill Information
    /// </summary>
    /// <param name="target"></param>
    private void showCenterSkillInfo(UISkillInfo target)
    {
        int currentIndex = transformIndexNumber(SwipeScrollRect.FindClosestIndexToCenter());
        var id = skillIDList[currentIndex].skillId;
        target.Init(id);
    }

    private void showTargetIndexSkill(UISkillInfo target, int index)
    {
        var id = skillIDList[index].skillId;
        target.Init(id);
    }

    /// <summary>
    /// 顯示替換 SKill與當前選擇Skill的關係與資訊
    /// </summary>
    /// <param name="target"></param>
    private void SetupUpdateSkillBlock(UISkillInfo target)
    {

        chagneSkillPage.ResetStateUI();
        int currentIndex = transformIndexNumber(SwipeScrollRect.FindClosestIndexToCenter());
        int centerID = skillIDList[currentIndex].skillId;

        switch (skillManager.GetSkillChangeState(centerID, changeID))
        {
            case SkillChangeStateEnum.LevelUp:
                int levelupID = skillManager.GetLeveUpID(centerID);
                // await skill.OpenChangeSkillPage(saveManager.GetContainer<NetworkSaveBattleSkillContainer>().GetSortedActorSkillList();
                var levelupIsUse = saveManager.GetContainer<NetworkSaveBattleSkillContainer>().GetData(currentIndex).isUsed;
                if (levelupID == -1)
                {
                    chagneSkillPage.OpenMaxLevelUI();
                    return;
                }
                var levelUpData = dataTableManager.GetSkillDefine(levelupID);
                target.Init(levelupID);
                chagneSkillPage.OpenLevelUpUI();
                chagneSkillPage.AddLevelUpButtonListener(async () =>
                {
                    ActorSkill newSkill = new ActorSkill()
                    {
                        skillId = levelupID,
                        isUsed = levelupIsUse,
                        originIndex = currentIndex,
                    };
                    await sdk.BattleReplaceSkill(currentIndex, newSkill);
                    //battleManager.UpdateSkill(battleManager.player.baseSkills, currentIndex, levelupID);
                    SetChangeResult(true);
                    uIManager.RemoveUI<UISkill>();
                });
                break;
            case SkillChangeStateEnum.Replace:
                var replaceData = dataTableManager.GetSkillDefine(changeID);
                var replaceIsUse = saveManager.GetContainer<NetworkSaveBattleSkillContainer>().GetData(currentIndex).isUsed;
                target.Init(changeID);
                chagneSkillPage.OpenReplaceUI();
                chagneSkillPage.AddReplaceButtonListener(async () =>
                {
                    ActorSkill newSkill = new ActorSkill()
                    {
                        skillId = changeID,
                        isUsed = replaceIsUse,
                        originIndex = currentIndex,
                    };
                    await sdk.BattleReplaceSkill(currentIndex, newSkill);
                    //battleManager.UpdateSkill(battleManager.player.baseSkills, currentIndex, changeID);
                    SetChangeResult(true);
                    uIManager.RemoveUI<UISkill>();
                });
                break;
        }
    }

    /// <summary>
    /// Give external information wheather the skill list has been changed.
    /// </summary>
    /// <returns></returns>
    public UniTask<bool> IsSkillsChange()
    {
        lock (_lock)
        {
            if (changeResultTask == null || changeResultTask.Task.Status == UniTaskStatus.Canceled || changeResultTask.Task.Status == UniTaskStatus.Faulted)
            {
                changeResultTask = new UniTaskCompletionSource<bool>();
            }
            else if (changeResultTask.Task.Status == UniTaskStatus.Pending)
            {
                // 正在進行中的任務
                changeResultTask.TrySetCanceled();
                Debug.LogWarning("The changeResultTask had exit");

            }
        }
        return changeResultTask.Task;
    }

    private void SetChangeResult(bool result)
    {
        lock (_lock)
        {
            if (changeResultTask != null)
            {
                changeResultTask.TrySetResult(result);
                changeResultTask = null;
            }
        }
    }

    public UniTask<int> SelectedGroupIndex()
    {
        lock (_chooseLock)
        {
            if (chooseResultTask == null || chooseResultTask.Task.Status == UniTaskStatus.Canceled || chooseResultTask.Task.Status == UniTaskStatus.Faulted)
            {
                chooseResultTask = new UniTaskCompletionSource<int>();
            }
            else if (chooseResultTask.Task.Status == UniTaskStatus.Pending)
            {
                // 正在進行中的任務
                chooseResultTask.TrySetCanceled();
                Debug.LogWarning("The chooseResultTask had exit");

            }
        }
        return chooseResultTask.Task;
    }

    private void SetGroupResult(int index)
    {
        lock (_changeLock)
        {
            if (chooseResultTask != null)
            {
                chooseResultTask.TrySetResult(index);
                chooseResultTask = null;
            }
        }
    }
    #endregion

    #region Skill Set Bar

    /// <summary>
    /// 設定技能設置欄的按鈕與其事件，最多可以同時設置四個。
    /// </summary>
    /// <param name="skillGroups">限制輸入的 SkillGroups最多只有四個</param>
    private void SetSetBar(List<SDKProtocol.SkillGroup> skillGroups)
    {
        chooseSkillSetPage.RemoveAllSetItemButtonListener();
        chooseSkillSetPage.ResetSkillSetItem();
        // 設置個別 Page set Button
        for (int i = 0; i < skillGroups.Count; i++)
        {
            chooseSkillSetPage.SetTargetSetItemButton(i + groupHeadIndex + 1, i);
            var group = skillGroups[i];
            var index = i;
            chooseSkillSetPage.AddTargetSetItemButtonListener(async () =>
            {
                chooseSkillSetPage.TurnOffAllSetItemButton();
                CommonPageFadeOut();
                endDragEventTriggerInit();
                dragEventTriggerInit();
                chooseSkillSetPage.skillSetName.text = group.Name;
                chooseSkillSetPage.ResetAllSelectImage();
                chooseSkillSetPage.TurnOnTargetSelectImage(index);
                chooseGroupIndex = index;
                CommonPageFadeIn();

                setupManaPool(group.Skills);
                RemoveAllDragEventTrigger();

                await createSkillBar(ChangeToActorSKills(group.Skills));
                addDragListener(UpdateMarkPosition);
                chooseSkillSetPage.TurnOnAllSetItemButton();
            }, i);
        }
    }


    /// <summary>
    /// Get 4 skill groups data according to the current page head number.
    /// </summary>
    /// <param name="skillGroups">輸入玩家所有的Skill Groups</param>
    /// <returns></returns>
    private List<SDKProtocol.SkillGroup> GetCurrentPageSkillGroups(List<SDKProtocol.SkillGroup> skillGroups)
    {
        List<SDKProtocol.SkillGroup> cureentPageGroup = new List<SDKProtocol.SkillGroup>();

        for (int i = groupHeadIndex; i < groupHeadIndex + 4 && i < skillGroups.Count; i++)
        {
            cureentPageGroup.Add(skillGroups[i]);
        }

        return cureentPageGroup;

    }

    /// <summary>
    /// 設定 Next & Prev Page Button。
    /// </summary>
    private void SetupChangePageButton()
    {
        chooseSkillSetPage.nextPageButton.onClick.RemoveAllListeners();
        chooseSkillSetPage.prevPageButton.onClick.RemoveAllListeners();
        if (CanClickNextButton())
        {
            chooseSkillSetPage.nextPageButton.interactable = true;
            chooseSkillSetPage.nextPageButton.onClick.AddListener(() =>
            {
                groupHeadIndex += 4;
                currentGroup = GetCurrentPageSkillGroups(totalSkillGroups);
                SetSetBar(currentGroup);
                chooseSkillSetPage.ClickTargetSetButton(0);
                SetupChangePageButton();
            });
        }
        else
        {
            chooseSkillSetPage.nextPageButton.interactable = false;
        }

        if (CanClickPrevButton())
        {
            chooseSkillSetPage.prevPageButton.interactable = true;
            chooseSkillSetPage.prevPageButton.onClick.AddListener(() =>
            {
                groupHeadIndex -= 4;
                currentGroup = GetCurrentPageSkillGroups(totalSkillGroups);
                SetSetBar(currentGroup);
                chooseSkillSetPage.ClickTargetSetButton(0);
                SetupChangePageButton();
            });
        }
        else
        {
            chooseSkillSetPage.prevPageButton.interactable = false;
        }
    }

    /// <summary>
    /// 用目前 Page Head(groupHeadIndex)判斷是否有下一頁
    /// </summary>
    /// <returns></returns>
    private bool CanClickNextButton()
    {
        return (totalSkillGroups.Count - groupHeadIndex) > 4;
    }

    /// <summary>
    /// 用目前 Page Head(groupHeadIndex)判斷是否有或上一頁
    /// </summary>
    /// <returns></returns>
    private bool CanClickPrevButton()
    {
        return (groupHeadIndex - 4) >= 0;
    }

    private void setupManaPool(List<int> skillGroup)
    {
        int red = 0, green = 0, blue = 0;

        foreach (var skillID in skillGroup)
        {
            var SkillData = dataTableManager.GetSkillDefine(skillID);
            foreach (var color in SkillData.poolColor)
            {
                switch (color)
                {
                    case SkillCostColorEnum.Red:
                        red++;
                        break;
                    case SkillCostColorEnum.Green:
                        green++;
                        break;
                    case SkillCostColorEnum.Blue:
                        blue++;
                        break;
                }
            }
        }
        chooseSkillSetPage.SetManaPool(red, green, blue);
    }

    #endregion


    /// <summary>
    /// Create Player Unlock Skill Button
    /// </summary>
    /// <param name="skillIDList"></param>
    private void createPlayerOwnSkill(List<int> skillIDList)
    {
        foreach (var id in skillIDList)
        {
            int skillID = id;
            var data = dataTableManager.GetSkillDefine(id);
            var skillItem = chooseSkillSetPage.CreatePlayerSkill(data.skillName, data.icon);
            if (CanLongPress)
            {
                UISkillPopupInfoPage popUpPage;
                skillItem.ButtonLongPress.onLongPress.RemoveAllListeners();
                skillItem.ButtonLongPress.onLongPress.AddListener(async () =>
                {
                    popUpPage = await uIManager.OpenUI<UISkillPopupInfoPage>();
                    popUpPage.Init(skillID);
                    popUpPage.CancelButton.onClick.AddListener(() =>
                    {
                        Destroy(popUpPage.gameObject);
                    });
                });
            }
            skillItemsList.Add(skillItem);
        }
    }

    /// <summary>
    /// Destory Player Unlock Skill Button
    /// </summary>
    private void ResetPlayerOwnSKill()
    {
        foreach (var item in skillItemsList)
        {
            Destroy(item.gameObject);
        }
        skillItemsList.Clear();
    }

    public List<ActorSkill> ChangeToActorSKills(List<int> intList)
    {
        var list = new List<ActorSkill>();
        // 對 Dictionary 的鍵進行排序並提取值
        for (int i = 0; i < intList.Count; i++)
        {
            list.Add(new ActorSkill()
            {
                skillId = intList[i],
                isUsed = false,
                originIndex = i,
            });
        }
        return list;
    }
    /// <summary>
    /// 把 ActorSkill List轉為 Int List
    /// </summary>
    /// <param name="actorSkills"></param>
    /// <returns></returns>
    private List<int> ChangeToSkillIDList(List<ActorSkill> actorSkills)
    {
        List<int> skillIDList = new List<int>();
        foreach (var skill in actorSkills)
        {
            skillIDList.Add(skill.skillId);
        }
        return skillIDList;

    }
    public override async void OnClose()
    {
        RxEventBus.UnRegister(this);
        await thisPageFadeOut();
        assetManager.ClearObjectPool<UISkillSwipeItem>();
        base.OnClose();
    }

}
