using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

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

        public SpecialMoveConfig SpecialMoveConfig { get; private set; }

        private bool isTriggering;

        private int currentInputButtonIndex;

        public bool IsTriggering
        {
            get => isTriggering;
            set
            {
                isTriggering = value;
                OkGo.SetActive(value);
            }
        }

        public void Init(SpecialMoveConfig specialMoveConfig)
        {
            SpecialMoveConfig = specialMoveConfig;
            SpecialMoveNameText.text = specialMoveConfig.Name;

            InputButtons?.Clear();
            InputButtons ??= new List<SpecialMoveButton>(SpecialMoveConfig.InputButtons.Count);
            InputButtons.Capacity = SpecialMoveConfig.InputButtons.Count;

            foreach (var config in SpecialMoveConfig.InputButtons)
            {
                var button = Instantiate(SpecialMoveButtonPrefab, ButtonsParent);
                button.Init(config);
                InputButtons.Add(button);
            }
        }

        public void ResetButton()
        {
            foreach (var inputButton in InputButtons)
            {
                inputButton.SetActive(false);
            }
            currentInputButtonIndex = 0;
        }

        public async UniTask StartDetecting(PriorityQueue<SpecialMoveExecuteCommand, int> priorityQueue, PlayerLoopTiming timing, CancellationToken ct)
        {
            while (true)
            {
                await UniTask.WaitWhile(() => IsTriggering, timing, ct);
                var success = false;

                foreach (var inputButton in InputButtons)
                {
                    if (ct.IsCancellationRequested)
                        return;

                    success = await inputButton.StartWaiting(timing, ct);
                    if (success)
                    {
                        Debug.Log($"{SpecialMoveConfig.Name} {inputButton.InputButtonConfig.ButtonSprite}输入检测成功，进入下一阶段判定");
                        currentInputButtonIndex++;
                    }
                    else
                        break;
                }

                if (success)
                {
                    var cmd = new SpecialMoveExecuteCommand(this);
                    cmd.Start(timing, ct).Forget();
                    priorityQueue.Enqueue(cmd, int.MaxValue - SpecialMoveConfig.Priority);
                    currentInputButtonIndex = 0;
                }
                else
                {
                    if (currentInputButtonIndex > 0)
                        Debug.Log($"{SpecialMoveConfig.Name} {InputButtons[currentInputButtonIndex].InputButtonConfig.ButtonSprite}输入检测失败，重置");

                    ResetButton();
                }

                await UniTask.NextFrame(timing, ct);
            }
        }

        public void OnActionTriggered(InputInfo inputInfo)
        {
            InputButtons[currentInputButtonIndex].SetResult(inputInfo);
        }
    }
}