using System;
using System.Collections.Generic;
using CometSystem;
using SpawnSystem;
using UISystem;
using UnityEngine;
using WorldSystem;

namespace FlappyCometTests
{
    // Подставные зависимости: классы игры не отличают их от настоящих, потому что знают только интерфейсы.
    sealed class FakeFlightInput : IFlightInput
    {
        public bool Held { get; set; }
    }

    sealed class FakeClock : IGameClock
    {
        public float DeltaTime { get; set; } = 0.02f;
        public float UnscaledTime { get; set; }
        public bool Frozen { get; private set; }

        public void Freeze() => Frozen = true;
        public void Resume() => Frozen = false;
    }

    sealed class FakeComet : IComet
    {
        public event Action StarCollected;
        public event Action Crashed;

        public float Height { get; set; }
        public int Launches { get; private set; }

        public void Launch() => Launches++;
        public void CollectStar() => StarCollected?.Invoke();
        public void Crash() => Crashed?.Invoke();
    }

    sealed class FakePlayField : IPlayField
    {
        public float SpawnX => 9.5f;
        public float ExitX => -9.5f;
    }

    sealed class FakeObstacleFactory : IObstacleFactory
    {
        public readonly List<(Vector2 position, float speed)> Meteors = new List<(Vector2, float)>();
        public readonly List<Vector2> Stars = new List<Vector2>();
        public int Clears { get; private set; }

        public Meteor CreateMeteor(Vector2 position, float speed)
        {
            Meteors.Add((position, speed));
            return null;
        }

        public Star CreateStar(Vector2 position)
        {
            Stars.Add(position);
            return null;
        }

        public void Clear()
        {
            Clears++;
            Meteors.Clear();
            Stars.Clear();
        }
    }

    sealed class FakeSparks : ISparks
    {
        public int Clears { get; private set; }

        public void Shed(Vector3 position) { }
        public void Burst(Vector3 position) { }
        public void Clear() => Clears++;
    }

    sealed class FakeHudView : IGameHudView
    {
        public int Score { get; private set; } = -1;
        public string Message { get; private set; }
        public bool HintVisible { get; private set; }

        public void ShowScore(int score) => Score = score;
        public void ShowMessage(string text) => Message = text;
        public void HideMessage() => Message = null;
        public void ShowControlsHint(bool visible) => HintVisible = visible;
    }
}
