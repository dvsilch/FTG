using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dvsilch
{
    public class SpecialMoveItem : MonoBehaviour
    {
        [field: SerializeField]
        public List<SpecialMoveButton> InputButtons { get; private set; }

        [field: SerializeField]
        public RectTransform ButtonsParent { get; private set; }

        [field: SerializeField]
        public TextMeshProUGUI SpecialMoveNameText { get; private set; }

        [field: SerializeField]
        public GameObject OkGo { get; private set; }

        [field: SerializeField]
        public SpecialMoveButton SpecialMoveButtonPrefab { get; private set; }

        public PlayerInput PlayerInput { get; private set; }

        public SpecialMoveConfig SpecialMoveConfig { get; private set; }

        public InputActionAsset InputActionAsset { get; private set; }

        private bool isTriggering;

        public bool IsTriggering
        {
            get => isTriggering;
            set
            {
                isTriggering = value;
                OkGo.SetActive(value);
            }
        }

        public void Init(SpecialMoveConfig specialMoveConfig, InputActionAsset asset)
        {
            SpecialMoveConfig = specialMoveConfig;
            SpecialMoveNameText.text = specialMoveConfig.Name;

            InputButtons?.Clear();
            InputButtons ??= new List<SpecialMoveButton>(SpecialMoveConfig.InputButtons.Count);
            InputButtons.Capacity = SpecialMoveConfig.InputButtons.Count;

            foreach (var config in SpecialMoveConfig.InputButtons)
            {
                var button = Instantiate(SpecialMoveButtonPrefab, ButtonsParent);
                button.Init(config, asset);
                InputButtons.Add(button);
            }
        }

        public void ResetButton()
        {
            foreach (var inputButton in InputButtons)
            {
                inputButton.SetActive(false);
            }
        }

        public async UniTask StartDetecting(PriorityQueue<SpecialMoveExecuteCommand, int> priorityQueue, PlayerLoopTiming timing, CancellationToken ct)
        {
            while (true)
            {
                await UniTask.WaitWhile(() => IsTriggering, timing, ct);

                var success = false;
                var i = 0;
                foreach (var inputButton in InputButtons)
                {
                    if (ct.IsCancellationRequested)
                        return;

                    success = await inputButton.StartWaiting(timing, ct);
                    if (success)
                    {
                        Debug.Log($"{SpecialMoveConfig.Name} {inputButton.InputButtonConfig.ButtonSprite}输入检测成功，进入下一阶段判定");
                        i++;
                    }
                    else
                    {
                        if (i > 0)
                        {
                            Debug.Log($"{SpecialMoveConfig.Name} {inputButton.InputButtonConfig.ButtonSprite}输入检测失败，重置");
                        }
                        break;
                    }
                }

                if (success)
                {
                    var cmd = new SpecialMoveExecuteCommand(this);
                    cmd.Start(timing, ct).Forget();
                    priorityQueue.Enqueue(cmd, int.MaxValue - SpecialMoveConfig.Priority);
                }
                else
                {
                    if (i != 0 && !IsTriggering)
                        ResetButton();
                }

                await UniTask.NextFrame(timing, ct);
            }
        }
    }
}