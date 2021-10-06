using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public int index {private set; get; }
    private SpriteRenderer _symbolImage;
    public int symbolId { private set; get; }
    private void Awake()
    {
        _symbolImage = GetComponent<SpriteRenderer>();
    }

    public void UpdateIndex(int value)
    {
        index = value;
    }
}
