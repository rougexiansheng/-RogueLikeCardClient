using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// 戰鬥演出
/// </summary>
public class PerformanceMethods
{
    [Inject]
    BattleManager battleManager;
    [Inject]
    DataManager dataManager;
    [Inject]
    EnvironmentManager environmentManager;
    [Inject]
    UIManager uIManager;
    [Inject]
    AssetManager assetManager;
    [Inject]
    DataTableManager dataTableManager;


    [Inject]
    NetworkSaveManager saveManager;

    public async UniTask DoUIAnimation(PUIAnimatonStateData stateData)
    {
        var ui = uIManager.FindUI<UIBattle>();
        await ui.DoUIAnimator(stateData.stateEnum, stateData.autoClose, stateData.sprite);
    }

    private void RemoveLoopParticle(PPassiveData passiveData, EffectPosEnum position)
    {
        ILoopParticleContainer loopParticleContainer = null;
        if (position == EffectPosEnum.ViewCenter || position == EffectPosEnum.SceneCenter || passiveData.isPlayer)
        {
            var uiBattle = uIManager.FindUI<UIBattle>();
            if (uiBattle != null && uiBattle is ILoopParticleContainer)
                loopParticleContainer = uiBattle;
        }
        else
        {
            var mp = environmentManager.monsterPoints[(int)passiveData.monsterPosition];
            if (mp != null && mp is ILoopParticleContainer)
                loopParticleContainer = mp;
        }

        if (loopParticleContainer != null)
        {
            loopParticleContainer.UpdateLoopParticle(passiveData.passiveId, null);
        }
    }

   async public UniTask ShowParticle(PShowParticleData particleData)
    {
        if (particleData.isIgnore) return;
        if (particleData.sound) assetManager.PlayerAudio(AssetManager.AudioMixerVolumeEnum.SoundEffect, particleData.sound);
        if (particleData.particle)
        {
            var obj = GameObject.Instantiate(particleData.particle);
            obj.name = particleData.particle.name;

            var p = obj.GetComponent<ParticleItem>();
            p.ClearAction();

            var isLoopParticle = particleData.isPassive && p.ParticleSystemLength() < 0;
            if (!isLoopParticle)
            {
                p.AddStopAction(() => GameObject.DestroyImmediate(p.gameObject));
            }

            Transform point = null;
            ILoopParticleContainer loopParticleContainer = null;
            if (particleData.position == EffectPosEnum.ViewCenter)
            {
                UtilityHelper.SetChildLayer(obj.transform, LayerMask.NameToLayer("UI"));
                var uiBattle = uIManager.FindUI<UIBattle>();
                if (uiBattle != null)
                {
                    point = uiBattle.UICenterPoint;
                    if (isLoopParticle && uiBattle is ILoopParticleContainer)
                        loopParticleContainer = uiBattle;
                }
            }
            else if (particleData.position == EffectPosEnum.SceneCenter)
            {
                UtilityHelper.SetChildLayer(obj.transform, LayerMask.NameToLayer("Spine"));
                point = environmentManager.centerCanvas.transform;

                var uiBattle = uIManager.FindUI<UIBattle>();
                if (isLoopParticle && uiBattle != null)
                {
                    if (uiBattle is ILoopParticleContainer)
                        loopParticleContainer = uiBattle;
                }
            }
            else
            {
                SpineCharacterCtrl spintCtrl = null;
                if (particleData.isPlayer)
                {
                    UtilityHelper.SetChildLayer(obj.transform, LayerMask.NameToLayer("UI"));
                    var ui = uIManager.FindUI<UIBattle>();
                    if (ui != null)
                    {
                        spintCtrl = ui.spineCharactor;
                        if ((int)particleData.position >= 1 && (int)particleData.position <= 4)
                            point = ui.effectFrontPoint;
                        else if ((int)particleData.position >= 7 && (int)particleData.position <= 10)
                            point = ui.effectBackPoint;

                        if (isLoopParticle && ui is ILoopParticleContainer)
                            loopParticleContainer = ui;
                    }
                }
                else
                {
                    UtilityHelper.SetChildLayer(obj.transform, LayerMask.NameToLayer("Spine"));
                    var mp = environmentManager.monsterPoints[(int)particleData.monsterPosition];
                    if (mp != null)
                    {
                        spintCtrl = mp.spineCharactor;
                        if ((int)particleData.position >= 1 && (int)particleData.position <= 4)
                            point = mp.effectFrontPoint;
                        else if ((int)particleData.position >= 7 && (int)particleData.position <= 10)
                            point = mp.effectBackPoint;

                        if (isLoopParticle && mp is ILoopParticleContainer)
                            loopParticleContainer = mp;
                    }
                }
                Transform followTarget = null;
                switch (particleData.position)
                {
                    case EffectPosEnum.TopFront:
                    case EffectPosEnum.TopBack:
                        followTarget = spintCtrl.topPoint.transform;
                        break;
                    case EffectPosEnum.CenterFront:
                    case EffectPosEnum.CenterBack:
                        followTarget = spintCtrl.centerPoint.transform;
                        break;
                    case EffectPosEnum.BottomBack:
                    case EffectPosEnum.BottomFront:
                        followTarget = spintCtrl.bottomPoint.transform;
                        break;
                    case EffectPosEnum.WeaponBack:
                    case EffectPosEnum.WeaponFront:
                        followTarget = spintCtrl.weapenPoint.transform;
                        break;
                    case EffectPosEnum.BodyBack:
                    case EffectPosEnum.BodyFront:
                        followTarget = spintCtrl.bodyPoint.transform;
                        break;
                    case EffectPosEnum.None:
                    default:
                        Debug.LogError($"特效生在不存在的位置: {particleData.position}");
                        break;
                }
                p.SetFollowTarget(followTarget);
            }

            if (loopParticleContainer != null)
            {
                var updateSuccess = loopParticleContainer.UpdateLoopParticle(particleData.passiveId, obj.gameObject);
                if (!updateSuccess) // 儲存失敗，直接銷毀 Particle
                    p.AddStopAction(() => GameObject.DestroyImmediate(p.gameObject));
            }
            obj.transform.SetParent(point, false);

            await UniTask.DelayFrame(1);

            p.Play();
            if (particleData.isPassive) await UniTask.Delay(175);
            if (particleData.needWaitDestroy) await UniTask.WaitUntil(() => obj == null);
        }
    }

