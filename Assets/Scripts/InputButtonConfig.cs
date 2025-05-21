using System;
using UnityEngine;

[Serializable]
public struct InputButtonConfig
{
    [field: SerializeField]
    public ButtonMapping Button { get; private set; }

    [field: SerializeField]
    public ButtonMapping ButtonSprite { get; private set; }

    [field: SerializeField]
    public float DelayTime { get; private set; }
}