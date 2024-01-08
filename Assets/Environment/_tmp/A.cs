using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A : MonoBehaviour
{
    [SerializeField] Animator ani;

    void Start()
    {
        
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) {
            ani.SetBool("A",true);
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            ani.SetBool("A", false);
            if (gameObject.transform.position != Vector3.zero)
            {
                ani.SetBool("A", true);
            }
        }
    }
}
