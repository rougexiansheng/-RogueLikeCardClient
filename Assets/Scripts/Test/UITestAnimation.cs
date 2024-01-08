using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;

public class UITestAnimation : MonoBehaviour
{
    [Header("Shake")]
    [SerializeField]
    List<Camera> cameras;
    [SerializeField]
    float shakeTime;
    [SerializeField]
    Vector3 shakeStrV3;
    [SerializeField]
    int shakeCount;
    [Header("Scale")]
    [SerializeField]
    GameObject bossPos;
    [SerializeField]
    float scaleSize,scaleTime;
    [SerializeField]
    int scaleCount;
    [SerializeField]
    Ease scaleEase;
    [Header("Fade")]
    [SerializeField]
    List<GameObject> ways;
    [SerializeField]
    float fadeTime;
    [InspectorButton]
    void Shake()
    {
        // time:2 v3 0.2/0.5/0 100
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].transform.DOShakePosition(shakeTime, shakeStrV3, shakeCount);
        }
    }
    [InspectorButton]
    void FadeOut()
    {
        for (int i = 0; i < ways.Count; i++)
        {
            var skelet = ways[i].GetComponentInChildren<SkeletonAnimation>();
            if (skelet)
            {
                DOVirtual.Float(1, 0, fadeTime, f => 
                {
                    skelet.skeleton.A = f;
                }).OnComplete(() => 
                {
                    skelet.skeleton.A = 1;
                });
            }
        }
    }
    [InspectorButton]
    void Scale()
    {
        bossPos.transform.DOScale(scaleSize, scaleTime / scaleCount).SetLoops(scaleCount, LoopType.Yoyo).SetEase(scaleEase);
    }
}
