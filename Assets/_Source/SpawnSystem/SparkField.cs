using UnityEngine;

namespace SpawnSystem
{
    // Одна система частиц на все оранжевые искры: её не уничтожают вместе со звездой, поэтому искры доживают свой век.
    public class SparkField : MonoBehaviour, ISparks
    {
        [SerializeField] ParticleSystem field;

        public void Clear()
        {
            field.Clear();
        }

        public void Shed(Vector3 position)
        {
            var spark = new ParticleSystem.EmitParams
            {
                position = position,
                velocity = Vector3.zero,
                startLifetime = 2.6f,
                startSize = Random.Range(0.1f, 0.15f)
            };
            field.Emit(spark, 1);
        }

        public void Burst(Vector3 position)
        {
            for (int i = 0; i < 12; i++)
            {
                var spark = new ParticleSystem.EmitParams
                {
                    position = position,
                    velocity = Random.insideUnitCircle.normalized * Random.Range(1f, 3.5f),
                    startLifetime = Random.Range(0.3f, 0.6f),
                    startSize = Random.Range(0.1f, 0.2f)
                };
                field.Emit(spark, 1);
            }
        }
    }
}
