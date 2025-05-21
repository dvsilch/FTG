using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dvsilch
{
    public class SpecialMoveList : MonoBehaviour
    {
        [field: SerializeField]
        public InputActionAsset InputActionAsset { get; private set; }

        [field: SerializeField]
        public RectTransform SpecialMoveItemParent { get; private set; }

        [field: SerializeField]
        public SpecialMoveItem SpecialMoveItemTemplatePrefab { get; private set; }

        [field: SerializeField]
        public List<SpecialMoveItem> SpecialMoves { get; private set; } = new();

        [field: SerializeField]
        public CharacterSpecialMovesSO CharacterSpecialMovesSO { get; private set; }

        [field: SerializeField]
        public PlayerLoopTiming PlayerLoopTiming { get; private set; } = PlayerLoopTiming.LastUpdate;

        public List<SpecialMoveConfig> SpecialMoveConfigs => CharacterSpecialMovesSO.SpecialMoveConfigs;

        public Queue<SpecialMoveExecuteCommand> SpecialMoveExecutionQueue { get; private set; }

        public PriorityQueue<SpecialMoveExecuteCommand, int> PriorityQueue { get; private set; } = new();

        public SpecialMoveExecuteCommand CurrentSpecialMoveExecuteCommand { get; private set; }

        // 流程循环
        // 1 每个招式自身有一个等待输入完成的task，task完成后会将结果先存入优先级队列，这么做是防止同一帧内产生多个招式
        // 2 当优先级队列有数据时，取出优先级最高的一个，放入待执行队列，并清空优先级队列
        // 3 当待执行队列有数据时，取出一个合法的招式执行，执行完毕后回到1
        private void Start()
        {
            InputActionAsset = Instantiate(InputActionAsset);
            InputActionAsset.Enable();

            SpecialMoveExecutionQueue ??= new();

            SpecialMoves?.Clear();
            SpecialMoves ??= new(SpecialMoveConfigs.Count);
            SpecialMoves.Capacity = SpecialMoveConfigs.Count;

            foreach (var config in SpecialMoveConfigs)
            {
                var item = Instantiate(SpecialMoveItemTemplatePrefab, SpecialMoveItemParent);
                item.Init(config, InputActionAsset);

                // 生产 对应上述流程1
                item.StartDetecting(PriorityQueue, PlayerLoopTiming, destroyCancellationToken).Forget();
                SpecialMoves.Add(item);
            }

            StartExecuting().Forget();
        }

        private void OnDestroy()
        {
            DestroyImmediate(InputActionAsset);
        }

        // 消费
        private async UniTaskVoid StartExecuting()
        {
            while (true)
            {
                // 对应流程2
                await UniTask.WaitUntil(() => PriorityQueue.Count > 0, PlayerLoopTiming, destroyCancellationToken);
                SpecialMoveExecutionQueue.Enqueue(PriorityQueue.Dequeue());
                while (PriorityQueue.TryDequeue(out var discarded, out _))
                {
                    Debug.Log($"{discarded.SpecialMoveItem.SpecialMoveConfig.Name} 优先级不够, 丢弃");
                }

                // 消费 对应流程3
                SpecialMoveExecuteCommand cmd = null;
                while (SpecialMoveExecutionQueue.TryDequeue(out cmd))
                {
                    if (cmd.IsValid)
                        break;
                }
                if (cmd != null)
                {
                    CurrentSpecialMoveExecuteCommand = cmd;
                    await cmd.Execute(PlayerLoopTiming, destroyCancellationToken);
                    CurrentSpecialMoveExecuteCommand = null;
                }
            }
        }
    }
}