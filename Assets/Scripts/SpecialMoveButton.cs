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

        private AutoResetUniTaskCompletionSource<bool> utcs;

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
                ButtonMapping.Stop => 8,
                _ => -1
            };

            if (idx >= 0)
            {
                ButtonImage.sprite = ButtonSprites[idx];
            }
        }

        public void Init(InputButtonConfig config)
        {
            InputButtonConfig = config;
            SetSprite();
        }

        public async UniTask<bool> StartWaiting(PlayerLoopTiming timing, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return false;

            DelaySetResult(
                InputButtonConfig.Button == ButtonMapping.Stop,
                (int)(InputButtonConfig.DelayTime * 1000),
                timing, ct).Forget();
            var (isCanceled, isMatch) = await (utcs ??= AutoResetUniTaskCompletionSource<bool>.Create()).Task.SuppressCancellationThrow();
            return !isCanceled && isMatch;
        }

        public void SetResult(InputInfo inputInfo)
        {
            if (utcs == null)
                return;

            var (button, ctx) = inputInfo;

            if (button != ButtonMapping.None)
            {
                var match = (button & InputButtonConfig.Button) > 0;

                if (match)
                {
                    SetResult(true);
                }
                else
                {
                    //Debug.Log($"当前输入: {button} 不匹配 {InputButtonConfig.Button}");
                    // 不匹配时，只有按下攻击键，才清空状态
                    if ((button == ButtonMapping.Punch || button == ButtonMapping.Kick) && ctx.phase == InputActionPhase.Started)
                        SetResult(false);
                }
            }
        }

        private async UniTaskVoid DelaySetResult(bool result, int delay, PlayerLoopTiming timing, CancellationToken ct)
        {
            await UniTask.Delay(delay, delayTiming: timing, cancellationToken: ct);
            SetResult(result);
        }

        private void SetResult(bool result)
        {
            if (utcs != null)
            {
                SetActive(result);
                utcs.TrySetResult(result);
                utcs = null;
            }
        }

        private void OnDestroy()
        {
            SetResult(false);
        }

        public void SetActive(bool active)
        {
            ButtonImage.color = active ? Color.green : Color.white;
        }
    }
}