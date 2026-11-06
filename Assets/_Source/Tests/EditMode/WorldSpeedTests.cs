using NUnit.Framework;
using WorldSystem;

namespace FlappyCometTests
{
    public class WorldSpeedTests
    {
        static WorldSpeed Create() => new WorldSpeed(new WorldSettings { startSpeed = 5f, maxSpeed = 9f, speedGrowth = 2f });

        [Test]
        public void StandsStill_UntilRestarted()
        {
            Assert.That(Create().Current, Is.EqualTo(0f));
        }

        [Test]
        public void Grows_AndStopsAtTheMaximum()
        {
            WorldSpeed world = Create();
            world.Restart();

            world.Advance(1f);
            Assert.That(world.Current, Is.EqualTo(7f).Within(0.001f));
            Assert.That(world.Difficulty, Is.EqualTo(0.5f).Within(0.001f));

            world.Advance(100f);
            Assert.That(world.Current, Is.EqualTo(9f));
            Assert.That(world.Difficulty, Is.EqualTo(1f));
        }

        [Test]
        public void Halt_StopsTheWorld_AndKeepsTheDifficulty()
        {
            WorldSpeed world = Create();
            world.Restart();
            world.Advance(1f);

            world.Halt();

            Assert.That(world.Current, Is.EqualTo(0f));
            Assert.That(world.Difficulty, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void Restart_ReturnsToTheStartSpeed()
        {
            WorldSpeed world = Create();
            world.Restart();
            world.Advance(100f);

            world.Restart();

            Assert.That(world.Current, Is.EqualTo(5f));
            Assert.That(world.Difficulty, Is.EqualTo(0f));
        }
    }
}
