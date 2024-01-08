using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SDKProtocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UIChooseCharactor : UIBase
{
    [Inject]
    UIManager uIManager;

    [Inject]
    DataManager dataManager;

    [SerializeField]
    public int DungeonGroupId = 1;

    [SerializeField]
    private TMP_Text charNameText;

    [SerializeField]
    private Button chooseSkillGroupButton;

    [SerializeField]
    private Button startButton;

    [SerializeField]
    private EventTrigger swipeEventTrigger;

    private List<Vector3> cardsGeneratePoints = new List<Vector3>();

    [SerializeField]
    private CharCard CharCardPrefab;

    [SerializeField]
    private Transform charListRoot;

    private Dictionary<ActorProfessionEnum, CharCard> charCardsDic = new Dictionary<ActorProfessionEnum, CharCard>();


    [SerializeField]
    private Transform circuleCenter;

    [SerializeField]
    private Transform circulePoint;


    [SerializeField]
    private int totalPosition = 9;
    private int totalCard = 3;

    public float testAngle = 30f;

    public int testNum = 3;

    private int chooseCharacterID = 0;

    private SelectProfessionData selectCharacterData = new SelectProfessionData();
    private UniTaskCompletionSource<(int, SelectProfessionData)> chooseResultTask;
    private object _lock = new object();
    private Vector2 startPointerPosition;

    public float minSwipeDistance = 60f;

    public float cardMoveSpeed = 0.1f;
    DG.Tweening.Sequence sequence;


    [SerializeField]
    private List<GameObject> CharacterGameobjects = new List<GameObject>();
    [SerializeField]
    private List<Transform> spineAnchor = new List<Transform>();
    private Dictionary<ActorProfessionEnum, PlayerProfessionData> professionDataDic = new Dictionary<ActorProfessionEnum, PlayerProfessionData>();

    [SerializeField]
    private List<SpineCharacterCtrl> spineCharacterCtrls = new List<SpineCharacterCtrl>();

    public override UniTask OnOpen()
    {
        return base.OnOpen();
    }

    public void Init(List<int> unlockCharacterIds, Dictionary<ActorProfessionEnum, PlayerProfessionData> playerProfessionDataDic)
    {
        //RemoveAllDragEventTrigger();
        professionDataDic = playerProfessionDataDic;
        // 9 / 35 - 8 / 30
        resetCharacter();
        SetupGeneratePoints(testAngle);
        UnLockCharactor(unlockCharacterIds);
        SwipeCardToTargetCharacter(chooseCharacterID);
        InitSwipeEventTrigger();
        SetupStartButton();

    }


    #region Card
    public UniTask<(int, SelectProfessionData)> CharacterChoose()
    {
        lock (_lock)
        {
            if (chooseResultTask == null || chooseResultTask.Task.Status == UniTaskStatus.Canceled || chooseResultTask.Task.Status == UniTaskStatus.Faulted)
            {
                chooseResultTask = new UniTaskCompletionSource<(int, SelectProfessionData)>();
            }
            else if (chooseResultTask.Task.Status == UniTaskStatus.Pending)
            {
                // 正在進行中的任務
                chooseResultTask.TrySetCanceled();
                Debug.LogWarning("The changeResultTask had exit");

            }
        }
        return chooseResultTask.Task;
    }

    private void SetCharacterResult(int characterID, SelectProfessionData selectData)
    {
        lock (_lock)
        {
            if (chooseResultTask != null)
            {
                chooseResultTask.TrySetResult((characterID, selectData));
                chooseResultTask = null;
            }
        }
    }

    private void UnLockCharactor(List<int> unlockCharacterIds)
    {
        totalCard = 3;
        //chooseCharacterID = unlockCharacterIds[unlockCharacterIds.Count / 2];
        chooseCharacterID = 1;
        for (int i = 1; i < totalCard + 1; i++)
        {
            InstantiateCharCard((ActorProfessionEnum)(i));
        }
        foreach (var id in unlockCharacterIds)
        {
            charCardsDic[(ActorProfessionEnum)id].UnlockCard();
        }

    }
    private void SetupGeneratePoints(float angle)
    {
        Vector3 center = circuleCenter.localPosition;
        Vector3 point = circulePoint.localPosition;
        Vector3 startVect = RotatePointAroundPivot(point, center, angle);
        Vector3 EndVect = RotatePointAroundPivot(point, center, -angle);
        for (int j = 0; j < totalPosition; j++)
        {
            float lerpValue = (float)1f / (totalPosition - 1);
            Vector3 offset = Vector3.Slerp(startVect - center, EndVect - center, j * lerpValue);
            cardsGeneratePoints.Add(offset + center);
        }
    }
    private void InstantiateCharCard(ActorProfessionEnum professionEnum)
    {
        var charCard = Instantiate(CharCardPrefab);
        charCard.transform.SetParent(charListRoot, false);
        charCard.Init(professionEnum, swipeEventTrigger);
        charCard.SetCanvasSortOrder(1);
        charCard.SetOnClick(OnActorProfessionClick);
        charCardsDic.Add(professionEnum, charCard);
    }

    private void OnActorProfessionClick(ActorProfessionEnum professionEnum)
    {
        //Debug.Log($"OnActorProfessionClick: {professionEnum}");
        chooseCharacterID = (int)professionEnum;
        SwipeCardToTargetCharacter(chooseCharacterID);
    }

    /// <summary>
    /// 滑動到指定 ID 卡片讓其進行置中，並進行資料設定
    /// </summary>
    /// <param name="targetID"></param>
    private void SwipeCardToTargetCharacter(int targetID)
    {
        if (sequence == null) sequence = DOTween.Sequence();
        chooseSkillGroupButton.onClick.RemoveAllListeners();
        resetCards();
        resetCharacter();
        int circuleCenterIndex = (totalPosition - 1) / 2;
        int start = circuleCenterIndex - (targetID - 1);
        int end = start + charCardsDic.Count;
        //9
        for (int j = start; j < end; j++)
        {
            if (totalPosition <= j)
            {
                charCardsDic[changeListIndextoProfessionEnum(j - start)].gameObject.SetActive(false);
                sequence.Join(charCardsDic[changeListIndextoProfessionEnum(j - start)].transform.DOLocalMove(cardsGeneratePoints[totalPosition - 1], cardMoveSpeed).SetEase(Ease.OutExpo));
            }
            else if (j <= 0)
            {
                charCardsDic[changeListIndextoProfessionEnum(Mathf.Abs(j - start))].gameObject.SetActive(false);
                sequence.Join(charCardsDic[changeListIndextoProfessionEnum(Mathf.Abs(j - start))].transform.DOLocalMove(cardsGeneratePoints[0], cardMoveSpeed).SetEase(Ease.OutExpo));
            }
            else
            {
                sequence.Join(charCardsDic[changeListIndextoProfessionEnum(j - start)].transform.DOLocalMove(cardsGeneratePoints[j], cardMoveSpeed).SetEase(Ease.OutExpo));
            }
        }
        SortCardLayer(targetID);
        var playerCharData = professionDataDic[(ActorProfessionEnum)targetID];
        SetupSkillGroupButton(playerCharData.SkillGroups, playerCharData.UnlockSkillIDs, playerCharData.DefalutProfessionDatas.SkillGroupsIndex);
        if (playerCharData.DefalutProfessionDatas != null)
        {
            selectCharacterData = playerCharData.DefalutProfessionDatas;
        }
        else
        {
            selectCharacterData.CharacterSkinId = 0;
            selectCharacterData.SkillGroupsIndex = 0;
        }

        charNameText.text = charCardsDic[(ActorProfessionEnum)(targetID)].profession.ToString();
        charCardsDic[(ActorProfessionEnum)(targetID)].Select();
        //TODO - 上一次腳色造型設定
        OpenCharacter(targetID);
    }


    /// <summary>
    /// 讓卡片疊層進行適當排列，會依據指定Index當作最前排 
    /// </summary>
    /// <param name="centerID"></param>
    private void SortCardLayer(int centerID)
    {
        int cardsNum = charCardsDic.Count;
        int highestLayer = (int)(Mathf.Ceil((float)totalPosition));

        for (int i = 0; i < charCardsDic.Count; i++)
        {
            // -1 because change character ID to position index
            int order = Mathf.Abs(centerID - 1 - i);
            order = Mathf.Abs(highestLayer - order);
            charCardsDic[(ActorProfessionEnum)(i + 1)].SetCanvasSortOrder(order);
        }
    }

    private void resetCardLayer()
    {
        for (int i = 0; i < charCardsDic.Count; i++)
        {
            charCardsDic[(ActorProfessionEnum)(i + 1)].SetCanvasSortOrder(0);
        }
    }

    /// <summary>
    /// Change Position List index to profession enum
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private ActorProfessionEnum changeListIndextoProfessionEnum(int index)
    {
        return (ActorProfessionEnum)(index + 1);
    }

    /// <summary>
    /// 把所有卡片開啟
    /// </summary>
    private void resetCards()
    {
        foreach (var card in charCardsDic)
        {
            card.Value.gameObject.SetActive(true);
            card.Value.UnSelect();
        }
    }
    #endregion

    #region Character
    private void resetCharacter()
    {
        foreach (var character in CharacterGameobjects)
        {
            character.gameObject.SetActive(false);
        }
        foreach (var spine in spineCharacterCtrls)
        {
            spine.SetSkin(SpineCharacterCtrl.SpineSkinEnum.Origin);
            //spine.PlayAnimation(SpineAnimationEnum.None);
        }
    }


    private void OpenCharacter(int targetID)
    {
        CharacterGameobjects[targetID - 1].gameObject.SetActive(true);
        if (spineCharacterCtrls.Count < (targetID - 1))
            spineCharacterCtrls[targetID - 1].PlayIdle();

        // Character Idle
    }

    #endregion


    #region Swipe Function

    private void InitSwipeEventTrigger()
    {
        var beginDrag = createEventTriggerEntry(EventTriggerType.BeginDrag);
        beginDrag.callback.AddListener((eventData) => { onBeginDrag((PointerEventData)eventData); });
        var drag = createEventTriggerEntry(EventTriggerType.Drag);
        drag.callback.AddListener((eventData) => { onDrag((PointerEventData)eventData); });
        var endDrag = createEventTriggerEntry(EventTriggerType.EndDrag);
        endDrag.callback.AddListener((eventData) => { onEngDrag((PointerEventData)eventData); });
    }


    private EventTrigger.Entry createEventTriggerEntry(EventTriggerType type)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        swipeEventTrigger.triggers.Add(entry);
        return entry;
    }


    private void RemoveAllDragEventTrigger()
    {
        swipeEventTrigger.triggers.Clear();
    }

    private void onBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            startPointerPosition = eventData.position;
        }
    }


    private void onDrag(PointerEventData eventData)
    {
        Vector2 newPosition = eventData.position;
        float distance = Vector2.Distance(newPosition, startPointerPosition);
        float direction = newPosition.x - startPointerPosition.x;
        if (distance > minSwipeDistance)
        {
            if (direction > 0)
            {
                AddCardIndex(false);
                SwipeCardToTargetCharacter(chooseCharacterID);
            }
            else
            {
                AddCardIndex(true);
                SwipeCardToTargetCharacter(chooseCharacterID);
            }
            startPointerPosition = eventData.position;
        }
    }

    private void onEngDrag(PointerEventData eventData)
    {
        //ScrollRect a;
        //a.OnBeginDrag(eventData);
    }

    private void AddCardIndex(bool IsAdd)
    {
        if (IsAdd)
        {
            if (chooseCharacterID < totalCard)
                chooseCharacterID++;
        }
        else
        {
            if (chooseCharacterID > 1)
                chooseCharacterID--;
        }
    }


    #endregion



    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos()
    {
        Vector3 center = circuleCenter.position;
        Vector3 point = circulePoint.position;
        Vector3 startVect = RotatePointAroundPivot(point, center, testAngle);
        Vector3 EndVect = RotatePointAroundPivot(point, center, -testAngle);

        float step = ((float)testNum - 1f) / 2f;
        int start = (int)(4 - Mathf.Ceil(step));
        int end = (int)(4 + Mathf.Floor(step));


        //9
        for (int j = start; j < end + 1; j++)
        {
            float lerpValue = (float)1f / (totalPosition - 1);
            Vector3 offset = Vector3.Slerp(startVect - center, EndVect - center, j * lerpValue);
            Gizmos.DrawLine(center, center + offset);
        }
    }



    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle)
    {
        angle = angle * Mathf.Deg2Rad; // 將角度轉換為弧度
        var rotatedX = pivot.x + Mathf.Cos(angle) * (point.x - pivot.x) - Mathf.Sin(angle) * (point.y - pivot.y);
        var rotatedY = pivot.y + Mathf.Sin(angle) * (point.x - pivot.x) + Mathf.Cos(angle) * (point.y - pivot.y);
        return new Vector3(rotatedX, rotatedY, point.z);
    }



    private void SetupStartButton()
    {
        startButton.onClick.AddListener(() =>
        {
            SetCharacterResult(chooseCharacterID, selectCharacterData);
        });
    }

    private void SetupSkillGroupButton(List<SkillGroup> skillGroups, List<int> unlockSkill, int prevGroupsIndex)
    {
        chooseSkillGroupButton.onClick.AddListener(async () =>
        {
            resetCardLayer();
            var uI = await OpenSkillGroupPage(skillGroups, unlockSkill, prevGroupsIndex);

            int groupIndex = await uI.SelectedGroupIndex();
            selectCharacterData.SkillGroupsIndex = groupIndex;
            Debug.Log("Group Index: " + groupIndex);
            uIManager.RemoveUI<UISkill>();
            SortCardLayer(chooseCharacterID);
        });
    }

    private async UniTask<UISkill> OpenSkillGroupPage(List<SkillGroup> skillGroups, List<int> unlockSkill, int prevGroupsIndex)
    {
        var ui = await uIManager.OpenUI<UISkill>();
        var skill = uIManager.FindUI<UISkill>();
        uIManager.LoadingUI(false);
        await uIManager.FadeOut(0.2f);
        skill.OpenEquipmentSkillPage(skillGroups, unlockSkill, prevGroupsIndex);
        return skill;
    }

    public override void OnClose()
    {
        RemoveAllDragEventTrigger();
        base.OnClose();
    }

}
