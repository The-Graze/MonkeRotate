using UnityEngine;

namespace MonkeSwim.Utils
{
    public readonly struct AverageDirection
    {
        private readonly uint directionAmount; // stores how many directions have been added
        private readonly Vector3 direction;
        private readonly float speed;

        public uint Directions { get { return directionAmount; } private set { } }
        public float Speed { get { return ((directionAmount > 1 && speed != 0f) ? speed / directionAmount : speed); } }
        public Vector3 Direction { get { return direction.normalized; } private set { } }
        static public AverageDirection Zero { get { return new AverageDirection(Vector3.zero, 0f, 0); } private set { } }

        public AverageDirection(Vector3 dir, float speedVal, uint dirAmount = 1)
        {
            direction = dir;
            speed = speedVal;
            directionAmount = dirAmount <= 0 ? 0 : dirAmount;
        }

        public static AverageDirection operator +(AverageDirection first, AverageDirection secound)
        {
            return new AverageDirection(first.direction + secound.direction, first.speed + secound.speed, first.directionAmount + 1);
        }

        public static AverageDirection operator -(AverageDirection first, AverageDirection secound)
        {
            return new AverageDirection(first.direction - secound.direction, first.speed - secound.speed, (first.directionAmount > 0 ? first.directionAmount - 1 : 0));
        }
    }
}