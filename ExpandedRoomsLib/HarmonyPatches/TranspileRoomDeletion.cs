using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.GameContent;

namespace ExpandedRoomsLib.HarmonyPatches
{
    [HarmonyPatch(typeof(RoomRegistry), "Event_ChunkDirty")]
    public static class TranspileRoomDeletion
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Locate the `Dictionary.Remove` call by finding the opcodes.
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt &&
                    codes[i].operand is MethodInfo method &&
                    method.Name == "Remove" &&
                    method.DeclaringType.IsGenericType &&
                    method.DeclaringType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    codes.Insert(i - 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TranspileRoomDeletion), nameof(OnChunkRoomsRemoved))));
                    codes.Insert(i - 3, new CodeInstruction(OpCodes.Ldloc_S, 13));
                    break;
                }
            }

            return codes;
        }

        // Static method to handle the removed ChunkRooms
        public static void OnChunkRoomsRemoved(long removed)
        {
            //TODO test if this also gets triggered when mod is added but only required server side
            var chunkRooms = Traverse.Create(ExpandedRoomsLibModSystem.Instance.registry).Field<Dictionary<long, ChunkRooms>>("roomsByChunkIndex").Value;
            if(!chunkRooms.TryGetValue(removed, out var chunkRoom)) return;
            foreach (ExpandedRoom expandedRoom in chunkRoom.Rooms.OfType<ExpandedRoom>()) 
            {
                expandedRoom.PendingRemoval = true;
            }
        }
    }

}
