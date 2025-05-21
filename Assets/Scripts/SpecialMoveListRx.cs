using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Dvsilch
{
    public class SpecialMoveListRx : MonoBehaviour
    {
        [field: SerializeField]
        public InputActionAsset InputActionAsset { get; private set; }

        [field: SerializeField]
        public CharacterSpecialMovesSO CharacterSpecialMovesSO { get; private set; }

        [field: SerializeField]
        public InputAction PunchAction { get; private set; }

        [field: SerializeField]
        public InputAction KickAction { get; private set; }

        [field: SerializeField]
        public InputAction MoveAction { get; private set; }

        public const string ACTION_PUNCH = "Punch";

        public const string ACTION_KICK = "Kick";

        public const string ACTION_MOVE = "Move";

        public List<SpecialMoveConfig> SpecialMoveConfigs => CharacterSpecialMovesSO.SpecialMoveConfigs;

        private void Start()
        {
            InputActionAsset = Instantiate(InputActionAsset);
            InputActionAsset.Enable();

            PunchAction = InputActionAsset.FindAction(ACTION_PUNCH);
            KickAction = InputActionAsset.FindAction(ACTION_KICK);
            MoveAction = InputActionAsset.FindAction(ACTION_MOVE);

            var updateStream = Observable.EveryUpdate(destroyCancellationToken);

            var moveActionInputStream = updateStream
                .Select(_ => MoveAction.ReadValue<Vector2>())
                .Where(v => v.sqrMagnitude > 0.5) // 随便写的一个值
                .Select(Vector2ButtonMapping);
            var punchActionInputStream = updateStream
                .Where(_ => PunchAction.WasPerformedThisFrame())
                .Select(_ => ButtonMapping.Punch);
            var kickActionInputStream = updateStream
                .Where(_ => KickAction.WasPerformedThisFrame())
                .Select(_ => ButtonMapping.Kick);
            var inputStream = moveActionInputStream
                .Merge(punchActionInputStream)
                .Merge(kickActionInputStream);

            inputStream.Subscribe(button => Debug.Log(button));

            var config = SpecialMoveConfigs[Random.Range(0, SpecialMoveConfigs.Count)];
            // 写一个根据config生成的observable流
            // 这个流会在config的delayTime内等待输入
            Observable<Unit> specialObservable = null;
            foreach (var item in config.InputButtons)
            {
                var timeoutObservable = Observable.Timer(TimeSpan.FromSeconds(item.DelayTime))
                    .Select(_ => ButtonMapping.None);

                var inputObservable = inputStream.Where(button => (button & item.Button) > 0);
            }
        }

        public static ButtonMapping Vector2ButtonMapping(Vector2 vector2)
        {
            var angle = (Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg + 360) % 360;
            if (angle >= 337.5f || angle < 22.5f)
                return ButtonMapping.Right;
            else if (angle >= 22.5f && angle < 67.5f)
                return ButtonMapping.RightUp;
            else if (angle >= 67.5f && angle < 112.5f)
                return ButtonMapping.Up;
            else if (angle >= 112.5f && angle < 157.5f)
                return ButtonMapping.LeftUp;
            else if (angle >= 157.5f && angle < 202.5f)
                return ButtonMapping.Left;
            else if (angle >= 202.5f && angle < 247.5f)
                return ButtonMapping.LeftDown;
            else if (angle >= 247.5f && angle < 292.5f)
                return ButtonMapping.Down;
            else if (angle >= 292.5f && angle < 337.5f)
                return ButtonMapping.RightDown;

            return ButtonMapping.None;
        }
    }
}