using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
public class UIMapButton : MonoBehaviour
{
    [SerializeField]
    public Image Enable;
    [SerializeField]
    public Image Disable;
    [SerializeField]
    public Image Pass;

    [SerializeField]
    private Image flash;

    private float flashScaleTime = 0.7f;



    private float scaleSize = 1.1f;

    private Sequence flashSequence;
    public enum MapButtonState
    {
        Enable,
        Disable,
        Pass,
    }

    public int MapNodeEnum;
    public MapButtonState CurrentState;


    public void Init(MapButtonData data, MapButtonState state, int nodeEnum)
    {
        this.MapNodeEnum = nodeEnum;
        setImage(Enable, data.EnableImage);
        setImage(Disable, data.DisableImage);
        setImage(Pass, data.PassImage);
        setImageSize();
        ChangeImage(state);


    }

    private void setImage(UnityEngine.UI.Image source, Sprite target)
    {
        source.sprite = target;
    }

    public void SetUIPosition(Transform root, Vector3 position)
    {
        this.transform.SetParent(root, false);
        this.transform.localPosition = position;
    }


    public void ChangeImage(MapButtonState state)
    {
        switch (state)
        {
            case MapButtonState.Enable:
                Enable.gameObject.SetActive(true);
                Disable.gameObject.SetActive(false);
                Pass.gameObject.SetActive(false);
                //flash.gameObject.SetActive(true);
                //Flash();
                break;
            case MapButtonState.Disable:
                Enable.gameObject.SetActive(false);
                Disable.gameObject.SetActive(true);
                Pass.gameObject.SetActive(false);
                flash.gameObject.SetActive(false);
                break;
            case MapButtonState.Pass:
                Enable.gameObject.SetActive(false);
                Disable.gameObject.SetActive(false);
                Pass.gameObject.SetActive(true);
                flash.gameObject.SetActive(false);
                break;

        }
        this.CurrentState = state;
    }

    public void AddListener(UnityAction func)
    {
        this.GetComponent<Button>().onClick.AddListener(func);
    }

    public void RemoveAllListerner()
    {
        this.GetComponent<Button>().onClick.RemoveAllListeners();
    }

    private void Flash()
    {
        flashSequence = DOTween.Sequence()
        .Append(flash.transform.DOScale(new Vector3(scaleSize, scaleSize, scaleSize), flashScaleTime))
        .Append(flash.DOFade(0f, flashScaleTime));
        flashSequence.SetLoops(-1, LoopType.Restart);
    }

    public Sequence FadeOut()
    {
        if (flashSequence != null)
            flashSequence.Kill();
        var sequence = DOTween.Sequence()
        .Join(flash.DOFade(0f, flashScaleTime))
        .Join(Enable.DOFade(0f, flashScaleTime))
        .Join(Disable.DOFade(0f, flashScaleTime))
        .Join(Pass.DOFade(0f, flashScaleTime));

        return sequence;
    }

    public Sequence ZoomIn(float value, float time)
    {
        var sequence = DOTween.Sequence()
        .Join(flash.transform.DOScale(value, time))
        .Join(Enable.transform.DOScale(value, time))
        .Join(Disable.transform.DOScale(value, time))
        .Join(Pass.transform.DOScale(value, time));

        return sequence;
    }

    public Sequence ZoomOut(float value, float time)
    {
        var sequence = DOTween.Sequence()
        .Join(flash.transform.DOScale(value, time))
        .Join(Enable.transform.DOScale(value, time))
        .Join(Disable.transform.DOScale(value, time))
        .Join(Pass.transform.DOScale(value, time));

        return sequence;
    }

    private void setImageSize()
    {
        flash.transform.DOScale(0f, 0).Play();
        Enable.transform.DOScale(0f, 0).Play();
        Disable.transform.DOScale(0f, 0).Play();
        Pass.transform.DOScale(0f, 0).Play();

    }

}
