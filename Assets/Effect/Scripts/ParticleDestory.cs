using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MAY
{
    /// <summary>
    /// 粒子刪除時間
    /// </summary>
    public class ParticleDestory : MonoBehaviour
    {
        [SerializeField] float destoryTime = 0.5f;

        void Awake()
        {
            Destroy(gameObject, destoryTime);
        }
    }

}
