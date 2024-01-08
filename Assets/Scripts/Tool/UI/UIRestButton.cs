using UnityEngine;
using UnityEngine.UI;

public class UIRestButton : MonoBehaviour
{
    [SerializeField]
    protected Image m_imageCamp;

    public void Init(Sprite target)
    {
        SetImage(m_imageCamp, target);
    }

    private void SetImage(Image source, Sprite target)
    {
        source.sprite = target;
    }
}
