using System;
using CometSystem;
using VContainer;
using VContainer.Unity;
using WorldSystem;

namespace GameSystem
{
    // Правила хода игры: старт, очки, конец и перезапуск. Новое нажатие запускает игру, столкновение её заканчивает.
    public sealed class GameFlow : IInitializable, ITickable, IDisposable
    {
        readonly IFlightInput input;
        readonly IComet comet;
        readonly GameSession session;
        readonly WorldSpeed world;
        readonly IGameClock clock;
        readonly FlowSettings settings;

        float restartAllowedAt;
        bool wasHeld;

        [Inject]
        public GameFlow(IFlightInput input, IComet comet, GameSession session, WorldSpeed world, IGameClock clock, FlowSettings settings)
        {
            this.input = input;
            this.comet = comet;
            this.session = session;
            this.world = world;
            this.clock = clock;
            this.settings = settings;
        }

        public void Initialize()
        {
            comet.StarCollected += OnStarCollected;
            comet.Crashed += OnCrashed;
        }

        public void Dispose()
        {
            comet.StarCollected -= OnStarCollected;
            comet.Crashed -= OnCrashed;
            clock.Resume();
        }

        public void Tick()
        {
            bool held = input.Held;
            bool pressed = held && !wasHeld;
            wasHeld = held;

            switch (session.State)
            {
                case GameState.Ready when pressed:
                case GameState.GameOver when pressed && clock.UnscaledTime >= restartAllowedAt:
                    StartRun();
                    break;
                case GameState.Playing:
                    world.Advance(clock.DeltaTime);
                    break;
            }
        }

        void StartRun()
        {
            clock.Resume();
            world.Restart();
            comet.Launch();
            session.StartRun();
        }

        void OnStarCollected()
        {
            session.AddPoint();
        }

        void OnCrashed()
        {
            if (session.State != GameState.Playing) return;
            restartAllowedAt = clock.UnscaledTime + settings.restartDelay;
            world.Halt();
            session.EndRun();
            clock.Freeze();
        }
    }
}
