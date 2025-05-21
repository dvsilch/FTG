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

        // ����ѭ��
        // 1 ÿ����ʽ������һ���ȴ�������ɵ�task��task��ɺ�Ὣ����ȴ������ȼ����У���ô���Ƿ�ֹͬһ֡�ڲ��������ʽ
        // 2 �����ȼ�����������ʱ��ȡ�����ȼ���ߵ�һ���������ִ�ж��У���������ȼ�����
        // 3 ����ִ�ж���������ʱ��ȡ��һ���Ϸ�����ʽִ�У�ִ����Ϻ�ص�1
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

                // ���� ��Ӧ��������1
                item.StartDetecting(PriorityQueue, PlayerLoopTiming, destroyCancellationToken).Forget();
                SpecialMoves.Add(item);
            }

            StartExecuting().Forget();
        }

        private void OnDestroy()
        {
            DestroyImmediate(InputActionAsset);
        }

        // ����
        private async UniTaskVoid StartExecuting()
        {
            while (true)
            {
                // ��Ӧ����2
                await UniTask.WaitUntil(() => PriorityQueue.Count > 0, PlayerLoopTiming, destroyCancellationToken);
                SpecialMoveExecutionQueue.Enqueue(PriorityQueue.Dequeue());
                while (PriorityQueue.TryDequeue(out var discarded, out _))
                {
                    Debug.Log($"{discarded.SpecialMoveItem.SpecialMoveConfig.Name} ���ȼ�����, ����");
                }

                // ���� ��Ӧ����3
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