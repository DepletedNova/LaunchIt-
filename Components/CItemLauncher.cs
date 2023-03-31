using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Entities;

namespace LaunchIt.Components
{
    public struct CItemLauncher : IApplianceProperty, IModComponent
    {
        // Appliance Variables
        public bool TargetSmart;
        public bool CrossesWalls;
        public float Cooldown;
        public float LaunchSpeed;
        public int MaxTileRange;
        public int MinTileRange;
        public bool SingleTarget;
        public bool TargetFar;

        // Generic
        public float FlightDelta;
        public int LaunchDistance;
        public Entity CurrentTarget;
        public LauncherState State;

        public enum LauncherState
        {
            Idle = 0,
            Launching = 1,
            Reloading = 2,
        }

        // Smart
        public int SmartTargetID;
    }
}