using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace LaunchIt.Systems.Cannon
{
    public class CollectReceivers : StartOfDaySystem, IModSystem
    {
        public static Dictionary<int, List<Entity>> Channels = new();

        private EntityQuery ChannelUsers;
        protected override void Initialise()
        {
            base.Initialise();
            ChannelUsers = GetEntityQuery(new QueryHelper()
                .All(typeof(CChannelUser), typeof(CItemHolder)));
        }

        protected override void OnUpdate()
        {
            if (ChannelUsers.IsEmpty)
                return;

            Channels.Clear();
            using var entities = ChannelUsers.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var cChannel = GetComponent<CChannelUser>(entity);
                if (!cChannel.Receiver || cChannel.Channel == 0)
                    continue;

                if (!Channels.ContainsKey(cChannel.Channel))
                    Channels.Add(cChannel.Channel, new());
                Channels[cChannel.Channel].Add(entity);
            }
        }
    }
}
