using System;
using System.Collections;
using CometSystem;
using GameSystem;
using NUnit.Framework;
using SpawnSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace FlappyCometTests
{
    // Правила игры на настоящей сцене: удержание и отпускание, звёзды, метеориты, полосы, перезапуск и автопилот.
    public class GameFlowTests
    {
        GameController game;
        Comet comet;
        Spawner spawner;
        bool held;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("Game");
            game = Object.FindAnyObjectByType<GameController>();
            comet = Object.FindAnyObjectByType<Comet>();
            spawner = Object.FindAnyObjectByType<Spawner>();
            held = false;
            game.HeldSource = () => held;
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
        }

        IEnumerator Tap()
        {
            held = true;
            yield return null;
            yield return null;
        }

        IEnumerator Until(Func<bool> condition, float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, $"Condition not met in {seconds} s");
        }

        [UnityTest]
        public IEnumerator Ready_WaitsForPress_AndNothingFlies()
        {
            yield return new WaitForSeconds(0.3f);

            Assert.That(game.State, Is.EqualTo(GameState.Ready));
            Assert.That(comet.transform.position.y, Is.EqualTo(0f));
            Assert.That(Object.FindObjectsByType<Meteor>(FindObjectsSortMode.None), Is.Empty);
        }

        [UnityTest]
        public IEnumerator Holding_FliesUp_AndReleasing_FliesDown()
        {
            yield return Tap();
            Assert.That(game.State, Is.EqualTo(GameState.Playing));

            float before = comet.transform.position.y;
            yield return new WaitForSeconds(0.35f);
            Assert.That(comet.transform.position.y, Is.GreaterThan(before + 0.2f), "Holding must lift the comet");

            held = false;
            yield return Until(() => comet.Velocity < -1f, 2f);
            float high = comet.transform.position.y;
            yield return new WaitForSeconds(0.2f);
            Assert.That(comet.transform.position.y, Is.LessThan(high - 0.1f), "Releasing must drop the comet");
        }

        [UnityTest]
        public IEnumerator Star_GivesOnePoint_AndDisappears()
        {
            yield return Tap();
            Vector3 position = comet.transform.position;
            Star star = spawner.SpawnStar(position.x + 1.5f, position.y + 0.4f);

            yield return Until(() => game.Score == 1, 3f);
            yield return null;

            Assert.That(star == null, Is.True, "Collected star must be removed");
            Assert.That(game.State, Is.EqualTo(GameState.Playing));
        }

        [UnityTest]
        public IEnumerator Meteor_EndsTheGame_AndFreezesTheWorld()
        {
            yield return Tap();
            Vector3 position = comet.transform.position;
            spawner.SpawnMeteor(position.x + 1.5f, position.y + 0.4f);

            yield return Until(() => game.State == GameState.GameOver, 3f);

            Assert.That(Time.timeScale, Is.EqualTo(0f));
            Assert.That(game.WorldSpeed, Is.EqualTo(0f));
        }

        [UnityTest]
        public IEnumerator Meteors_FlyStraightAtTheComet_FromAhead_WithATrail()
        {
            yield return Tap();
            held = false;
            yield return Until(() => Object.FindAnyObjectByType<Meteor>() != null, 4f);

            Meteor meteor = Object.FindAnyObjectByType<Meteor>();
            Vector3 start = meteor.transform.position;
            Assert.That(start.y, Is.EqualTo(comet.transform.position.y).Within(0.3f), "Meteor must appear at the comet's height");
            Assert.That(start.x, Is.GreaterThan(Camera.main.orthographicSize * Camera.main.aspect - 1f), "Meteor must appear at the right edge");
            Assert.That(meteor.Speed, Is.GreaterThan(game.WorldSpeed));
            ParticleSystem trail = meteor.GetComponentInChildren<ParticleSystem>();
            Assert.That(trail.isPlaying, Is.True);

            held = true;
            yield return new WaitForSeconds(0.25f);
            Assert.That(meteor.transform.position.x, Is.LessThan(start.x - 1f));
            Assert.That(meteor.transform.position.y, Is.EqualTo(start.y).Within(0.001f));
            Assert.That(trail.particleCount, Is.GreaterThan(5));
        }

        [UnityTest]
        public IEnumerator BottomBand_EndsTheGame_WhenReleased()
        {
            yield return Tap();
            held = false;
            yield return Until(() => game.State == GameState.GameOver, 5f);
            Assert.That(comet.transform.position.y, Is.LessThan(-3f));
        }

        [UnityTest]
        public IEnumerator TopBand_EndsTheGame_WhenHeld()
        {
            yield return Tap();
            yield return Until(() => game.State == GameState.GameOver, 5f);
            Assert.That(comet.transform.position.y, Is.GreaterThan(3f));
        }

        [UnityTest]
        public IEnumerator NewPress_AfterCrash_StartsAFreshRun()
        {
            yield return Tap();
            Vector3 position = comet.transform.position;
            spawner.SpawnStar(position.x + 0.5f, position.y);
            yield return Until(() => game.Score == 1, 2f);
            spawner.SpawnMeteor(position.x + 1.5f, comet.transform.position.y);
            yield return Until(() => game.State == GameState.GameOver, 3f);

            // Кнопка всё ещё зажата с момента столкновения: само по себе это не перезапуск.
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.That(game.State, Is.EqualTo(GameState.GameOver));

            held = false;
            yield return null;
            yield return Tap();

            Assert.That(game.State, Is.EqualTo(GameState.Playing));
            Assert.That(game.Score, Is.EqualTo(0));
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(Mathf.Abs(comet.transform.position.y), Is.LessThan(0.2f));
            yield return null;
            Assert.That(Object.FindObjectsByType<Meteor>(FindObjectsSortMode.None), Is.Empty);
        }

        // Игра проходима: автопилот уворачивается от метеоритов, собирает звёзды и доходит до максимальной скорости.
        // Автопилот эвристический и может ошибиться, поэтому даётся до трёх попыток: для доказательства хватит одной.
        [UnityTest, Timeout(300000)]
        public IEnumerator Autopilot_ReachesMaxSpeed_AndCollectsStars()
        {
            UnityEngine.Random.InitState(2026);
            const int Attempts = 3;
            bool reached = false;
            int score = 0;
            for (int attempt = 0; attempt < Attempts && !reached; attempt++)
            {
                game.HeldSource = () => held;
                held = false;
                yield return new WaitForSecondsRealtime(0.5f);
                yield return Tap();
                game.HeldSource = Autopilot;

                Time.timeScale = 3f;
                while (game.State == GameState.Playing && game.Difficulty < 1f) yield return null;
                reached = game.State == GameState.Playing;
                score = game.Score;
                Debug.Log($"[Autopilot] attempt {attempt + 1}: reached max speed {reached}, score {score}, difficulty {game.Difficulty:0.00}");
            }
            Time.timeScale = 1f;

            Assert.That(reached, Is.True, $"Autopilot crashed before max speed in all {Attempts} attempts");
            Assert.That(score, Is.GreaterThanOrEqualTo(5));
        }

        // Перебирает планы «держать столько-то секунд, потом отпустить» и наоборот,
        // отбрасывает ведущие к столкновению и берёт план, который ближе всего подводит к следующей звезде.
        bool Autopilot()
        {
            Vector3 position = comet.transform.position;
            Meteor[] meteors = Object.FindObjectsByType<Meteor>(FindObjectsSortMode.None);

            float target = 0f;
            float nearest = float.MaxValue;
            float arrival = RehearsalHorizon;
            foreach (Star star in Object.FindObjectsByType<Star>(FindObjectsSortMode.None))
            {
                float ahead = star.transform.position.x - position.x;
                if (ahead > -0.2f && ahead < nearest)
                {
                    nearest = ahead;
                    target = star.transform.position.y;
                    arrival = Mathf.Max(ahead, 0f) / Mathf.Max(game.WorldSpeed, 1f);
                }
            }

            float best = float.NegativeInfinity;
            bool hold = false;
            foreach (bool first in new[] { true, false })
            {
                foreach (float switchAt in new[] { 0.05f, 0.1f, 0.2f, 0.3f, 0.45f, 0.6f, 0.8f, 1.1f, 1.5f })
                {
                    float score = Rehearse(position, meteors, first, switchAt, target, arrival);
                    if (score > best)
                    {
                        best = score;
                        hold = first;
                    }
                }
            }
            return hold;
        }

        const float RehearsalHorizon = 1.5f;

        // Оценка плана: столкновение отбрасывает его, иначе важно, насколько близко к звезде комета будет в момент её подлёта.
        float Rehearse(Vector3 position, Meteor[] meteors, bool first, float switchAt, float target, float arrival)
        {
            const float Step = 0.03f;
            float y = position.y;
            float atArrival = float.NaN;
            float velocity = comet.Velocity;
            for (float t = Step; t <= RehearsalHorizon; t += Step)
            {
                bool holding = t < switchAt ? first : !first;
                velocity = Mathf.MoveTowards(velocity, holding ? comet.ClimbSpeed : -comet.FallSpeed, comet.Acceleration * Step);
                y += velocity * Step;
                if (Mathf.Abs(y) > 3.5f) return -1000f + t;
                if (float.IsNaN(atArrival) && t >= arrival) atArrival = y;

                foreach (Meteor meteor in meteors)
                {
                    Vector3 m = meteor.transform.position;
                    float dx = m.x - meteor.Speed * t - position.x;
                    float dy = m.y - y;
                    float reach = meteor.transform.localScale.x * 0.71f + 0.33f + 0.1f;
                    if (dx * dx + dy * dy < reach * reach) return -1000f + t;
                }
            }
            return -Mathf.Abs((float.IsNaN(atArrival) ? y : atArrival) - target);
        }
    }
}
