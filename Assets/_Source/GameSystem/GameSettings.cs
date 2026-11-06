using System;
using CometSystem;
using SpawnSystem;
using UnityEngine;

namespace GameSystem
{
    // Весь баланс игры в одном ассете. Контейнер раздаёт каждому классу только его часть настроек.
    [CreateAssetMenu(menuName = "Flappy Comet/Game Settings", fileName = "GameSettings")]
    public class GameSettings : ScriptableObject
    {
        public WorldSettings world = new WorldSettings();
        public FlightSettings flight = new FlightSettings();
        public SpawnSettings spawn = new SpawnSettings();
    }

    [Serializable]
    public class WorldSettings
    {
        [Tooltip("Скорость мира в начале игры, единиц в секунду")]
        public float startSpeed = 5f;
        [Tooltip("Максимальная скорость мира, единиц в секунду")]
        public float maxSpeed = 9f;
        [Tooltip("Рост скорости мира, единиц в секунду за секунду")]
        public float speedGrowth = 0.06f;
        [Tooltip("Пауза после столкновения, чтобы случайное нажатие не перезапустило игру сразу, секунды")]
        public float restartDelay = 0.35f;
    }
}
