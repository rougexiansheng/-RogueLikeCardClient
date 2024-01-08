using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIBattleRewardChoose : UIBase
{
    public enum RewardType
    {
        Skill,
        Coin,
        Rest,
    }

    [Inject]
    UIManager uIManager;
    [Inject]
    DataManager dataManager;
    [Inject]
    FakeServer sdk;

    [SerializeField]
    private Button skillButton;

    [SerializeField]
    private Button coinButton;

    [SerializeField]
    private Button restButton;

    [SerializeField]
    private GameObject centerPoint;

    [SerializeField]
    private GameObject lines;

    [SerializeField]
    private CanvasGroup thisUICanvas;

    [SerializeField]
    private UITopBar uITopBar;
    private Dictionary<RewardType, Button> buttonsDic = new Dictionary<RewardType, Button>();

    [SerializeField]
    private TMP_Text coinNumberText;

    [SerializeField]
    private TMP_Text restPersentText;

    [SerializeField]
    private float moveTime = 0.5f;
    [SerializeField]
    private float ScaleTime = 0.7f;
    [SerializeField]
    private float ScaleSize = 1.1f;

    [SerializeField]
    private float fadeOutValue = 0f;
    [SerializeField]
    private float fadeOutTime = 0.4f;

    [SerializeField]
    public ParticleItem HealParticle;

    [SerializeField]
    public ParticleItem HealRingParticle;

    [SerializeField]
    public ParticleItem GoldRingParticle;

    [SerializeField]
    public ParticleItem GoldParticle;

    private UniTaskCompletionSource<GameFlowChoiceRewardState.RewardChooseEnum> rewardResultTask;

    private object _rewardLock = new object();


    public override UniTask OnOpen()
    {
        return base.OnOpen();
    }


    public void Init(int coinIncreaceNumber, float restvalue)
    {
        this.coinNumberText.text = coinIncreaceNumber.ToString();
        float resetValue = restvalue;
        this.restPersentText.text = ((int)resetValue).ToString();
        setSkillOnclick();
        setCoinOnclick(coinIncreaceNumber);
        setRestOnclick();


    }

    public UniTask<GameFlowChoiceRewardState.RewardChooseEnum> ChooseReward()
    {
        lock (_rewardLock)
        {
            if (rewardResultTask == null || rewardResultTask.Task.Status == UniTaskStatus.Canceled || rewardResultTask.Task.Status == UniTaskStatus.Faulted)
            {
                rewardResultTask = new UniTaskCompletionSource<GameFlowChoiceRewardState.RewardChooseEnum>();
            }
            else if (rewardResultTask.Task.Status == UniTaskStatus.Pending)
            {
                // 正在進行中的任務
                rewardResultTask.TrySetCanceled();
                Debug.LogWarning("The rewardResultTask had exit");

            }
        }
        return rewardResultTask.Task;
    }

    private void SetRewardResult(GameFlowChoiceRewardState.RewardChooseEnum result)
    {
        lock (_rewardLock)
        {
            if (rewardResultTask != null)
            {
                rewardResultTask.TrySetResult(result);
                rewardResultTask = null;
            }
        }
    }

    private void setSkillOnclick()
    {
        buttonsDic.Add(RewardType.Skill, skillButton);
        skillButton.interactable = true;
        skillButton.onClick.AddListener(async () =>
        {
            skillButton.interactable = true;
            removeAllListerners();
            await IconMoveToCenterAsync(RewardType.Skill);
            await IconDisappearAsync(RewardType.Skill);
            SetRewardResult(GameFlowChoiceRewardState.RewardChooseEnum.Skill);



            //移至離開動畫做完
        });
    }

    private void setCoinOnclick(int coinIncreaceNumber)
    {
        buttonsDic.Add(RewardType.Coin, coinButton);
        coinButton.interactable = true;
        coinButton.onClick.AddListener(() =>
        {
            coinButton.interactable = false;
            removeAllListerners();
            SetRewardResult(GameFlowChoiceRewardState.RewardChooseEnum.Coin);
        });
    }

    private void setRestOnclick()
    {
        buttonsDic.Add(RewardType.Rest, restButton);
        restButton.interactable = true;
        restButton.onClick.AddListener(async () =>
        {
            restButton.interactable = false;
            removeAllListerners();

            SetRewardResult(GameFlowChoiceRewardState.RewardChooseEnum.Rest);

        });
    }


    public async UniTask IconMoveToCenterAsync(RewardType rewardType)
    {
        Sequence sequence = DOTween.Sequence();
        Button targetButton = null;
        foreach (var buttonDic in buttonsDic)
        {
            if (buttonDic.Key == rewardType)
            {
                targetButton = buttonDic.Value;
            }
            else
            {
                //淡入淡出
                sequence.Join(buttonDic.Value.GetComponent<CanvasGroup>().DOFade(fadeOutValue, fadeOutTime));
            }
        }
        sequence.Join(lines.GetComponent<CanvasGroup>().DOFade(fadeOutValue, fadeOutTime));

        sequence.Join(targetButton.transform.DOMove(centerPoint.transform.position, moveTime));

        sequence.Join(targetButton.transform.DOScale(new Vector3(ScaleSize, ScaleSize, ScaleSize), ScaleTime));

        await sequence.AsyncWaitForCompletion();

    }

    public async UniTask IconDisappearAsync(RewardType rewardType)
    {
        Sequence sequence = DOTween.Sequence();
        Button targetButton = null;
        foreach (var buttonDic in buttonsDic)
        {
            if (buttonDic.Key == rewardType)
                targetButton = buttonDic.Value;
        }

        sequence.Join(targetButton.GetComponent<CanvasGroup>().DOFade(fadeOutValue, fadeOutTime));

        await sequence.AsyncWaitForCompletion();

    }

    public async UniTask FadeOutThisUIPage()
    {
        await thisUICanvas.DOFade(fadeOutValue, fadeOutTime).AsyncWaitForCompletion();
    }

    public UniTask<bool> IsCoinEffectFinished()
    {
        return uITopBar.IsEffectEnd();
    }

    private void removeAllListerners()
    {
        foreach (var button in buttonsDic)
        {
            button.Value.onClick.RemoveAllListeners();
        }
    }


    public override void OnClose()
    {

        removeAllListerners();
        RxEventBus.UnRegister(this);
        base.OnClose();
    }

}
