using UnityEngine;

namespace SpawnSystem
{
    // Звезда даёт очко при касании. Пока летит, роняет редкие искры, которые остаются позади неё.
    public class Star : MonoBehaviour
    {
        [SerializeField] float sparkInterval = 0.9f;
        [SerializeField] float sparkSpread = 0.35f;

        SparkField sparks;
        float untilSpark;

        public void Init(SparkField field)
        {
            sparks = field;
            untilSpark = Random.Range(0f, sparkInterval);
        }

        void Update()
        {
            untilSpark -= Time.deltaTime;
            if (untilSpark > 0f) return;
            untilSpark += sparkInterval;
            sparks.Shed(transform.position + (Vector3)(Random.insideUnitCircle * sparkSpread));
        }

        public void Collect()
        {
            sparks.Burst(transform.position);
            Destroy(gameObject);
        }
    }
}
