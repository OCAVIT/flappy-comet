using UnityEngine;
using VContainer;

namespace SpawnSystem
{
    // Квадратный метеорит: вылетает спереди на высоте кометы и летит прямо на неё, быстрее мира, оставляя след.
    public class Meteor : MonoBehaviour
    {
        [SerializeField] Vector2 sizeRange = new Vector2(1f, 1.3f);
        [SerializeField] Vector2 spinRange = new Vector2(10f, 45f);

        [SerializeField] ParticleSystem trail;
        [Tooltip("Длина следа в единицах мира, как у кометы")]
        [SerializeField] float trailLength = 4f;

        public float Speed { get; private set; }

        IPlayField field;
        float spin;

        [Inject]
        public void Construct(IPlayField playField)
        {
            field = playField;
        }

        public void Launch(float speed)
        {
            Speed = speed;
            transform.localScale = Vector3.one * Random.Range(sizeRange.x, sizeRange.y);
            spin = Random.Range(spinRange.x, spinRange.y) * (Random.value < 0.5f ? -1f : 1f);
            var main = trail.main;
            main.startLifetime = trailLength / speed;
        }

        void Update()
        {
            transform.position += Vector3.left * (Speed * Time.deltaTime);
            transform.Rotate(0f, 0f, spin * Time.deltaTime);
            // Метеорит удаляется вместе со следом, поэтому сначала за край должен уйти весь след.
            if (transform.position.x < field.ExitX - trailLength) Destroy(gameObject);
        }
    }
}
