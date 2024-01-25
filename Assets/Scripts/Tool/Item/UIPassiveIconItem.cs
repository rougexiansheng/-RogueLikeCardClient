using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPassiveIconItem : MonoBehaviour
{
    [SerializeField]
    CanvasGroup canvasGroup;
    [SerializeField]
    ParticleItem particleItem;
    public Image icon;
    public TMPro.TMP_Text stackCount;
    public int passiveId = 0;
    Sequence sequence;
    float time = 0.5f;

    public void Clear()
    {
        if (sequence == null) sequence = DOTween.Sequence();
        if (sequence != null && sequence.IsActive()) sequence.Kill();
        gameObject.SetActive(false);
        passiveId = 0;
        canvasGroup.alpha = 0;
    }
    public void DoAddAnimate()
    {
        gameObject.SetActive(true);
        if (sequence == null) sequence = DOTween.Sequence();
        if (sequence != null && sequence.IsActive()) sequence.Kill();
        particleItem.Play();
        transform.localScale = Vector3.one * 3f;
        canvasGroup.alpha = 0;
        sequence.Join(transform.DOScale(1, time));
        sequence.Join(canvasGroup.DOFade(1, time));
    }

    public void DoRemove()
    {
        if (sequence == null) sequence = DOTween.Sequence();
        if (sequence != null && sequence.IsActive()) sequence.Kill();
        transform.localScale = Vector3.one;
        sequence.Join(transform.DOScale(3f, time));
        sequence.Join(canvasGroup.DOFade(0, time).OnComplete(() => { gameObject.SetActive(false); }));
    }

    public void DoUpdate()
    {
        if (sequence == null) sequence = DOTween.Sequence();
        if (sequence != null && sequence.IsActive()) sequence.Kill();
        transform.localScale = Vector3.one * 3f;
        sequence.Join(transform.DOScale(1, time));
    }
}
