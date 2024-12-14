using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.GameContent;

namespace ExpandedRoomsLib.HarmonyPatches
{
    [HarmonyPatch(typeof(RoomRegistry), "FindRoomForPosition")]
    public static class TransformRoom
    {
        public static void Postfix(ref Room __result)
        {
            if(__result.ExitCount != 0 || __result is ExpandedRoom) return;

            
            __result = ExpandedRoomsLibModSystem.Instance.FindOrCreateExpandedRoom(__result);
        }
    }
}
