using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIUltimate : MonoBehaviour
{
    AssetManager assetManager;
    AudioClip audioClip;
    [SerializeField]
    Transform spinePoint;
    [SerializeField]
    SpineCharacterCtrl characterCtrl;
    [SerializeField]
    Image flashImg;
    [SerializeField]
    SpriteAnimation spriteAnimation;
    [SerializeField]
    Vector3 startV3, endV3;
    [SerializeField]
    float moveT,colorT;
    
    public void Init(GameObject spineObj, AudioClip audioClip, AssetManager assetManager)
    {
        if (characterCtrl != null)
        {
            Destroy(characterCtrl.gameObject);
            characterCtrl = null;
        }
        this.assetManager = assetManager;
        this.audioClip = audioClip;
        var obj = GameObject.Instantiate(spineObj, spinePoint);
        characterCtrl = obj.GetComponent<SpineCharacterCtrl>();
        characterCtrl.SetSkin(SpineCharacterCtrl.SpineSkinEnum.Origin);
        characterCtrl.transform.localRotation = Quaternion.identity;
        ((RectTransform)characterCtrl.transform).anchoredPosition3D = Vector3.zero;
        ((RectTransform)characterCtrl.transform).localScale = Vector3.one;
    }

    public async UniTask Show(SpineCharacterCtrl.SpineSkinEnum spineSkin)
    {
        spriteAnimation.gameObject.SetActive(false);
        characterCtrl.SetSkin(spineSkin);
        gameObject.SetActive(true);

        spriteAnimation.Play();
        flashImg.DOFade(1, 0.2f).SetLoops(2,LoopType.Yoyo);
        await PlayAnimate();

        gameObject.SetActive(false);
        spriteAnimation.Stop();
    }

    async UniTask PlayAnimate()
    {
        characterCtrl.spine.color = Color.clear;
        characterCtrl.spine.DOColor(Color.white, colorT);
        await ((RectTransform)spinePoint).DOAnchorPos3D(endV3, moveT).AsyncWaitForCompletion().AsUniTask();
        assetManager.PlayerAudio(AssetManager.AudioMixerVolumeEnum.Speak, audioClip);
        await characterCtrl.PlayAnimation(SpineAnimationEnum.Attack03);
        characterCtrl.spine.DOColor(Color.clear, colorT);
        await ((RectTransform)spinePoint).DOAnchorPos3D(startV3, moveT).AsyncWaitForCompletion().AsUniTask();
    }
}
