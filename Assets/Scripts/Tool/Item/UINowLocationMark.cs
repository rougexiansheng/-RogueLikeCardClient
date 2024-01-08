using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UINowLocationMark : MonoBehaviour
{

    public void MoveTo(Vector3 location)
    {
        transform.DOKill();
        transform.DOMove(location, 0, true);
    }


}
