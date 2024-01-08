using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleItem : MonoBehaviour
{

    ParticleSystem particle;
    Action callBack = () => { };
    Transform target;
    ParticleSystem[] particleSystems;
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (particle == null)
        {
            particle = GetComponent<ParticleSystem>();
            var m = particle.main;
            m.stopAction = ParticleSystemStopAction.Callback;
        }
    }

    public void SetFollowTarget(Transform transform)
    {
        target = transform;
    }
    [InspectorButton]
    public void Stop()
    {
        Init();
        particle.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    [InspectorButton]
    public void Play()
    {
        Init();
        particle.Stop();
        particle.Play();
        var f = ParticleSystemLength();
        if (f < 0) End();
        else Invoke("End", f);
    }

    public void AddStopAction(Action action)
    {
        Init();
        callBack += action;
    }

    public void ClearAction()
    {
        callBack = null;
        callBack = () => { };
        Stop();
    }

    void End()
    {
        callBack?.Invoke();
    }

    /// <summary>
    /// 取得 Particle 播放時長(回傳 -1 為 loop)
    /// </summary>
    /// <returns></returns>
    public float ParticleSystemLength()
    {
        if(particleSystems == null)
            particleSystems = transform.GetComponentsInChildren<ParticleSystem>();
        float maxDuration = 0;
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps.emission.enabled)
            {
                if (ps.main.loop)
                {
                    return -1f;
                }
                float dunration = 0f;
                if (ps.emission.rateOverTime.constantMin <= 0)
                {
                    dunration = ps.main.startDelay.constantMax + ps.main.startLifetime.constantMax;
                }
                else
                {
                    dunration = ps.main.startDelay.constantMax + Mathf.Max(ps.main.duration, ps.main.startLifetime.constantMax);
                }

                if (dunration > maxDuration)
                {
                    maxDuration = dunration;
                }
            }
        }
        return maxDuration;
    }


    private void Update()
    {
        if (target == null) return;
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
