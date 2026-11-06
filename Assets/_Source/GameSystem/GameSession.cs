using System;
using VContainer;

namespace GameSystem
{
    public enum GameState
    {
        Ready,
        Playing,
        GameOver
    }

    // Что об игре можно узнать снаружи. Менять состояние может только GameFlow: у него есть сам GameSession.
    public interface IGameSession
    {
        event Action Changed;
        event Action RunStarted;
        event Action RunEnded;

        GameState State { get; }
        int Score { get; }
    }

    public sealed class GameSession : IGameSession
    {
        [Inject]
        public GameSession()
        {
        }

        public event Action Changed;
        public event Action RunStarted;
        public event Action RunEnded;

        public GameState State { get; private set; } = GameState.Ready;
        public int Score { get; private set; }

        public void StartRun()
        {
            Score = 0;
            State = GameState.Playing;
            RunStarted?.Invoke();
            Changed?.Invoke();
        }

        public void AddPoint()
        {
            Score++;
            Changed?.Invoke();
        }

        public void EndRun()
        {
            State = GameState.GameOver;
            RunEnded?.Invoke();
            Changed?.Invoke();
        }
    }
}
