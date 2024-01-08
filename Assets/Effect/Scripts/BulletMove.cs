using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAY
{

    public class BulletMove : MonoBehaviour
    {
        /// <summary>
        /// 子彈動態
        /// </summary>
        [SerializeField] float speed;
        public float fireRote;
        [SerializeField] GameObject hitPrefab;//放入受擊的預置物


        void Update()
        {
            if (speed != 0)
            {
                transform.position += transform.forward * (speed * Time.deltaTime);
            }
            else
            {
                print("Nospeed");
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            speed = 0;
          


            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point;

            //觸碰到物件
            if (hitPrefab != null)
            {
                var hitVFX = Instantiate(hitPrefab, pos, rot);
            }

            //刪除物件
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {


        }
    }


}