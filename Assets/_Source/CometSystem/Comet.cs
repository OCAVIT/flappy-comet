using System;
using SpawnSystem;
using UnityEngine;
using VContainer;
using WorldSystem;

namespace CometSystem
{
    // Комета стоит на месте по горизонтали, летит мир. Зажато: разгон вверх. Отпущено: разгон вниз.
    // Шлейф — частицы в мировых координатах, которые уносит влево со скоростью мира, поэтому он повторяет путь кометы.
    public class Comet : MonoBehaviour, IComet
    {
        [SerializeField] ParticleSystem trail;

        public event Action StarCollected;
        public event Action Crashed;

        public float Height => transform.position.y;
        public float Velocity { get; private set; }

        IFlightInput input;
        IWorldSpeed world;
        FlightSettings settings;
        Vector3 startPosition;
        bool flying;

        [Inject]
        public void Construct(IFlightInput flightInput, IWorldSpeed worldSpeed, FlightSettings flightSettings)
        {
            input = flightInput;
            world = worldSpeed;
            settings = flightSettings;
        }

        void Awake()
        {
            startPosition = transform.position;
        }

        public void Launch()
        {
            transform.position = startPosition;
            Velocity = 0f;
            flying = true;
            trail.Clear();
            trail.Play();
        }

        void Update()
        {
            if (!flying) return;

            float target = input.Held ? settings.climbSpeed : -settings.fallSpeed;
            Velocity = Mathf.MoveTowards(Velocity, target, settings.acceleration * Time.deltaTime);
            transform.position += Vector3.up * (Velocity * Time.deltaTime);

            float worldSpeed = world.Current;
            if (worldSpeed > 0f)
            {
                var velocity = trail.velocityOverLifetime;
                velocity.x = -worldSpeed;
                var main = trail.main;
                main.startLifetime = settings.trailLength / worldSpeed;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!flying) return;

            if (other.TryGetComponent(out Star star))
            {
                star.Collect();
                StarCollected?.Invoke();
            }
            else if (other.TryGetComponent(out Hazard _))
            {
                Crashed?.Invoke();
            }
        }
    }
}
