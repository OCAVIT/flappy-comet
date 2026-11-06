using System;
using CometSystem;
using SpawnSystem;
using UnityEngine;
using WorldSystem;

namespace GameSystem
{
    // Весь баланс игры в одном ассете. Контейнер раздаёт каждому классу только его часть настроек.
    [CreateAssetMenu(menuName = "Flappy Comet/Game Settings", fileName = "GameSettings")]
    public class GameSettings : ScriptableObject
    {
        public FlowSettings flow = new FlowSettings();
        public WorldSettings world = new WorldSettings();
        public FlightSettings flight = new FlightSettings();
        public SpawnSettings spawn = new SpawnSettings();
    }

    [Serializable]
    public class FlowSettings
    {
        [Tooltip("Пауза после столкновения, чтобы случайное нажатие не перезапустило игру сразу, секунды")]
        public float restartDelay = 0.35f;
    }
}
