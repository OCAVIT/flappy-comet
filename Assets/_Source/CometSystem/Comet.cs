using System;
using SpawnSystem;
using UnityEngine;
using VContainer;

namespace CometSystem
{
    // Комета стоит на месте по горизонтали, летит мир. Зажато: разгон вверх. Отпущено: разгон вниз.
    // Шлейф — частицы в мировых координатах, которые уносит влево со скоростью мира, поэтому он повторяет путь кометы.
    public class Comet : MonoBehaviour
    {
        [SerializeField] ParticleSystem trail;

        public event Action StarCollected;
        public event Action Crashed;

        public bool Holding { get; set; }
        public float WorldSpeed { get; set; }
        public float Velocity { get; private set; }

        FlightSettings settings;
        Vector3 startPosition;
        bool flying;

        [Inject]
        public void Construct(FlightSettings flightSettings)
        {
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

            float target = Holding ? settings.climbSpeed : -settings.fallSpeed;
            Velocity = Mathf.MoveTowards(Velocity, target, settings.acceleration * Time.deltaTime);
            transform.position += Vector3.up * (Velocity * Time.deltaTime);

            if (WorldSpeed > 0f)
            {
                var velocity = trail.velocityOverLifetime;
                velocity.x = -WorldSpeed;
                var main = trail.main;
                main.startLifetime = settings.trailLength / WorldSpeed;
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
