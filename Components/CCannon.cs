using KitchenData;
using KitchenMods;
using Unity.Entities;

namespace LaunchIt.Components
{
    public struct CCannon : IApplianceProperty, IModComponent
    {
        public float Cooldown;
        public float FireSpeed;
        public float MaxRange;
        public float MinRange;

        public CannonState State;
        public float FlightDelta;
        public float RotationDelta;
        public Entity Target;

        public enum CannonState
        {
            Idle = 0,
            Turning = 1,
            Firing = 2,
            Reloading = 3,
        }
    }
}