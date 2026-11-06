using System;
using UnityEngine;
using VContainer;

namespace WorldSystem
{
    [Serializable]
    public class WorldSettings
    {
        [Tooltip("Скорость мира в начале игры, единиц в секунду")]
        public float startSpeed = 5f;
        [Tooltip("Максимальная скорость мира, единиц в секунду")]
        public float maxSpeed = 9f;
        [Tooltip("Рост скорости мира, единиц в секунду за секунду")]
        public float speedGrowth = 0.06f;
    }

    // То, что нужно знать о мире остальным: как быстро он летит и насколько уже сложно.
    public interface IWorldSpeed
    {
        float Current { get; }
        float Difficulty { get; }
    }

    // Скорость мира: растёт за время игры и останавливается при столкновении. Управляет ей только GameFlow.
    public sealed class WorldSpeed : IWorldSpeed
    {
        readonly WorldSettings settings;
        float speed;
        bool moving;

        [Inject]
        public WorldSpeed(WorldSettings settings)
        {
            this.settings = settings;
            speed = settings.startSpeed;
        }

        public float Current => moving ? speed : 0f;
        public float Difficulty => Mathf.InverseLerp(settings.startSpeed, settings.maxSpeed, speed);

        public void Restart()
        {
            speed = settings.startSpeed;
            moving = true;
        }

        public void Advance(float deltaTime)
        {
            speed = Mathf.MoveTowards(speed, settings.maxSpeed, settings.speedGrowth * deltaTime);
        }

        public void Halt()
        {
            moving = false;
        }
    }
}
