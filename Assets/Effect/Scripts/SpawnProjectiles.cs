using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAY
{
    /// <summary>
    /// 粒子特效發射器
    /// </summary>


    public class SpawnProjectiles : MonoBehaviour
    {
        [SerializeField] GameObject firePoint;
        [SerializeField] List<GameObject> vfx = new List<GameObject>();
        [SerializeField] RotateTMouse rotateTMouse;
        [SerializeField] float timeToFire=1f;

        GameObject effectToSpawn;

        void Start()
        {
            effectToSpawn = vfx[0];
        }


        void Update()
        {
            if (Input.GetMouseButton(0)&&Time.time>=timeToFire)
            {
                timeToFire = Time.time + 1 / effectToSpawn.GetComponent<BulletMove>().fireRote;
                SpawnVFX();

            }
        }

        /// <summary>
        /// 發射
        /// </summary>
        void SpawnVFX()
        {
            GameObject vfx;
            if (firePoint != null)
            {
                vfx = Instantiate(effectToSpawn, firePoint.transform.position, Quaternion.identity);
                if (rotateTMouse != null)
                {
                    vfx.transform.localRotation = rotateTMouse.GetQuaternion();
                }

            }
            else
            {
                print("沒有發射點");
            }
        }

    }

}
