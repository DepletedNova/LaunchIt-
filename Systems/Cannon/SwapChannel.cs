using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using Unity.Entities;
using UnityEngine;

namespace LaunchIt.Systems.Cannon
{
    [UpdateBefore(typeof(PickUpAndDropAppliance))]
    [UpdateBefore(typeof(RotateAppliances))]
    public class SwapChannel : ApplianceInteractionSystem, IModSystem
    {
        private EntityQuery ChannelGranters;
        protected override void Initialise()
        {
            base.Initialise();
            ChannelGranters = GetEntityQuery(new QueryHelper().All(typeof(CChannelUser)).Any(typeof(CCannon)));
        }

        protected override InteractionType RequiredType => InteractionType.Act;

        private CChannelUser cUser;
        private int channelCount;
        protected override bool IsPossible(ref InteractionData data)
        {
            channelCount = ChannelGranters.CalculateEntityCount();
            if (Require(data.Target, out cUser) && channelCount > 0)
                return true;
            return false;
        }

        protected override void Perform(ref InteractionData data)
        {
            cUser.Channel = (cUser.Channel + 1) % (channelCount + 1);
            Set(data.Target, cUser);
        }
    }
}
