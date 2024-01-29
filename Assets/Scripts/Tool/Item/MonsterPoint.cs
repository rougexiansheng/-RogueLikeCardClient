using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPoint : MonoBehaviour, ILoopParticleContainer
{
    public UIMonsterHpBar hpBar;
    public UIMonsterHpBar bossHpBar;
    public UIMonsterHpBar currenthpBar;
    public int monsterIndex;
    public SpineCharacterCtrl spineCharactor;
    public Transform effectFrontPoint, effectBackPoint;
    public RectTransform spinePoint;
    public Canvas canvas;
    /// <summary> 儲存掛在身上的 Loop Particle 特效。 key: passiveId,  value:ParticleObj </summary>
    private Dictionary<int, GameObject> loopParticleObj = new Dictionary<int, GameObject>();

    public void SetHpBar(bool isBoss)
    {
        currenthpBar = isBoss ? bossHpBar : hpBar;
        currenthpBar.Active(true);
    }

    public void Active(bool torf)
    {
        gameObject.SetActive(torf);
        currenthpBar.Active(torf);
    }

    public async UniTask MonsterRemove()
    {
        spineCharactor.spine.color = Color.white;
        currenthpBar.Active(false);
        await spineCharactor.spine.DOFade(0, 0.5f).AsyncWaitForCompletion().AsUniTask();

    }

    public async UniTask MonsterShow()
    {
        Active(true);
        spineCharactor.spine.color = new Color(1, 1, 1, 0);
        await spineCharactor.spine.DOFade(1, 0.5f).AsyncWaitForCompletion().AsUniTask();
    }

    public bool UpdateLoopParticle(int passiveId, GameObject particle)
    {
        var updateSuccess = false;
        if (passiveId > 0)
        {
            if (particle != null && !loopParticleObj.ContainsKey(passiveId))
            {
                loopParticleObj.Add(passiveId, particle);
                updateSuccess = true;
            }
            else if (particle == null && loopParticleObj.ContainsKey(passiveId))
            {
                var removeParticle = loopParticleObj[passiveId];
                removeParticle.SetActive(false);
                GameObject.DestroyImmediate(removeParticle);
                loopParticleObj.Remove(passiveId);
                updateSuccess = true;
            }
        }

        return updateSuccess;
    }

    public void ResetLoopParticle()
    {
        foreach (var particle in loopParticleObj.Values)
        {
            if (particle == null) continue;
            particle.gameObject.SetActive(false);
            GameObject.DestroyImmediate(particle.gameObject);
        }
        loopParticleObj.Clear();
    }

    public void Clear()
    {
        if (spineCharactor != null) Destroy(spineCharactor.gameObject);
        monsterIndex = -1;
        if (bossHpBar != null) bossHpBar.Clear();
        hpBar.Clear();
        ResetLoopParticle();
        Active(false);
    }


}
