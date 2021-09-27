using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Infinitus
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "InfinitusMind/Game Settings", order = 0)]
    public class GameSettings : SingletonScriptableObject<GameSettings>
    {
        public bool debug;

        public enum CharacterType
        {
            Both,
            Model,
            Spine
        }

        public CharacterType characterType;

        [ReadOnly] public int version;
        [ReadOnly] public int subVersion;
    }
}