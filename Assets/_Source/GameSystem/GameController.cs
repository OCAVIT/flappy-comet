using System;
using CometSystem;
using UnityEngine;
using VContainer;

namespace GameSystem
{
    public enum GameState
    {
        Ready,
        Playing,
        GameOver
    }

    // Ход игры: старт, счёт, скорость мира, конец и перезапуск.
    // Ввод один на всю игру: зажатая ЛКМ или пробел. Новое нажатие запускает игру, удержание ведёт комету вверх.
    public class GameController : MonoBehaviour
    {
        public event Action Changed;
        public event Action RunStarted;
        public event Action RunEnded;

        public GameState State { get; private set; } = GameState.Ready;
        public int Score { get; private set; }
        public float WorldSpeed => State == GameState.Playing ? speed : 0f;
        public float Difficulty => Mathf.InverseLerp(settings.startSpeed, settings.maxSpeed, speed);

        // Источник ввода. Тесты подставляют сюда своего игрока.
        public Func<bool> HeldSource { get; set; } = () => Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);

        Comet comet;
        WorldSettings settings;
        float speed;
        float restartAllowedAt;
        bool wasHeld;

        [Inject]
        public void Construct(Comet cometInScene, WorldSettings worldSettings)
        {
            comet = cometInScene;
            settings = worldSettings;
            speed = settings.startSpeed;
        }

        void OnEnable()
        {
            comet.StarCollected += OnStarCollected;
            comet.Crashed += OnCrashed;
        }

        void OnDisable()
        {
            comet.StarCollected -= OnStarCollected;
            comet.Crashed -= OnCrashed;
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        void Update()
        {
            bool held = HeldSource();
            bool pressed = held && !wasHeld;
            wasHeld = held;

            switch (State)
            {
                case GameState.Ready when pressed:
                case GameState.GameOver when pressed && Time.unscaledTime >= restartAllowedAt:
                    StartRun();
                    break;
                case GameState.Playing:
                    speed = Mathf.MoveTowards(speed, settings.maxSpeed, settings.speedGrowth * Time.deltaTime);
                    break;
            }

            comet.Holding = held;
            comet.WorldSpeed = WorldSpeed;
        }

        void StartRun()
        {
            Time.timeScale = 1f;
            Score = 0;
            speed = settings.startSpeed;
            State = GameState.Playing;
            comet.Launch();
            RunStarted?.Invoke();
            Changed?.Invoke();
        }

        void OnStarCollected()
        {
            Score++;
            Changed?.Invoke();
        }

        void OnCrashed()
        {
            if (State != GameState.Playing) return;
            State = GameState.GameOver;
            restartAllowedAt = Time.unscaledTime + settings.restartDelay;
            RunEnded?.Invoke();
            // Весь мир замирает в момент столкновения: и препятствия, и шлейф, и искры.
            Time.timeScale = 0f;
            Changed?.Invoke();
        }
    }
}
