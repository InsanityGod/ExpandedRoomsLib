using ExpandedRoomsLib.Code.Rooms.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace ExpandedRoomsLib.Code.Rooms
{
    /// <summary>
    /// This room is purely server side only, interaction with it should be done server side and then synced to client if needed
    /// </summary>
    public class ExpandedRoom : Room
    {
        public List<RoomBehavior> RoomBehaviors { get; set; } = new();

        internal List<ExpandedRoom> expandedRooms = new();

        public T GetBehavior<T>() where T : RoomBehavior => RoomBehaviors.OfType<T>().FirstOrDefault();
        
        public ICoreAPI Api { get; private set; }
        internal void Initialize(ICoreAPI api)
        {
            Api = api;
            foreach (var behaviorType in ExpandedRoomsLibModSystem.RoomBehaviorTypes)
            {
                try
                {
                    var behavior = (RoomBehavior)Activator.CreateInstance(behaviorType);
                    RoomBehaviors.Add(behavior);
                    behavior.Initialize(ExpandedRoomsLibModSystem.ServerApi, this);
                }
                catch (Exception ex)
                {
                    ExpandedRoomsLibModSystem.ServerApi.Logger.Error($"Failed to initialize room behavior {behaviorType.Name}: {ex}");
                }
            }

            Console.WriteLine("An expanded room has been created"); //TODO remove
        }

        public bool PendingRemoval { get; set; }

        /// <summary>
        /// Keeps track of how many times it was found during RemovalCheck, if this reaches 3 it will be removed
        /// </summary>
        internal int removalCount;

        public int Volume;

        public int SurfaceCount;

        public ExpandedRoom()
        {
            //TODO remove
        }

        public bool IsDisposed { get; protected set; }

        internal void Dispose()
        {
            foreach (var behavior in RoomBehaviors) behavior.Dispose();

            IsDisposed = true;
            Console.WriteLine("An expanded room has been disposed"); //TODO remove
        }
    }
}
