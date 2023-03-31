using KitchenData;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

namespace LaunchIt.Components
{
    public struct CTrampoline : IApplianceProperty, IModComponent
    {
        // Launching
        public float FlightDelta;
        public Entity Target;

        // Origin
        public bool LaunchedTo;
        public int Distance;
        public Quaternion Orientation;
    }
}
