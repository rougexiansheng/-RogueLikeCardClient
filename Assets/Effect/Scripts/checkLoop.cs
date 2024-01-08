using Coffee.UIExtensions;

using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;


namespace MAY
{
    /// <summary>
    ///人物特效的檢查
    /// </summary>

    [RequireComponent(typeof(UIParticle),typeof(ParticleItem))]
    public class CheckLoop : MonoBehaviour
    {

        GameObject target;
        void Awake()
        {
            target = this.gameObject;

            //如果有這個物件，且不存在任何error則報告以下錯誤
            if (this.GetComponent<CheckLoop>() && hasError == false)
            {
                Debug.Log("<color=#DDA0DD><size=16><i>" + gameObject.name + "的checkLoop腳本未移除" + "</i></size></color>");
            }
            else
            {
                Debug.Log("<color=##02C874><size=16><i>物件存在transform未更改</i></size></color>");
            }


        }

        [InspectorButton("Reset")]
        void Reset()
        {
            ResetParticles(gameObject);
            print(gameObject.name + "時間:" + ParticleSystemLength());
        }

        bool hasError = false;

        //重置粒子參數

        void ResetParticles(GameObject obj)
        {
            ParticleSystem ps = obj.GetComponent<ParticleSystem>();
            ParticleSystemRenderer psr = obj.GetComponent<ParticleSystemRenderer>();
            if (ps != null)
            {
                //重置粒子參數
                ps.loop = false;
                ps.playOnAwake = false;
                ps.scalingMode = ParticleSystemScalingMode.Hierarchy;
                ps.simulationSpace = ParticleSystemSimulationSpace.Local;
              
                psr.maxParticleSize = 1000;
                psr.sortingOrder = 0;
                

                var main = ps.main;
                main.duration = ParticleSystemLength();


                //檢查是否添加RecTransform
                if (!obj.GetComponent<RectTransform>())
                {
                    Debug.LogError(this.name + "transform未更改");
                    hasError = true;
                }


            }

            foreach (Transform child in obj.transform)
            {
                ResetParticles(child.gameObject);
            }

        }


      

        float ParticleSystemLength()
        {
            ParticleSystem[] particleSystems = this.transform.GetComponentsInChildren<ParticleSystem>();
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


    }


}