using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.GameContent;
using ExpandedRoomsLib.Code;
using ExpandedRoomsLib.Code.Rooms;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ExpandedRoomsLib.Code.HarmonyPatches
{
    [HarmonyPatch(typeof(RoomRegistry), "Event_ChunkDirty")]
    public static class TranspileRoomDeletion
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);
            var normalRouteLabel = generator.DefineLabel();

            // Locate the `Dictionary.Remove` call by finding the opcodes.
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt &&
                    codes[i].operand is MethodInfo method &&
                    method.Name == "Remove" &&
                    method.DeclaringType.IsGenericType &&
                    method.DeclaringType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    codes[i - 10].labels.Add(normalRouteLabel);

                    codes.InsertRange(i - 10, new CodeInstruction[]
                    {
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldfld, AccessTools.Field(typeof(RoomRegistry), "api")),
                        new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ICoreAPI), nameof(ICoreAPI.Side))),
                        new(OpCodes.Ldc_I4_2),
                        new(OpCodes.Beq_S, normalRouteLabel),
                        new(OpCodes.Ldloc_2),
                        new(OpCodes.Call, AccessTools.Method(typeof(TranspileRoomDeletion), nameof(OnChunkRoomsRemoved)))
                    });

                    break;
                }
            }
            return codes;
        }

        public static void OnChunkRoomsRemoved(FastSetOfLongs removed)
        {
            var chunkRooms = Traverse.Create(ExpandedRoomsLibModSystem.RoomRegistry).Field<Dictionary<long, ChunkRooms>>("roomsByChunkIndex").Value;

            foreach (var chunkIndex in removed)
            {
                if (!chunkRooms.TryGetValue(chunkIndex, out var chunkRoom)) continue;
                
                Console.WriteLine($"Disposing Chunk: {chunkIndex}");
                foreach (ExpandedRoom expandedRoom in chunkRoom.Rooms.OfType<ExpandedRoom>())
                {
                    expandedRoom.PendingRemoval = true;
                    expandedRoom.removalCount = 0;
                }
            }
        }
    }

}
