using GameSystem;
using NUnit.Framework;
using SpawnSystem;
using UnityEngine;
using WorldSystem;

namespace FlappyCometTests
{
    // Правила появления препятствий без сцены и префабов: фабрика подставная и только записывает, что её просили создать.
    public class ObstacleSpawnerTests
    {
        readonly SpawnSettings settings = new SpawnSettings
        {
            fieldHalfHeight = 3f,
            meteorExtraSpeed = new Vector2(2f, 4f),
            firstMeteorAfter = 3f,
            firstStarAfter = 4f,
            meteorGapEasy = new Vector2(6f, 6f),
            meteorGapHard = new Vector2(2f, 2f),
            starGap = new Vector2(8f, 8f)
        };

        GameSession session;
        WorldSpeed world;
        FakeClock clock;
        FakeComet comet;
        FakeObstacleFactory factory;
        FakeSparks sparks;
        ObstacleSpawner spawner;

        [SetUp]
        public void SetUp()
        {
            session = new GameSession();
            world = new WorldSpeed(new WorldSettings { startSpeed = 5f, maxSpeed = 10f, speedGrowth = 0f });
            clock = new FakeClock { DeltaTime = 0.1f };
            comet = new FakeComet { Height = 1.5f };
            factory = new FakeObstacleFactory();
            sparks = new FakeSparks();
            spawner = new ObstacleSpawner(session, world, clock, comet, new FakePlayField(), factory, sparks, settings);
            spawner.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            spawner.Dispose();
        }

        void StartRun()
        {
            world.Restart();
            session.StartRun();
        }

        // 0,1 с на скорости 5: каждый такт мир проходит 0,5 единицы пути.
        void Fly(float distance)
        {
            for (float passed = 0f; passed < distance - 0.001f; passed += 0.5f) spawner.Tick();
        }

        [Test]
        public void NothingSpawns_BeforeTheRunStarts()
        {
            Fly(50f);

            Assert.That(factory.Meteors, Is.Empty);
            Assert.That(factory.Stars, Is.Empty);
        }

        [Test]
        public void FirstMeteor_FliesFromBeyondTheRightEdge_AtTheCometsHeight_FasterThanTheWorld()
        {
            StartRun();
            Fly(2.5f);
            Assert.That(factory.Meteors, Is.Empty);

            Fly(0.5f);

            Assert.That(factory.Meteors, Has.Count.EqualTo(1));
            (Vector2 position, float speed) = factory.Meteors[0];
            Assert.That(position, Is.EqualTo(new Vector2(9.5f, 1.5f)));
            Assert.That(speed, Is.InRange(7f, 9f));
        }

        [Test]
        public void EveryMeteor_TakesTheCometsCurrentHeight()
        {
            StartRun();
            Fly(3f);
            comet.Height = -2f;
            // С небольшим запасом: Random.Range(6, 6) может вернуть чуть больше шести из-за округления.
            Fly(6.5f);

            Assert.That(factory.Meteors, Has.Count.EqualTo(2));
            Assert.That(factory.Meteors[1].position.y, Is.EqualTo(-2f));
        }

        [Test]
        public void Stars_AppearWithinTheField()
        {
            StartRun();
            Fly(100f);

            Assert.That(factory.Stars.Count, Is.GreaterThan(5));
            foreach (Vector2 star in factory.Stars)
            {
                Assert.That(star.x, Is.EqualTo(9.5f));
                Assert.That(Mathf.Abs(star.y), Is.LessThanOrEqualTo(3f));
            }
        }

        [Test]
        public void Meteors_ComeMoreOften_AsDifficultyGrows()
        {
            StartRun();
            Fly(60f);
            int easy = factory.Meteors.Count;

            var hardWorld = new WorldSpeed(new WorldSettings { startSpeed = 5f, maxSpeed = 5.0001f, speedGrowth = 1f });
            hardWorld.Restart();
            hardWorld.Advance(1f);
            var hardSession = new GameSession();
            var hardFactory = new FakeObstacleFactory();
            var hard = new ObstacleSpawner(hardSession, hardWorld, clock, comet, new FakePlayField(), hardFactory, sparks, settings);
            hard.Initialize();
            hardSession.StartRun();
            for (int tick = 0; tick < 120; tick++) hard.Tick();
            hard.Dispose();

            Assert.That(hardFactory.Meteors.Count, Is.GreaterThan(easy));
        }

        [Test]
        public void SpawningStops_WhenTheRunEnds()
        {
            StartRun();
            Fly(10f);
            int meteors = factory.Meteors.Count;

            world.Halt();
            session.EndRun();
            Fly(50f);

            Assert.That(factory.Meteors, Has.Count.EqualTo(meteors));
        }

        [Test]
        public void NewRun_ClearsObstaclesAndSparks_AndCountsFromTheStart()
        {
            StartRun();
            Fly(20f);
            session.EndRun();

            StartRun();

            Assert.That(factory.Clears, Is.EqualTo(2));
            Assert.That(sparks.Clears, Is.EqualTo(2));
            Assert.That(factory.Meteors, Is.Empty);
            Fly(2.5f);
            Assert.That(factory.Meteors, Is.Empty);
            Fly(0.5f);
            Assert.That(factory.Meteors, Has.Count.EqualTo(1));
        }
    }
}
