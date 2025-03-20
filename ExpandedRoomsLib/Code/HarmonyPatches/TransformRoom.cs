using ExpandedRoomsLib.Code;
using ExpandedRoomsLib.Code.Rooms;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace ExpandedRoomsLib.Code.HarmonyPatches
{
    [HarmonyPatch]
    public static class TransformRoom
    {
        [HarmonyPatch(typeof(RoomRegistry), nameof(RoomRegistry.GetRoomForPosition))]
        [HarmonyPostfix]
        public static void CleanupPendingRemovalPostfix(RoomRegistry __instance, Room __result)
        {
            if(__result is ExpandedRoom expandedRoom)
            {
                expandedRoom.PendingRemoval = false;
                expandedRoom.removalCount = 0;
            }
        }

        [HarmonyPatch(typeof(RoomRegistry), "FindRoomForPosition")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CreateExpandedRoomTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();

            var surfaceCountLocal = generator.DeclareLocal(typeof(int));

            var isClientLabel = generator.DefineLabel();
            var hasExitsLabel = generator.DefineLabel();
            var isServerLabel = generator.DefineLabel();

            var constructorToFind = AccessTools.Constructor(typeof(Room));

            codes.InsertRange(0, new CodeInstruction[]
            {
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Stloc_S, surfaceCountLocal)
            });

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Newobj && code.operand is ConstructorInfo constructor && constructor  == constructorToFind)
                {
                    var startCode = new CodeInstruction(OpCodes.Ldarg_0)
                    {
                        labels = code.labels
                    };

                    code.labels = new()
                    {
                        isClientLabel,
                        hasExitsLabel
                    };
                    codes[i + 1].labels.Add(isServerLabel);
                    
                    //TODO keep track of surface count
                    codes.InsertRange(i, new CodeInstruction[]
                    {
                        startCode,
                        new(OpCodes.Ldfld, AccessTools.Field(typeof(RoomRegistry), "api")),
                        new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ICoreAPI), nameof(ICoreAPI.Side))),
                        new(OpCodes.Ldc_I4_2),
                        new(OpCodes.Beq_S, isClientLabel),
                        new(OpCodes.Ldloc_S, 9),
                        new(OpCodes.Brtrue, hasExitsLabel), //TODO maybe look at volume as well //TODO maybe with configurable minimum size as well
                        new(OpCodes.Newobj, AccessTools.Constructor(typeof(ExpandedRoom))),
                        new(OpCodes.Dup),
                        new(OpCodes.Ldloc_S, 29),
                        new(OpCodes.Stfld, AccessTools.Field(typeof(ExpandedRoom), nameof(ExpandedRoom.Volume))),
                        new(OpCodes.Dup),
                        new(OpCodes.Ldloc_S, surfaceCountLocal),
                        new(OpCodes.Stfld, AccessTools.Field(typeof(ExpandedRoom), nameof(ExpandedRoom.SurfaceCount))),
                        new(OpCodes.Br, isServerLabel)
                    });

                    codes.InsertRange(codes.Count - 1, new CodeInstruction[]
                    {
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldfld, AccessTools.Field(typeof(RoomRegistry), "api")),
                        new(OpCodes.Call, AccessTools.Method(typeof(ExpandedRoomsLibModSystem), nameof(ExpandedRoomsLibModSystem.RoomCleanupCode))),
                    });
                    break;
                }
                else if (code.opcode == OpCodes.Stloc_S && code.operand is LocalBuilder local && (local.LocalIndex == 5 || local.LocalIndex == 6) && codes[i -1].opcode != OpCodes.Ldc_I4_0)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[]
                    {
                        new(OpCodes.Ldloc, surfaceCountLocal),
                        new(OpCodes.Ldc_I4_1),
                        new(OpCodes.Add),
                        new(OpCodes.Stloc, surfaceCountLocal)
                    });

                    i += 4;
                }
            }

            return codes;
        }
    }
}
