using UnityEngine;
using VContainer;

namespace WorldSystem
{
    // Время игры. Статический Time спрятан за интерфейсом, поэтому правила игры проверяются без сцены и без ожидания.
    public interface IGameClock
    {
        float DeltaTime { get; }
        float UnscaledTime { get; }
        void Freeze();
        void Resume();
    }

    public sealed class UnityGameClock : IGameClock
    {
        [Inject]
        public UnityGameClock()
        {
        }

        public float DeltaTime => Time.deltaTime;
        public float UnscaledTime => Time.unscaledTime;

        // Весь мир замирает в момент столкновения: и препятствия, и шлейф, и искры.
        public void Freeze() => Time.timeScale = 0f;
        public void Resume() => Time.timeScale = 1f;
    }
}
