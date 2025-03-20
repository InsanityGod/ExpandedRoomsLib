using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace ExpandedRoomsLib.Code.Rooms.Behaviors.Temperature
{
    public class RoomTemperatureBehavior : RoomBehavior
    {
        //TODO save data somehow
        public float Temperature { get; set; }
        private long tickListener;

        public override void Initialize(ICoreServerAPI api, ExpandedRoom expandedRoom)
        {
            base.Initialize(api, expandedRoom);
            
            var climate = api.World.BlockAccessor.GetClimateAt(ExpandedRoom.Location.Center.AsBlockPos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, Api.World.Calendar.TotalDays);
            Temperature = climate.Temperature;
            tickListener = Api.World.RegisterGameTickListener(OnGameTick, ExpandedRoomsLibModSystem.Config.RoomTemperatureUpdateIntervalInMs, Api.World.Rand.Next(0, ExpandedRoomsLibModSystem.Config.RoomTemperatureUpdateIntervalInMs));
        }

        public float TemperatureRetentionFactor
        {
            get
            {
                //Calculate how well the walls retain heat
                var heatRetentionScore = ExpandedRoom.NonCoolingWallCount - ExpandedRoom.CoolingWallCount;

                return heatRetentionScore / (float)ExpandedRoom.SurfaceCount;
            } 
        }

        private void OnGameTick(float secondsPassed)
        {
            // Update temperature based on climate and room information
            var climate = Api.World.BlockAccessor.GetClimateAt(ExpandedRoom.Location.Center.AsBlockPos, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, Api.World.Calendar.TotalDays);
            float outsideTemperature = climate.Temperature;

            // Rate at which room temperature changes towards outside temperature
            float temperatureChangeRate = 0.1f;
            
            Temperature += (outsideTemperature - Temperature) * temperatureChangeRate * secondsPassed; //TODO use calander time instead of seconds passed

            // Ensure temperature stays within reasonable bounds
            Temperature = Math.Clamp(Temperature, -30f, 50f);
            Console.WriteLine($"Room Score: {TemperatureRetentionFactor} temperature: {Temperature}");
        }

        public override void Dispose()
        {
            base.Dispose();
            Api.World.UnregisterGameTickListener(tickListener);
        }
    }
}
