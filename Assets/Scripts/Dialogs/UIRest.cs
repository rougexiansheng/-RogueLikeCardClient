using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIRest : UIBase
{
    [Inject]
    UIManager uIManager;

    [SerializeField]
    protected float fadeInValue = 1f;
    [SerializeField]
    protected float fadeOutValue = 0f;
    [SerializeField]
    protected float fadeOutTime = 0.7f;
    /// <summary>
    /// 開啟 UI 後要等待多久才實際執行恢復
    /// </summary>
    [SerializeField]
    protected float waitRecoverTime = 2f;

    [SerializeField]
    protected ParticleItem m_campFireVFX;

    [SerializeField]
    protected CanvasGroup m_canvasGroup; 

    [SerializeField]
    protected UIRestButton m_restButtonGameObject;

    [SerializeField]
    protected List<Sprite> m_campImageList;

    [SerializeField]
    protected GameObject m_effectRecover;

    private (int, int) m_restData;

    public override UniTask OnOpen()
    {
        m_campFireVFX.gameObject.SetActive(false);
        m_effectRecover.SetActive(false);
        m_canvasGroup.alpha = 0f;
        return base.OnOpen();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    /// <summary>
    /// 開啟UI，FadeIn
    /// </summary>
    /// <param name="campEnum"></param>
    /// <param name="restValue"></param>
    /// <returns></returns>
    public async UniTask Init(int campEnum, int restValue)
    {
        m_restData = (campEnum, restValue);
        CreateCamp(m_restData.Item1);

        await AutoRecoverTask();
    }

    public async UniTask RecoverFinish()
    {
        await FadeOut();
        uIManager.RemoveUI<UIRest>();
        await UniTask.WaitUntil(() => this == null);
    }

    private async UniTask AutoRecoverTask()
    {
        await FadeIn();

        m_campFireVFX.gameObject.SetActive(true);
        m_campFireVFX.Play();

        await UniTask.Delay((int)
            (Mathf.Max(m_campFireVFX.ParticleSystemLength(), waitRecoverTime) * 1000)); // 配合Max (特效時間/等待時間)
    }    
    
    private UIRestButton CreateCamp(int campEnum) 
    {
        if (m_campImageList != null && m_campImageList.Count > 0)
        {
            m_restButtonGameObject.Init(m_campImageList[m_campImageList.Count > campEnum ? campEnum : 0]);
        }
        else
        {
            // woring: no image source
            m_restButtonGameObject.Init(null);
        }
            
        return m_restButtonGameObject;
    }

    /// <summary>
    /// UI Fade In Function
    /// </summary>
    /// <returns></returns>
    private async UniTask FadeIn()
    {
        var sequence = DOTween.Sequence();
        sequence.Join(m_canvasGroup.DOFade(fadeInValue, fadeOutTime));

        await sequence.AsyncWaitForCompletion();
    }

    /// <summary>
    /// UI Fade Out Function
    /// </summary>
    /// <returns></returns>
    private async UniTask FadeOut()
    {
        var sequence = DOTween.Sequence();
        sequence.Join(m_canvasGroup.DOFade(fadeOutValue, fadeOutTime));

        await sequence.AsyncWaitForCompletion();
    }

    public void ShowEffect()
    {
        m_effectRecover.SetActive(true);
    }
}
