using Cysharp.Threading.Tasks;
using DG.Tweening;
using SDKProtocol;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIAntique : UIBase
{
    [Inject]
    UIManager uIManager;
    [Inject]
    NetworkSaveManager saveManager;
    [Inject]
    BattleManager battleManager;

    #region Inspector Setting
    [SerializeField]
    protected float fadeInValue = 1f;
    [SerializeField]
    protected float fadeOutValue = 0f;
    [SerializeField]
    protected float fadeOutTime = 0.7f;
    [SerializeField]
    protected float delayTime = 0.75f;
    [SerializeField]
    protected GameObject m_panelAntiqueInfo;
    [SerializeField]
    protected GameObject m_objAntiqueBackground;
    [SerializeField]
    protected Image m_imageScavengers;
    [SerializeField]
    protected Image m_imageAntiqueHeap;
    [SerializeField]
    protected Button m_buttonSkill;
    [SerializeField]
    protected Button m_buttonSkip;
    [SerializeField]
    protected UIViewItem m_antiquesItem;
    [SerializeField]
    protected List<Sprite> m_antiqueHeapImageList;
    #endregion

    #region private member
    public bool IsDone { get; private set; }
    private Action<ViewItemData> m_onItemClicked;
    #endregion

    public override UniTask OnOpen()
    {
        IsDone = false;

        var color = m_imageAntiqueHeap.color;
        color.a = 0;
        m_imageAntiqueHeap.color = color;
        m_imageAntiqueHeap.gameObject.SetActive(false);

        color = m_imageScavengers.color;
        color.a = 0;
        m_imageScavengers.color = color;

        m_imageScavengers.gameObject.SetActive(false);
        m_panelAntiqueInfo.SetActive(false);
        m_objAntiqueBackground.SetActive(false);


        m_buttonSkill.onClick.AddListener(OnSkillButtonClick);
        m_buttonSkip.onClick.AddListener(OnSkipAntique);
        m_antiquesItem.BindClick(OnAntiqueItemClick);

        return base.OnOpen();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    public async UniTask Init(List<ViewItemData> viewItemData, Action<ViewItemData> onItemClick)
    {
        m_onItemClicked = onItemClick;
        var antiqueHeapCount = m_antiqueHeapImageList.Count;
        if (antiqueHeapCount > 0)
        {
            m_imageAntiqueHeap.sprite = m_antiqueHeapImageList[0];
            m_imageAntiqueHeap.gameObject.SetActive(true);
            var sequence = DOTween.Sequence();
            sequence.Join(m_imageAntiqueHeap.DOFade(fadeInValue, fadeOutTime));
            await sequence.AsyncWaitForCompletion();
            sequence.Kill();
        }

        await UniTask.Delay((int)(delayTime * 1000)); // 短暫等待避免遺跡剛出來就馬上黑屏

        m_objAntiqueBackground.SetActive(true);
        m_imageScavengers.gameObject.SetActive(true);
        var sequenceScavenger = DOTween.Sequence();
        sequenceScavenger.Join(m_imageScavengers.DOFade(fadeInValue, fadeOutTime));
        await sequenceScavenger.AsyncWaitForCompletion();
        sequenceScavenger.Kill();

        await UniTask.Delay((int)(delayTime * 1000)); // 短暫等待

        if (viewItemData != null && viewItemData.Count > 0)
        {
            m_antiquesItem.SetViewItemData(viewItemData[0]);
            m_antiquesItem.gameObject.SetActive(true);
        }
        else
        {
            m_antiquesItem.gameObject.SetActive(false);
        }


        m_panelAntiqueInfo.SetActive(true);
    }

    private async void OnSkillButtonClick()
    {
        var ui = await uIManager.OpenUI<UISkill>();
        await ui.OpenCheckSkillPage(battleManager.player.skills);
    }

    private void OnAntiqueItemClick(ViewItemData viewItemData)
    {
        m_onItemClicked?.Invoke(viewItemData);
        IsDone = true;
    }


    private void OnSkipAntique()
    {
        IsDone = true;
    }
}
