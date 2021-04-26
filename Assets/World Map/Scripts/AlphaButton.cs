using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaButton : MonoBehaviour
{
    [Tooltip("Remember to set the Texture to Read/Write Enabled. Alpha values less than the threshold will not be detected")]
    [Range(0, 1)]
    public float alphaThreshhold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshhold;
    }
}
