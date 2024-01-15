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
        ((RectTransform)transform).anchoredPosition3D += new Vector3(0, 300, 0);
        transform.DOLocalMove(new Vector3(1, -120, 0), 0.5f);
        await UniTask.Delay(2000);
    }


}
