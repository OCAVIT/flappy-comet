using CometSystem;
using SpawnSystem;
using UISystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameSystem
{
    // Корень композиции: единственное место, где решается, кто от кого получает зависимости.
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] GameController game;
        [SerializeField] Comet comet;
        [SerializeField] Spawner spawner;
        [SerializeField] SparkField sparks;
        [SerializeField] GameHud hud;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(game);
            builder.RegisterComponent(comet);
            builder.RegisterComponent(spawner);
            builder.RegisterComponent(sparks);
            builder.RegisterComponent(hud);
        }
    }
}
