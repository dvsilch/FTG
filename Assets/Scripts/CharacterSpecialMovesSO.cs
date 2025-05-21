using System.Collections.Generic;
using UnityEngine;

namespace Dvsilch
{
    [CreateAssetMenu(fileName = nameof(CharacterSpecialMovesSO), menuName = nameof(CharacterSpecialMovesSO))]
    public class CharacterSpecialMovesSO : ScriptableObject
    {
        [field: SerializeField]
        public List<SpecialMoveConfig> SpecialMoveConfigs { get; private set; } = new();
    }
}