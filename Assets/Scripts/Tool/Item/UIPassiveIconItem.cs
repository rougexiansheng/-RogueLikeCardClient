using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPassiveIconItem : MonoBehaviour
{
    [SerializeField]
    GameObject bg;
    [SerializeField]
    ParticleItem particleItem;
    public Image icon;
    public int passiveId = 0;
    Sequence sequence;
    float time = 0.5f;

    public void Clear()
    {
        if (sequence == null) sequence = DOTween.Sequence();
        if (sequence != null && sequence.IsActive()) sequence.Kill();
        if (bg != null) bg.SetActive(false);
        passiveId = 0;
        icon.color = new Color(1, 1, 1, 0);
    }
    public void DoAddAnimate()
    {
        if (bg) bg.SetActive(true);
        if (sequence == null) sequence = DOTween.Sequence();
        if (sequence != null && sequence.IsActive()) sequence.Kill();
        particleItem.Play();
        transform.localScale = Vector3.one * 3f;
        icon.color = new Color(1, 1, 1, 0);
        sequence.Join(transform.DOScale(1, time));
        sequence.Join(icon.DOFade(1, time));
    }

    public void DoRemove()
    {
        if (sequence == null) sequence = DOTween.Sequence();
        if (sequence != null && sequence.IsActive()) sequence.Kill();
        transform.localScale = Vector3.one;
        sequence.Join(transform.DOScale(3f, time));
        sequence.Join(icon.DOFade(0, time).OnComplete(() => { if (bg != null) bg.SetActive(false); }));
    }

    public void DoUpdate()
    {
        if (sequence == null) sequence = DOTween.Sequence();
        if (sequence != null && sequence.IsActive()) sequence.Kill();
        transform.localScale = Vector3.one * 3f;
        sequence.Join(transform.DOScale(1, time));
    }
}
