using System;
using CometSystem;
using GameSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using WorldSystem;
using Random = UnityEngine.Random;

namespace SpawnSystem
{
    // Выпускает из-за правого края экрана метеориты, каждый на высоте кометы, и звёзды на случайной высоте.
    // Промежутки считаются в пройденном пути, а с ростом сложности метеориты идут чаще.
    public sealed class ObstacleSpawner : IInitializable, ITickable, IDisposable
    {
        readonly IGameSession session;
        readonly IWorldSpeed world;
        readonly IGameClock clock;
        readonly IComet comet;
        readonly IPlayField field;
        readonly IObstacleFactory factory;
        readonly ISparks sparks;
        readonly SpawnSettings settings;

        float untilMeteor;
        float untilStar;

        [Inject]
        public ObstacleSpawner(IGameSession session, IWorldSpeed world, IGameClock clock, IComet comet, IPlayField field,
            IObstacleFactory factory, ISparks sparks, SpawnSettings settings)
        {
            this.session = session;
            this.world = world;
            this.clock = clock;
            this.comet = comet;
            this.field = field;
            this.factory = factory;
            this.sparks = sparks;
            this.settings = settings;
        }

        public void Initialize()
        {
            session.RunStarted += OnRunStarted;
        }

        public void Dispose()
        {
            session.RunStarted -= OnRunStarted;
        }

        void OnRunStarted()
        {
            factory.Clear();
            sparks.Clear();
            untilMeteor = settings.firstMeteorAfter;
            untilStar = settings.firstStarAfter;
        }

        public void Tick()
        {
            if (session.State != GameState.Playing) return;

            float moved = world.Current * clock.DeltaTime;
            untilMeteor -= moved;
            untilStar -= moved;

            if (untilMeteor <= 0f)
            {
                float speed = world.Current + Random.Range(settings.meteorExtraSpeed.x, settings.meteorExtraSpeed.y);
                factory.CreateMeteor(new Vector2(field.SpawnX, comet.Height), speed);
                Vector2 gap = Vector2.Lerp(settings.meteorGapEasy, settings.meteorGapHard, world.Difficulty);
                untilMeteor += Random.Range(gap.x, gap.y);
            }
            if (untilStar <= 0f)
            {
                factory.CreateStar(new Vector2(field.SpawnX, Random.Range(-settings.fieldHalfHeight, settings.fieldHalfHeight)));
                untilStar += Random.Range(settings.starGap.x, settings.starGap.y);
            }
        }
    }
}
