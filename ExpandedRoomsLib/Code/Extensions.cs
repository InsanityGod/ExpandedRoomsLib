using ExpandedRoomsLib.Code.Rooms;
using ExpandedRoomsLib.Code.Rooms.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.GameContent;

namespace ExpandedRoomsLib.Code
{
    public static class Extensions
    {
        public static T TryGetBehavior<T>(this Room room) where T : RoomBehavior => (room as ExpandedRoom)?.TryGetBehavior<T>();
    }
}
