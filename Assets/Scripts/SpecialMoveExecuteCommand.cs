using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Dvsilch;

public class SpecialMoveExecuteCommand
{
    public SpecialMoveItem SpecialMoveItem { get; private set; }

    public float RemainingTime { get; private set; }

    public bool IsValid { get; private set; } = true;

    public SpecialMoveExecuteCommand(SpecialMoveItem specialMoveItem)
    {
        SpecialMoveItem = specialMoveItem;
        RemainingTime = specialMoveItem.SpecialMoveConfig.ResetTime;
        SpecialMoveItem.IsTriggering = true;
    }

    public void Discard()
    {
        IsValid = false;
        SpecialMoveItem.IsTriggering = false;
        SpecialMoveItem.ResetButton();
    }

    public async UniTaskVoid Start(PlayerLoopTiming timing, CancellationToken ct)
    {
        try
        {
            await UniTask.Delay(SpecialMoveItem.SpecialMoveConfig.BufferTimeMs, delayTiming: timing, cancellationToken: ct);
        }
        finally
        {
            if (RemainingTime == SpecialMoveItem.SpecialMoveConfig.ResetTime && IsValid) // ˵����ûִ�У���Ϊ�Ƿ�
            {
                Debug.Log($"{SpecialMoveItem.SpecialMoveConfig.Name}���峬ʱ������");
                Discard();
            }
        }
    }

    public async UniTask Execute(PlayerLoopTiming timing, CancellationToken ct)
    {
        Debug.Log($"{SpecialMoveItem.SpecialMoveConfig.Name} ����");
        while (RemainingTime > 0f)
        {
            RemainingTime -= Time.deltaTime;
            await UniTask.NextFrame(timing, ct);
        }
        SpecialMoveItem.IsTriggering = false;
        SpecialMoveItem.ResetButton();
    }
}