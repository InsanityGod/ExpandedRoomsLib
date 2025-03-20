using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace ExpandedRoomsLib.Code.Rooms.Behaviors
{
    public abstract class RoomBehavior
    {
        public ICoreServerAPI Api { get; private set; }

        public ExpandedRoom ExpandedRoom { get; private set; }
        public virtual void Initialize(ICoreServerAPI api, ExpandedRoom expandedRoom)
        {
            Api = api;
            ExpandedRoom = expandedRoom;
        }


        /// <summary>
        /// Used to dispose whatever information should be disposed
        /// </summary>
        public virtual void Dispose()
        {
            //Empty stub
        }
    }
}
