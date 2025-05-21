using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Dvsilch
{
    public class SpecialMoveButton : MonoBehaviour
    {
        [field: SerializeField]
        public List<Sprite> ButtonSprites { get; private set; }

        [field: SerializeField]
        public Image ButtonImage { get; private set; }

        public InputButtonConfig InputButtonConfig { get; private set; }

        public InputActionAsset InputActionAsset { get; private set; }

        public const string ACTION_PUNCH = "Punch";

        public const string ACTION_KICK = "Kick";

        public const string ACTION_MOVE = "Move";

        public InputAction PunchAction { get; private set; }

        public InputAction KickAction { get; private set; }

        public InputAction MoveAction { get; private set; }

        private void SetSprite()
        {
            var idx = InputButtonConfig.ButtonSprite switch
            {
                ButtonMapping.Left => 6,
                ButtonMapping.Right => 2,
                ButtonMapping.Up => 0,
                ButtonMapping.Down => 4,
                ButtonMapping.LeftUp => 7,
                ButtonMapping.LeftDown => 5,
                ButtonMapping.RightUp => 1,
                ButtonMapping.RightDown => 3,
                ButtonMapping.Punch => 9,
                ButtonMapping.Kick => 10,
                ButtonMapping.None => 8,
                _ => -1
            };

            if (idx >= 0)
            {
                ButtonImage.sprite = ButtonSprites[idx];
            }
        }

        public void Init(InputButtonConfig config, InputActionAsset asset)
        {
            InputButtonConfig = config;
            InputActionAsset = asset;
            SetSprite();

            PunchAction = InputActionAsset.FindAction(ACTION_PUNCH);
            KickAction = InputActionAsset.FindAction(ACTION_KICK);
            MoveAction = InputActionAsset.FindAction(ACTION_MOVE);
        }

        public async UniTask<bool> StartWaiting(PlayerLoopTiming timing, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return false;

            var time = 0f;

            while (time < InputButtonConfig.DelayTime)
            {
                var button = ButtonMapping.None;

                if (PunchAction.IsPressed())
                {
                    //Debug.Log($"Action: Punch, Phase: {PunchAction.phase}, Interaction: {PunchAction.started}");
                    button = ButtonMapping.Punch;
                }
                else if (KickAction.IsPressed())
                    button = ButtonMapping.Kick;
                else if (MoveAction.IsPressed())
                    button = MoveAction.ReadValue<Vector2>().Vector2ButtonMapping();

                if (button != ButtonMapping.None)
                {
                    var match = (button & InputButtonConfig.Button) > 0;

                    if (match)
                    {
                        SetActive(true);
                        return true;
                    }
                    else
                    {
                        Debug.Log($"当前输入: {button} 不匹配 {InputButtonConfig.Button}");
                        // 不匹配时，只有按下攻击键，才清空状态
                        if (PunchAction.phase == InputActionPhase.Started || KickAction.phase == InputActionPhase.Started)
                        {
                            return false;
                        }
                    }
                }
                else if (InputButtonConfig.Button == ButtonMapping.None) // 专门处理空输入
                {
                    //var t = InputButtonConfig.DelayTime;
                    //while (t > 0)
                    //{
                    //    await UniTask.NextFrame(timing, ct);
                    //    if (PunchAction.IsPressed() || KickAction.IsPressed())
                    //        return false;
                    //    t -= Time.deltaTime;
                    //}
                    SetActive(true);
                    return true;
                }

                await UniTask.NextFrame(timing, ct);
                time += Time.deltaTime;
            }

            return false;
        }

        public void SetActive(bool active)
        {
            ButtonImage.color = active ? Color.green : Color.white;
        }
    }
}