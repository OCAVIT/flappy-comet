using System;
using UnityEngine;

namespace CometSystem
{
    [Serializable]
    public class FlightSettings
    {
        [Tooltip("Скорость подъёма при зажатой кнопке, единиц в секунду")]
        public float climbSpeed = 5.5f;
        [Tooltip("Скорость падения при отпущенной кнопке, единиц в секунду")]
        public float fallSpeed = 5.5f;
        [Tooltip("Разгон и торможение, единиц в секунду за секунду")]
        public float acceleration = 14f;
        [Tooltip("Длина шлейфа: от кометы до левого края экрана и чуть дальше, единиц")]
        public float trailLength = 3.6f;
    }
}
