using System;

namespace ithappy.Adventure_Land
{
    public interface IObstacleMovementState
    {
        public void Overcome(ObstacleInfo obstacles, Action<bool> callback);
    }
}
