using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MAY
{

    public class InstenceTest : MonoBehaviour
    {
        [Header("�ͦ�������")]
        [SerializeField] GameObject UIPartical;
        [SerializeField] KeyCode UIkey =KeyCode.Y;
        [SerializeField] GameObject ScenePartical;
        [SerializeField] KeyCode SceneKey = KeyCode.S;

        [Header("�ͦ���m")]
        [SerializeField] Transform UItarget;
        [SerializeField] Transform Scenetarget;

        void Start()
        {

        }


        void Update()
        {
            //��oUIkey�N�I�sUI
            if (Input.GetKeyDown(UIkey)) {
                Instantiate(UIPartical, UItarget, UItarget);
            }
            if (Input.GetKeyDown(SceneKey)) {
                Instantiate(ScenePartical, Scenetarget, Scenetarget);
            }
        }
    }

}