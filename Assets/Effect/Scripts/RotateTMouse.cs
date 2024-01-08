using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAY
{
    public class RotateTMouse : MonoBehaviour
    {
        /// <summary>
        /// 子彈跟隨滑鼠發射
        /// </summary>

        [SerializeField] Camera cam;
        [SerializeField] float maximumLenght;

        Ray rayMouse;
        Vector3 pos;
        Vector3 direction;
        Quaternion rotation;

        void Update()
        {
            if (cam != null)
            {
                RaycastHit hit;
                var mousePos = Input.mousePosition;
                rayMouse = cam.ScreenPointToRay(mousePos);

                if (Physics.Raycast(rayMouse.origin, rayMouse.direction, out hit, maximumLenght))
                {
                    RotateToMouseDirection(gameObject, hit.point);
                }
                else {
                    var pos = rayMouse.GetPoint(maximumLenght);
                    RotateToMouseDirection(gameObject, pos);
                }
            }
            else
            {
                print("沒放攝影機");


            }
        }

        void RotateToMouseDirection(GameObject obj, Vector3 destination)
        {
            direction = destination - obj.transform.position;
            rotation = Quaternion.LookRotation(direction);

            obj.transform.localRotation = Quaternion.Lerp(obj.transform.rotation, rotation, 1);
        }

        public Quaternion GetQuaternion()
        {
            return rotation;

        }


    }




}