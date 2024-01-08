using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteAnimation : MonoBehaviour
{
    [SerializeField]
    Image image;
    public Sprite[] sprites;
    public bool isLoop;
    public float time;
    bool isPlaying = false;

    [InspectorButton]
    public async void Play()
    {
        if (isPlaying) return;
        isPlaying = true;
        do
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (!isPlaying) return;
                image.sprite = sprites[i];
                await UniTask.Delay((int)(time*1000));
            }
        }
        while (isLoop);
        isPlaying = false;
    }

    [InspectorButton]
    public void Stop()
    {
        isPlaying = false;
    }
}
