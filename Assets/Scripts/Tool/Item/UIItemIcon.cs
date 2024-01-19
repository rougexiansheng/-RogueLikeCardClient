using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIItemIcon : MonoBehaviour
{
    [SerializeField]
    private Image icon;

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }

    public async UniTask DropAnimation()
    {
        Vector3 originalPosition = transform.position;
        ((RectTransform)transform).anchoredPosition3D += new Vector3(0, 200, 0);
        transform.DOLocalMoveY(-Vector3.Distance(originalPosition, transform.position), 0.15f)
           .SetEase(Ease.InCubic)
           .OnComplete(() =>
           {
               // 弹跳效果
               transform.DOJump(transform.position, 1, 1, 0.3f)
                   .SetEase(Ease.OutQuad);
           });

        await UniTask.Delay(2000);
    }


}
