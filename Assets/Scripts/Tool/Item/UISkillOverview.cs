using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class UISkillOverview : MonoBehaviour
{
    [SerializeField]
    private Button backButton;


    [SerializeField]
    public UISkillInfo infoItem;

    [SerializeField]
    private TMP_Text moveStepNumText;

    [SerializeField]
    private CanvasGroup infoCanvas;
    public void Init()
    {
        RemoveAllListeners();
        FadeOut();
    }

    public void SetNumText(string moveNum)
    {
        moveStepNumText.text = moveNum;
    }

    public void AddBackButtonListener(Action action)
    {
        backButton.onClick.AddListener(() =>
        {
            action();
        });
    }

    public void RemoveBackButtonListeners()
    {
        backButton.onClick.RemoveAllListeners();
    }

    public void RemoveAllListeners()
    {
        backButton.onClick.RemoveAllListeners();
    }

    public async UniTask SkillInfoFadeInAsync(float time)
    {
        await infoCanvas.DOFade(1, time).AsyncWaitForCompletion();
    }

    public void FadeOut()
    {
        infoCanvas.DOFade(0, 0);
    }

    private void OnDestroy()
    {
        RemoveAllListeners();
    }

}
