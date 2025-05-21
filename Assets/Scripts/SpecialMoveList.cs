using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dvsilch
{
    public class SpecialMoveList : MonoBehaviour
    {
        [field: SerializeField]
        public PlayerInput PlayerInput { get; private set; }

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

        public const string ACTION_PUNCH = "Punch";

        public const string ACTION_KICK = "Kick";

        public const string ACTION_MOVE = "Move";

        public List<SpecialMoveConfig> SpecialMoveConfigs => CharacterSpecialMovesSO.SpecialMoveConfigs;

        public Queue<SpecialMoveExecuteCommand> SpecialMoveExecutionQueue { get; private set; }

        public PriorityQueue<SpecialMoveExecuteCommand, int> PriorityQueue { get; private set; } = new();

        // ����ѭ��
        // 1 ÿ����ʽ������һ���ȴ�������ɵ�task��task��ɺ�Ὣ����ȴ������ȼ����У���ô���Ƿ�ֹͬһ֡�ڲ��������ʽ
        // 2 �����ȼ�����������ʱ��ȡ�����ȼ���ߵ�һ���������ִ�ж��У���������ȼ�����
        // 3 ����ִ�ж���������ʱ��ȡ��һ���Ϸ�����ʽִ�У�ִ����Ϻ�ص�1
        private void Start()
        {
            PlayerInput.onActionTriggered += OnActionTriggered;
            SpecialMoveExecutionQueue ??= new();

            SpecialMoves?.Clear();
            SpecialMoves ??= new(SpecialMoveConfigs.Count);
            SpecialMoves.Capacity = SpecialMoveConfigs.Count;

            foreach (var config in SpecialMoveConfigs)
            {
                var item = Instantiate(SpecialMoveItemTemplatePrefab, SpecialMoveItemParent);
                item.Init(config);

                // ���� ��Ӧ��������1
                item.StartDetecting(PriorityQueue, PlayerLoopTiming, destroyCancellationToken).Forget();
                SpecialMoves.Add(item);
            }

            StartExecuting().Forget();
        }

        private void OnActionTriggered(InputAction.CallbackContext ctx)
        {
            var button = ButtonMapping.None;

            switch (ctx.action.name)
            {
                case ACTION_PUNCH:
                    Debug.Log($"Action: Punch, Phase: {ctx.phase}, Interaction: {ctx.interaction}");
                    if (ctx.performed)
                        button = ButtonMapping.Punch;
                    break;

                case ACTION_KICK:
                    Debug.Log($"Action: Kick, Phase: {ctx.phase}, Interaction: {ctx.interaction}");
                    if (ctx.performed)
                        button = ButtonMapping.Kick;
                    break;

                case ACTION_MOVE:
                    button = ctx.ReadValue<Vector2>().Vector2ButtonMapping();
                    break;

                default:
                    break;
            }

            var inputInfo = new InputInfo
            {
                Button = button,
                Ctx = ctx
            };

            foreach (var item in SpecialMoves)
                item.OnActionTriggered(inputInfo);
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
                SpecialMoveExecuteCommand moveCmd = null;
                while (SpecialMoveExecutionQueue.TryDequeue(out var cmd) && cmd.IsValid)
                {
                    moveCmd = cmd;
                    break;
                }

                if (moveCmd != null)
                    await moveCmd.Execute(PlayerLoopTiming, destroyCancellationToken);
            }
        }
    }
}