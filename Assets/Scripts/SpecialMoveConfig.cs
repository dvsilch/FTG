using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpecialMoveConfig
{
    [field: SerializeField]
    public string Name { get; private set; }

    [field: SerializeField]
    public float ResetTime { get; private set; } = 0.5f;

    [field: SerializeField]
    public int Priority { get; private set; }

    /// <summary>
    /// �����ڻ�������ͣ����ʱ��
    /// </summary>
    [field: SerializeField]
    public int BufferTimeMs { get; private set; } = 200;

    [field: SerializeField]
    public List<InputButtonConfig> InputButtons { get; private set; } = new List<InputButtonConfig>();
}
