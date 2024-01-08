using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharCard : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private List<Sprite> charSprite;

    [SerializeField]
    private Image charImage;
    [SerializeField]
    private Image lockImage;

    [SerializeField]
    private Image lightImage;

    [SerializeField]
    private Canvas thisCanvs;

    [SerializeField]
    public Button btnClick;

    public ActorProfessionEnum profession;

    private EventTrigger scrollRect;

    private bool BtnInteractable = true;

    public void Init(ActorProfessionEnum professionEnum, EventTrigger charactorScrollRect)
    {
        scrollRect = charactorScrollRect;
        profession = professionEnum;
        charImage.sprite = charSprite[((int)professionEnum - 1)];
        lightImage.gameObject.SetActive(false);
        LockCard();

    }

    public void OnDrag(PointerEventData eventData)
    {
        BtnInteractable = false;
        scrollRect.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData != null)
            BtnInteractable = true;
    }


    public void Select()
    {
        lightImage.gameObject.SetActive(true);
    }

    public void UnSelect()
    {
        lightImage.gameObject.SetActive(false);
    }
    public void LockCard()
    {
        lockImage.gameObject.SetActive(true);
        float value = (float)150 / 200;
        charImage.color = new Color(value, value, value);
    }

    public void UnlockCard()
    {
        lockImage.gameObject.SetActive(false);
        charImage.color = new Color(1, 1, 1);
    }

    public void SetCanvasSortOrder(int layerNum)
    {
        thisCanvs.sortingOrder = layerNum;
    }

    public void SetOnClick(Action<ActorProfessionEnum> onClick)
    {
        btnClick.onClick.AddListener(() =>
        {
            if (BtnInteractable)
                onClick?.Invoke(profession);
        });
    }


}