    public UniTask MoveCamera(PCameraMoveData moveData)
    {
        if (moveData.isAll)
            return environmentManager.UltMove();
        else
        {
            if (moveData.monsterPosition == BattleActor.MonsterPositionEnum.None)
            {
                moveData.monsterPosition = battleManager.selectTargetEnum;
            }
            var i = (int)moveData.monsterPosition;
            if (environmentManager.monsterPoints[i].gameObject.activeSelf == false)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (environmentManager.monsterPoints[j].gameObject.activeSelf)
                    {
                        moveData.monsterPosition = (BattleActor.MonsterPositionEnum)j;
                        break;
                    }
                    moveData.monsterPosition = BattleActor.MonsterPositionEnum.Center;
                }
            }
            return UniTask.WhenAll(environmentManager.MoveCameraAngle(moveData.monsterPosition), environmentManager.BackCamera());
        }
    }

    public UniTask MonsterNextSkill(PMonsterNextSkillData nextSkillData)
    {
        return environmentManager.MonsterNextSkill(nextSkillData);
    }
    public UniTask ModifyShield(PModifyShieldData shieldData)
    {
        if (shieldData.isPlayer)
        {
            var ui = uIManager.FindUI<UIBattle>();
            ui.UpdatePlayerShield(shieldData);
        }
        else
        {
            environmentManager.monsterPoints[(int)shieldData.monsterPosition].hpBar.UpdateMonsterShield(shieldData);
        }
        return default;
    }

    public UniTask ModifyColor(PModifyColorData data)
    {
        var ui = uIManager.FindUI<UIBattle>();
        ui.UpdatePlayerColorCost(data);
        return default;
    }

    public UniTask InitSkillItem(PPlayerSkillDataInit initData)
    {
        var ui = uIManager.FindUI<UIBattle>();
        ui.InitSkillItem(initData);
        return default;
    }

    public UniTask InsertSKillItme(PPlayerSkillDataInsert dataInsert)
    {
        var ui = uIManager.FindUI<UIBattle>();
        return ui.InsertSkillItem(dataInsert);
    }

    public UniTask RemoveSKillItme(PPlayerSkillDataRemove dataRemove)
    {
        var ui = uIManager.FindUI<UIBattle>();
        return ui.RemoveSkillItem(dataRemove);
    }

    public UniTask MoveSKillItme(PPlayerSkillDataMove dataMove)
    {
        var ui = uIManager.FindUI<UIBattle>();
        return ui.PushSkillItem(dataMove);
    }

    public UniTask BannedSkillItem(PPlayerSkillDataBanned dataBanned)
    {
        var ui = uIManager.FindUI<UIBattle>();
        if (ui != null)
        {
            return ui.UpdateSkillItemSealState(dataBanned);
        }
        return default;
    }

    public UniTask UpdateSkillItem(PPlayerSkillDataUpdate updateData)
    {
        var ui = uIManager.FindUI<UIBattle>();
        if (ui != null)
        {
            switch (updateData.updateEnum)
            {
                case PPlayerSkillDataUpdate.UpdateTypeEnum.Use:
                case PPlayerSkillDataUpdate.UpdateTypeEnum.Cost:
                    return ui.UpdateSkillItem(updateData);
                default:
                    break;
            }
        }
        return default;
    }

    public UniTask PassiveData(PPassiveData passiveData)
    {
        var define = dataTableManager.GetPassiveDefine(passiveData.passiveId);

        #region 以下 Particle 表演設定 (因為企劃說有可能表演特效但不顯示 ui_icon ，所以跟下面的 icon 判斷分開執行)
        if (passiveData.isRemove)
        {
            RemoveLoopParticle(passiveData, define.effectPos);
        }
        #endregion

        // ui 的icon 顯示
        // 不顯示的被動就跳過
        if (define.isInvisible) return default;
        if (passiveData.isPlayer)
        {
            var ui = uIManager.FindUI<UIBattle>();
            ui?.SetPassive(passiveData);
        }
        else
        {
            environmentManager.PassiveData(passiveData);
        }
        return UniTask.Delay(175);
    }

    public async UniTask OnHeal(POnHealData healData)
    {
        Transform target = null;
        var ui = uIManager.FindUI<UIBattle>();
        if (healData.isPlayer)
        {
            target = ui.hpJumpPoint;
            await ui.UpdatePlayerHp(healData.currentHp, healData.maxHp);
        }
        else
        {
            ui.miniMonsterInfos[(int)healData.monsterPos].SetHp(healData.currentHp / (float)healData.maxHp);
            MonsterPoint mp = environmentManager.monsterPoints[(int)healData.monsterPos];
            mp.hpBar.SetHp(healData.currentHp, healData.maxHp);
            target = ui.monsterScreenPoint[(int)healData.monsterPos].transform;
        }
        var uiJump = assetManager.GetObject<UIJumpHpText>();
        uiJump.transform.SetParent(target, false);
        uiJump.transform.localRotation = Quaternion.identity;
        ((RectTransform)uiJump.transform).anchoredPosition3D = Vector3.zero;
        ((RectTransform)uiJump.transform).localScale = Vector3.one;
        Action jump = async () =>
        {
            await uiJump.Jump(healData.heal, false);
            if (uiJump == null) return;
            assetManager.ReturnObjToPool(uiJump.gameObject);
        };
        jump();
        await UniTask.Delay(175);
    }

    public async UniTask OnDamage(POnDamageData damageData)
    {
        Transform target = null;
        var ui = uIManager.FindUI<UIBattle>();
        environmentManager.ShakeCamera(0.17f).Forget();
        if (!damageData.isPlayer)
        {
            //怪物表演
            MonsterPoint mp = environmentManager.monsterPoints[(int)damageData.monsterPos];
            mp.spineCharactor.PlayAnimationOneShot(SpineAnimationEnum.Hit);
            target = ui.monsterScreenPoint[(int)damageData.monsterPos].transform;
            mp.hpBar.SetHp(damageData.currentHp, damageData.maxHp);
            var define = dataTableManager.GetMonsterDefine(damageData.monsterId);
            assetManager.PlayerAudio(AssetManager.AudioMixerVolumeEnum.SoundEffect, define.hitSound);
            ui.miniMonsterInfos[(int)damageData.monsterPos].SetHp(damageData.currentHp / (float)damageData.maxHp);
        }
        else
        {
            // 表演受擊
            ui.spineCharactor.PlayAnimationOneShot(SpineAnimationEnum.Hit, SpineAnimationEnum.None, true);
            target = ui.hpJumpPoint;
            ui.Hit();
            // 音效
            var define = dataTableManager.GetProfessionDataDefine(saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().SelectProfession);
            assetManager.PlayerAudio(AssetManager.AudioMixerVolumeEnum.SoundEffect, define.hitSound);
        }
        // 跳血
        var uiJump = assetManager.GetObject<UIJumpHpText>();
        uiJump.transform.SetParent(target, false);
        uiJump.transform.localRotation = Quaternion.identity;
        ((RectTransform)uiJump.transform).anchoredPosition3D = Vector3.zero;
        ((RectTransform)uiJump.transform).localScale = Vector3.one;
        Action jump = async () =>
        {
            if (damageData.isBlock)
                await uiJump.JumpBlock();
            else
                await uiJump.Jump(damageData.dmg, true);
            if (uiJump == null) return;
            assetManager.ReturnObjToPool(uiJump.gameObject);
        };
        jump();
        // 美術已經調整完
        await UniTask.Delay(175);
        // 更新血量 如果 低於50%表演爆衣
        if (damageData.isPlayer)
        {
            await ui.UpdatePlayerHp(damageData.currentHp, damageData.maxHp);
        }
    }

    public async UniTask MonsterAppear(PMonsterAppearData showData)
    {
        var ui = uIManager.FindUI<UIBattle>();
        for (int i = 0; i < ui.miniMonsterInfos.Count; i++)
        {
            ui.miniMonsterInfos[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < showData.positions.Count; i++)
        {
            ui.miniMonsterInfos[(int)showData.positions[i]].SetDeadMark(false);
            ui.miniMonsterInfos[(int)showData.positions[i]].gameObject.SetActive(true);
            ui.miniMonsterInfos[(int)showData.positions[i]].SetHp(1);
        }
        if (showData.isBoss)
        {
            // 相機抬高
            await environmentManager.UltMove();
            // Boss顯示
            await environmentManager.monsterPoints[(int)BattleActor.MonsterPositionEnum.Center].MonsterShow();
            // 顯示其他怪物
            var ls = new List<UniTask>();
            for (int i = 0; i < showData.positions.Count; i++)
            {
                var pos = showData.positions[i];
                if (pos != BattleActor.MonsterPositionEnum.Center)
                {
                    ls.Add(environmentManager.monsterPoints[(int)pos].MonsterShow());
                }
            }
            await UniTask.WhenAll(ls);
            ls.Clear();
            // 表演Boss登場
            var monster = battleManager.monsters.Find(m => m.monsterPos == BattleActor.MonsterPositionEnum.Center);
            var define = dataTableManager.GetMonsterDefine(monster.monsterId);
            var mp = environmentManager.monsterPoints[(int)BattleActor.MonsterPositionEnum.Center];
            assetManager.PlayerAudio(AssetManager.AudioMixerVolumeEnum.SoundEffect, define.showSound);
            if (mp.spineCharactor.HaveAnimaton(SpineAnimationEnum.Debut)) ls.Add(mp.spineCharactor.PlayAnimation(SpineAnimationEnum.Debut));
            for (int j = 0; j < define.appearEnums.Count; j++)
            {
                switch (define.appearEnums[j])
                {
                    case MonsterAppearEnum.ShakeCamera:
                        ls.Add(environmentManager.ShakeCamera());
                        break;
                    case MonsterAppearEnum.SpeedLine:
                        ls.Add(ui.SpeedLine());
                        break;
                    default:
                        break;
                }
            }
            await UniTask.WhenAll(ls);
            //相機回中間
            await environmentManager.BackCamera();
        }
        else
        {
            //非Boss每隻怪物輪流播
            for (int i = 0; i < showData.positions.Count; i++)
            {
                var pos = showData.positions[i];
                var mp = environmentManager.monsterPoints[(int)pos];
                await environmentManager.MoveCameraAngle(pos);
                await mp.MonsterShow();
            }
            if (showData.positions.Exists(p => p == BattleActor.MonsterPositionEnum.Center)) await environmentManager.MoveCameraAngle(BattleActor.MonsterPositionEnum.Center);
        }
    }

    public async UniTask OnAtack(POnAttackData attackData)
    {
        var skillDefine = dataTableManager.GetSkillDefine(attackData.skillId);
        if (attackData.isPlayer)
        {
            var ui = uIManager.FindUI<UIBattle>();
            var professionDefine = dataTableManager.GetProfessionDataDefine(saveManager.GetContainer<NetworkSaveBattleDungeonContainer>().SelectProfession);

            if (professionDefine.ultGroupIds.Contains(attackData.skillId))
            {
                await ui.uIUltimate.Show(ui.spineCharactor.currentSkin);
            }
            else
            {
                assetManager.PlayerAudio(AssetManager.AudioMixerVolumeEnum.SoundEffect, professionDefine.attackSound);
                ui.spineCharactor.PlayAnimationOneShot(skillDefine.animationEnum, SpineAnimationEnum.Attack01);
                await ui.spineCharactor.AttackTrigger();
            }
        }
        else
        {
            await environmentManager.MoveCameraAngle(attackData.monsterPosition);
            var mp = environmentManager.monsterPoints[(int)attackData.monsterPosition];
            await mp.hpBar.OnAttack();
            var ctrl = mp.spineCharactor;
            ctrl.PlayAnimationOneShot(skillDefine.animationEnum, SpineAnimationEnum.Attack01);
            var monsterDefine = dataTableManager.GetMonsterDefine(attackData.monsterId);
            assetManager.PlayerAudio(AssetManager.AudioMixerVolumeEnum.SoundEffect, monsterDefine.attackSound);
            await ctrl.AttackTrigger();
        }
    }

    public async UniTask MonsterRemove(PMonsterRemoveData removeData)
    {
        var ui = uIManager.FindUI<UIBattle>();
        for (int i = 0; i < removeData.positions.Count; i++)
        {
            ui.miniMonsterInfos[(int)removeData.positions[i]].SetDeadMark(true);
        }
        await environmentManager.MonsterRemove(removeData);
    }

    public async UniTask PSummonMonsterData(PSummonMonsterData addData)
    {
        var ls = new List<UniTask>();
        var ui = uIManager.FindUI<UIBattle>();
        for (int i = 0; i < addData.positions.Count; i++)
        {
            ui.miniMonsterInfos[(int)addData.positions[i]].SetDeadMark(false);
            ui.miniMonsterInfos[(int)addData.positions[i]].gameObject.SetActive(true);
            ui.miniMonsterInfos[(int)addData.positions[i]].SetHp(1);
        }
        for (int i = 0; i < addData.positions.Count; i++)
        {
            ls.Clear();
            var monster = battleManager.monsters.Find(m => m.monsterPos == addData.positions[i]);
            var define = dataTableManager.GetMonsterDefine(monster.monsterId);
            var mp = environmentManager.monsterPoints[(int)addData.positions[i]];
            await mp.MonsterShow();
            if (mp.spineCharactor.HaveAnimaton(SpineAnimationEnum.Debut)) ls.Add(mp.spineCharactor.PlayAnimation(SpineAnimationEnum.Debut));
            for (int j = 0; j < define.appearEnums.Count; j++)
            {
                switch (define.appearEnums[j])
                {
                    case MonsterAppearEnum.ShakeCamera:
                        ls.Add(environmentManager.ShakeCamera());
                        break;
                    case MonsterAppearEnum.SpeedLine:
                        ls.Add(ui.SpeedLine());
                        break;
                    default:
                        break;
                }
            }

        }
        await UniTask.WhenAll(ls);
    }
}
