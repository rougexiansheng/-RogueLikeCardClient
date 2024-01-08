using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAY {
    public class VFXTestPlay : MonoBehaviour
    {
        ParticleSystem particles;

        void Start()
        {
            particles = this.gameObject.GetComponent<ParticleSystem>();
        }

       
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                particles.Play();
            }
        }
   

    }

}