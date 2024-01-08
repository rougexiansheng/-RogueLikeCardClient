using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MAY
{

    public class InstenceTest : MonoBehaviour
    {
        [Header("生成的物件")]
        [SerializeField] GameObject UIPartical;
        [SerializeField] KeyCode UIkey =KeyCode.Y;
        [SerializeField] GameObject ScenePartical;
        [SerializeField] KeyCode SceneKey = KeyCode.S;

        [Header("生成位置")]
        [SerializeField] Transform UItarget;
        [SerializeField] Transform Scenetarget;

        void Start()
        {

        }


        void Update()
        {
            //獲得UIkey就呼叫UI
            if (Input.GetKeyDown(UIkey)) {
                Instantiate(UIPartical, UItarget, UItarget);
            }
            if (Input.GetKeyDown(SceneKey)) {
                Instantiate(ScenePartical, Scenetarget, Scenetarget);
            }
        }
    }

}