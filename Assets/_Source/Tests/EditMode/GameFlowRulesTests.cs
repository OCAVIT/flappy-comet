using GameSystem;
using NUnit.Framework;
using VContainer;
using WorldSystem;

namespace FlappyCometTests
{
    // Правила хода игры без сцены: GameFlow собирает контейнер из настоящих GameSession и WorldSpeed и подставного окружения.
    public class GameFlowRulesTests
    {
        FakeFlightInput input;
        FakeClock clock;
        FakeComet comet;
        GameSession session;
        WorldSpeed world;
        GameFlow flow;
        IObjectResolver container;

        [SetUp]
        public void SetUp()
        {
            input = new FakeFlightInput();
            clock = new FakeClock();
            comet = new FakeComet();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(new FlowSettings { restartDelay = 0.35f });
            builder.RegisterInstance(new WorldSettings { startSpeed = 5f, maxSpeed = 9f, speedGrowth = 1f });
            builder.RegisterInstance<CometSystem.IFlightInput>(input);
            builder.RegisterInstance<IGameClock>(clock);
            builder.RegisterInstance<CometSystem.IComet>(comet);
            builder.Register<GameSession>(Lifetime.Singleton);
            builder.Register<WorldSpeed>(Lifetime.Singleton);
            builder.Register<GameFlow>(Lifetime.Singleton);
            container = builder.Build();

            session = container.Resolve<GameSession>();
            world = container.Resolve<WorldSpeed>();
            flow = container.Resolve<GameFlow>();
            flow.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            container.Dispose();
        }

        void Press()
        {
            input.Held = true;
            flow.Tick();
        }

        void Release()
        {
            input.Held = false;
            flow.Tick();
        }

        [Test]
        public void Waits_ForTheFirstPress()
        {
            flow.Tick();
            flow.Tick();

            Assert.That(session.State, Is.EqualTo(GameState.Ready));
            Assert.That(comet.Launches, Is.EqualTo(0));
            Assert.That(world.Current, Is.EqualTo(0f));
        }

        [Test]
        public void Press_StartsTheRun_AndLaunchesTheComet()
        {
            Press();

            Assert.That(session.State, Is.EqualTo(GameState.Playing));
            Assert.That(comet.Launches, Is.EqualTo(1));
            Assert.That(world.Current, Is.EqualTo(5f));
        }

        [Test]
        public void HoldingTheButton_DoesNotStartTheRunAgain()
        {
            Press();
            flow.Tick();
            flow.Tick();

            Assert.That(comet.Launches, Is.EqualTo(1));
        }

        [Test]
        public void World_SpeedsUp_OnlyWhilePlaying()
        {
            clock.DeltaTime = 1f;
            flow.Tick();
            Assert.That(world.Current, Is.EqualTo(0f));

            Press();
            flow.Tick();
            Assert.That(world.Current, Is.EqualTo(6f).Within(0.001f));
        }

        [Test]
        public void Star_AddsOnePoint()
        {
            Press();
            comet.CollectStar();
            comet.CollectStar();

            Assert.That(session.Score, Is.EqualTo(2));
        }

        [Test]
        public void Crash_EndsTheRun_StopsTheWorld_AndFreezesTime()
        {
            Press();
            comet.Crash();

            Assert.That(session.State, Is.EqualTo(GameState.GameOver));
            Assert.That(world.Current, Is.EqualTo(0f));
            Assert.That(clock.Frozen, Is.True);
        }

        [Test]
        public void ButtonHeldSinceTheCrash_DoesNotRestart()
        {
            Press();
            comet.Crash();
            clock.UnscaledTime += 10f;
            flow.Tick();

            Assert.That(session.State, Is.EqualTo(GameState.GameOver));
        }

        [Test]
        public void NewPress_RightAfterTheCrash_IsIgnored()
        {
            Press();
            comet.Crash();
            Release();
            clock.UnscaledTime += 0.1f;
            Press();

            Assert.That(session.State, Is.EqualTo(GameState.GameOver));
        }

        [Test]
        public void NewPress_AfterTheDelay_StartsAFreshRun()
        {
            Press();
            comet.CollectStar();
            comet.Crash();
            Release();
            clock.UnscaledTime += 0.4f;
            Press();

            Assert.That(session.State, Is.EqualTo(GameState.Playing));
            Assert.That(session.Score, Is.EqualTo(0));
            Assert.That(comet.Launches, Is.EqualTo(2));
            Assert.That(clock.Frozen, Is.False);
            Assert.That(world.Current, Is.EqualTo(5f));
        }

        [Test]
        public void Dispose_UnfreezesTime_SoTheNextSceneIsNotStuck()
        {
            Press();
            comet.Crash();

            flow.Dispose();

            Assert.That(clock.Frozen, Is.False);
        }
    }
}
