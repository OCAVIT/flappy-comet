using System;
using SpawnSystem;
using UnityEngine;

namespace CometSystem
{
    // Комета стоит на месте по горизонтали, летит мир. Зажато: разгон вверх. Отпущено: разгон вниз.
    // Шлейф — частицы в мировых координатах, которые уносит влево со скоростью мира, поэтому он повторяет путь кометы.
    public class Comet : MonoBehaviour
    {
        [SerializeField] float climbSpeed = 5.5f;
        [SerializeField] float fallSpeed = 5.5f;
        [SerializeField] float acceleration = 14f;

        [SerializeField] ParticleSystem trail;
        [Tooltip("Длина шлейфа в единицах мира: от кометы до левого края экрана и чуть дальше")]
        [SerializeField] float trailLength = 3.6f;

        public event Action StarCollected;
        public event Action Crashed;

        public bool Holding { get; set; }
        public float WorldSpeed { get; set; }
        public float Velocity { get; private set; }
        public float ClimbSpeed => climbSpeed;
        public float FallSpeed => fallSpeed;
        public float Acceleration => acceleration;

        Vector3 startPosition;
        bool flying;

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

            float target = Holding ? climbSpeed : -fallSpeed;
            Velocity = Mathf.MoveTowards(Velocity, target, acceleration * Time.deltaTime);
            transform.position += Vector3.up * (Velocity * Time.deltaTime);

            if (WorldSpeed > 0f)
            {
                var velocity = trail.velocityOverLifetime;
                velocity.x = -WorldSpeed;
                var main = trail.main;
                main.startLifetime = trailLength / WorldSpeed;
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
