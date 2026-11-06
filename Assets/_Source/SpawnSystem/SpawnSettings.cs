using System;
using UnityEngine;

namespace SpawnSystem
{
    [Serializable]
    public class SpawnSettings
    {
        [Tooltip("Звезда появляется не дальше этого от центра по вертикали")]
        public float fieldHalfHeight = 3.55f;
        [Tooltip("Запас за краем экрана: объект появляется и исчезает невидимо")]
        public float offscreenMargin = 1.5f;
        [Tooltip("На сколько метеорит быстрее мира, единиц в секунду")]
        public Vector2 meteorExtraSpeed = new Vector2(2f, 4f);

        [Header("Промежутки, единиц пути")]
        public float firstMeteorAfter = 3f;
        public float firstStarAfter = 4f;
        public Vector2 meteorGapEasy = new Vector2(5.5f, 7.5f);
        public Vector2 meteorGapHard = new Vector2(4.5f, 6f);
        public Vector2 starGap = new Vector2(7f, 11f);
    }
}
