using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIShop : UIBase
{
    [Inject]
    UIManager uIManager;
    [Inject]
    NetworkSaveManager saveManager;
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
    protected Image m_imageSceneShop;
    [SerializeField]
    protected GameObject m_objShopBackground;
    [SerializeField]
    protected Image m_imageUiShop;
    [SerializeField]
    protected GameObject m_panelShopInfo;
    [SerializeField]
    protected Button m_buttonSkill;
    [SerializeField]
    protected Button m_buttonSkip;
    [SerializeField]
    protected List<UIViewItem> m_shopItemList;
    #endregion



    public UniTaskCompletionSource<ViewItemData> selectTask;
    public bool IsDone { get; private set; }

    [SerializeField]
    public ParticleItem HealParticle;

    public override UniTask OnOpen()
    {
        IsDone = false;

        var color = Color.white;
        color.a = 0;

        m_imageSceneShop.color = color;
        m_imageUiShop.color = color;
        m_objShopBackground.SetActive(false);
        m_imageUiShop.gameObject.SetActive(false);

        m_panelShopInfo.SetActive(false);
        m_shopItemList.ForEach(shopItem =>
        {
            shopItem.BindClick(OnShopItemClick);
            shopItem.gameObject.SetActive(false);
        });

        m_buttonSkill.onClick.AddListener(OnSkillButtonClick);
        m_buttonSkip.onClick.AddListener(OnSkipChest);

        return base.OnOpen();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    public async UniTask Init(List<ViewItemData> viewItemList)
    {
        var sequence = DOTween.Sequence();
        sequence.Join(m_imageSceneShop.DOFade(fadeInValue, fadeTime));
        await sequence.AsyncWaitForCompletion();
        sequence.Kill();

        m_objShopBackground.SetActive(true);
        m_imageUiShop.gameObject.SetActive(true);

        sequence = DOTween.Sequence();
        sequence.Join(m_imageSceneShop.DOFade(fadeOutValue, 1));
        sequence.Join(m_imageUiShop.DOFade(fadeInValue, 1));
        await sequence.AsyncWaitForCompletion();
        sequence.Kill();

        m_panelShopInfo.SetActive(true);

        for (int i = 0; i < m_shopItemList.Count; i++)
        {
            if (viewItemList.Count > i)
            {
                m_shopItemList[i].SetViewItemData(viewItemList[i]);
                m_shopItemList[i].TurnOnShopMode();
                m_shopItemList[i].gameObject.SetActive(true);
            }
            else
            {
                m_shopItemList[i].gameObject.SetActive(false);
            }
        }
    }

    public UniTask<ViewItemData> SelectItem()
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
    public void ResetTask()
    {
        if (selectTask.Task.Status == UniTaskStatus.Succeeded)
            selectTask = null;
    }
    private async void OnSkillButtonClick()
    {
        var ui = await uIManager.OpenUI<UISkill>();
        await ui.OpenCheckSkillPage(battleManager.player.skills);
    }
    private void OnSkipChest()
    {
        IsDone = true;
        selectTask.TrySetResult(null);

    }
    private void OnShopItemClick(ViewItemData viewItemData)
    {
        selectTask.TrySetResult(viewItemData);
    }
}
