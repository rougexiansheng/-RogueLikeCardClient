using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIChest : UIBase
{
    [Inject]
    UIManager uIManager;
    [Inject]
    BattleManager battleManager;

    #region Inspector Setting
    protected float fadeInValue = 1f;
    [SerializeField]
    protected float fadeOutValue = 0f;
    [SerializeField]
    protected float fadeTime = 0.7f;
    [SerializeField]
    protected float delayTime = 0.75f;
    [SerializeField]
    protected Image m_imageSceneChest;
    [SerializeField]
    protected ParticleItem m_ChestBoomVFX;
    [SerializeField]
    protected GameObject m_objChestBackground;
    [SerializeField]
    protected Image m_imageUiChest;
    [SerializeField]
    protected GameObject m_panelChestInfo;
    [SerializeField]
    protected Button m_buttonSkill;
    [SerializeField]
    protected Button m_buttonSkip;
    [SerializeField]
    protected List<UIViewItem> m_chestItemList;

    [SerializeField]
    protected TMP_Text titleText;
    [SerializeField]
    public UITopBar uITopBar;

    public UniTaskCompletionSource<ViewItemData> selectTask;
    #endregion


    public bool IsDone { get; private set; }

    public override UniTask OnOpen()
    {
        IsDone = false;

        var color = Color.white;
        color.a = 0;

        m_imageSceneChest.color = color;
        m_imageUiChest.color = color;
        m_ChestBoomVFX.gameObject.SetActive(false);
        m_objChestBackground.SetActive(false);
        m_imageUiChest.gameObject.SetActive(false);

        m_panelChestInfo.SetActive(false);
        m_chestItemList.ForEach(chest =>
        {
            chest.BindClick(OnChestItemClick);
            chest.gameObject.SetActive(false);
        });

        m_buttonSkill.onClick.AddListener(OnSkillButtonClick);
        m_buttonSkip.onClick.AddListener(OnSkipChest);

        return base.OnOpen();
    }

    public async UniTask Init(List<ViewItemData> viewItemData)
    {
        titleText.text = "獲得獎勵";
        var sequence = DOTween.Sequence();
        sequence.Join(m_imageSceneChest.DOFade(fadeInValue, fadeTime));
        await sequence.AsyncWaitForCompletion();
        sequence.Kill();

        m_ChestBoomVFX.gameObject.SetActive(true);
        m_ChestBoomVFX.Play();
        await UniTask.Delay((int)(m_ChestBoomVFX.ParticleSystemLength() * 1000)); // 配合特效時間

        m_objChestBackground.SetActive(true);
        m_imageUiChest.gameObject.SetActive(true);

        sequence = DOTween.Sequence();
        sequence.Join(m_imageUiChest.DOFade(fadeInValue, 1));
        await sequence.AsyncWaitForCompletion();
        sequence.Kill();

        m_panelChestInfo.SetActive(true);

        for (int i = 0; i < m_chestItemList.Count; i++)
        {
            if (viewItemData.Count > i)
            {
                m_chestItemList[i].SetViewItemData(viewItemData[i]);
                m_chestItemList[i].gameObject.SetActive(true);
            }
            else
            {
                m_chestItemList[i].gameObject.SetActive(false);
            }
        }
    }

    public void InitWitoutPerformance(List<ViewItemData> viewItemData)
    {

        titleText.text = "三選一獎勵技能";
        m_objChestBackground.SetActive(true);
        m_panelChestInfo.SetActive(true);

        for (int i = 0; i < m_chestItemList.Count; i++)
        {
            if (viewItemData.Count > i)
            {
                m_chestItemList[i].SetViewItemData(viewItemData[i]);
                m_chestItemList[i].gameObject.SetActive(true);
            }
            else
            {
                m_chestItemList[i].gameObject.SetActive(false);
            }
        }
    }

    public UniTask<ViewItemData> SelectSkill()
    {
        if (selectTask == null || selectTask.Task.Status == UniTaskStatus.Canceled || selectTask.Task.Status == UniTaskStatus.Faulted)
        {
            selectTask = new UniTaskCompletionSource<ViewItemData>();
        }
        else if (selectTask.Task.Status == UniTaskStatus.Pending)
        {
            // 正在進行中的任務
            selectTask.TrySetCanceled();

            Debug.LogWarning("The chooseResultTask had exit");

        }
        return selectTask.Task;
    }

    private async void OnSkillButtonClick()
    {
        var ui = await uIManager.OpenUI<UISkill>();
        await ui.OpenCheckSkillPage(battleManager.player.skills);
    }

    public void ResetTask()
    {
        if (selectTask.Task.Status == UniTaskStatus.Succeeded)
            selectTask = null;
    }

    public void Done()
    {
        IsDone = true;
    }
    private void OnSkipChest()
    {
        IsDone = true;
        selectTask?.TrySetResult(null);
    }

    private void OnChestItemClick(ViewItemData viewItemData)
    {
        selectTask?.TrySetResult(viewItemData);
    }
}
