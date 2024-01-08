using Cysharp.Threading.Tasks;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIDressBreak : MonoBehaviour
{
    AssetManager assetManager;
    AudioClip audioClip;
    [SerializeField]
    Image flashImg, BgImg;
    [SerializeField]
    Transform spinePoint;
    [SerializeField]
    SpineCharacterCtrl characterCtrl;
    [Header("表演位置參數")]
    [SerializeField]
    Vector3 offset;
    [SerializeField]
    float start, A, B, C, at, bt, ct, scale, delayTime, playSpeed;
    [SerializeField]
    float bgStartA, bgStartt, bgEndA, bgEndt;
    [SerializeField]
    float spinInT, spinOutT;
    [SerializeField]
    bool playOnShow;
    [SerializeField]
    Color flashColor;
    [SerializeField]
    AnimationCurve curve;
    public void Init(GameObject spineObj,AudioClip audioClip,AssetManager assetManager)
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
    [InspectorButton]
    public async UniTask Show()
    {
        characterCtrl.SetSkin(SpineCharacterCtrl.SpineSkinEnum.Origin);

        var t = PlayAnimation();

        ((RectTransform)spinePoint.transform).anchoredPosition3D = offset;
        spinePoint.transform.localScale = Vector3.one * scale;

        gameObject.SetActive(true);
        BgImg.color = Color.clear;

        flashImg.color = new Color(flashColor.r, flashColor.g, flashColor.b, start);

        await BgImg.DOFade(bgStartA, bgStartt).AsyncWaitForCompletion().AsUniTask();


        // 閃光
        await flashImg.DOFade(A, at).AsyncWaitForCompletion().AsUniTask();
        await flashImg.DOFade(B, bt).AsyncWaitForCompletion().AsUniTask();
        await flashImg.DOFade(C, ct).AsyncWaitForCompletion().AsUniTask();



        await t;
        // 淡出關閉
        await BgImg.DOFade(bgEndA, bgEndt).AsyncWaitForCompletion().AsUniTask();
        gameObject.SetActive(false);
    }

    async UniTask PlayAnimation()
    {
        if (playOnShow) spinePoint.gameObject.SetActive(false);
        if (!playOnShow)
        {
            spinePoint.gameObject.SetActive(true);
            await characterCtrl.spine.DOFade(1, spinInT).AsyncWaitForCompletion().AsUniTask();
        }
        await UniTask.Delay((int)(delayTime * 1000));
        // 表演
        if (playOnShow)
        {
            spinePoint.gameObject.SetActive(true);
            await characterCtrl.spine.DOFade(1, spinInT).AsyncWaitForCompletion().AsUniTask();
        }
        var state = characterCtrl.spine.AnimationState.GetCurrent(0);
        var f = characterCtrl.spine.AnimationState.GetCurrent(0).AnimationEnd;
        DOTween.To(() => characterCtrl.spine.timeScale, x => characterCtrl.spine.timeScale = x, playSpeed, f).SetEase(curve);
        assetManager.PlayerAudio(AssetManager.AudioMixerVolumeEnum.Speak, audioClip);
        await characterCtrl.PlayAnimation(SpineAnimationEnum.DressBreak);
        await characterCtrl.spine.DOFade(0, spinOutT).AsyncWaitForCompletion().AsUniTask();
        if (playOnShow) spinePoint.gameObject.SetActive(false);
    }
}
