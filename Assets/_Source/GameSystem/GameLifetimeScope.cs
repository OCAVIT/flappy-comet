using CometSystem;
using SpawnSystem;
using UISystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using WorldSystem;

namespace GameSystem
{
    // Корень композиции: единственное место, где решается, кто от кого получает зависимости.
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] GameSettings settings;
        [SerializeField] Camera gameCamera;
        [SerializeField] Comet comet;
        [SerializeField] SparkField sparks;
        [SerializeField] GameHud hud;
        [SerializeField] Meteor meteorPrefab;
        [SerializeField] Star starPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(settings.flow);
            builder.RegisterInstance(settings.world);
            builder.RegisterInstance(settings.flight);
            builder.RegisterInstance(settings.spawn);

            builder.Register<IFlightInput, MouseAndSpaceFlightInput>(Lifetime.Singleton);
            builder.Register<IGameClock, UnityGameClock>(Lifetime.Singleton);
            builder.Register<GameSession>(Lifetime.Singleton).AsSelf().As<IGameSession>();
            builder.Register<WorldSpeed>(Lifetime.Singleton).AsSelf().As<IWorldSpeed>();

            builder.Register<IPlayField, CameraPlayField>(Lifetime.Singleton).WithParameter(gameCamera);
            builder.Register<IObstacleFactory, ObstacleFactory>(Lifetime.Singleton)
                .WithParameter(meteorPrefab)
                .WithParameter(starPrefab);

            builder.RegisterComponent(comet).As<IComet>();
            builder.RegisterComponent(sparks).As<ISparks>();
            builder.RegisterComponent(hud);

            builder.RegisterEntryPoint<GameFlow>();
            builder.RegisterEntryPoint<ObstacleSpawner>();
        }
    }
}
