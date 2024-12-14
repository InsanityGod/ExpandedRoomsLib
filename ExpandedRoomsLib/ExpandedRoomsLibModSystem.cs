
using HarmonyLib;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ExpandedRoomsLib
{
    public class ExpandedRoomsLibModSystem : ModSystem
    {
        public ExpandedRoomsLibModSystem() => Instance = this;
        public static ExpandedRoomsLibModSystem Instance { get; private set; }

        public ICoreServerAPI ServerApi { get; private set; }
        
        private Harmony harmony;

        internal readonly HashSet<Type> roomBehaviorTypes = new();

        public void RegisterRoomBehavior<T>() where T : RoomBehavior => roomBehaviorTypes.Add(typeof(T));

        internal HashSet<ExpandedRoom> expandedRooms = new();
        internal Room FindOrCreateExpandedRoom(Room room)
        {
            //PERFORMANCE: see if we can maybe improve performance on this by grouping chunk based?
            foreach(var expandedRoom in expandedRooms)
            {
                //TODO allow for rooms to changed slightly and still be recognized
                if (expandedRoom.Location.Equals(room.Location))
                {
                    expandedRoom.PendingRemoval = false; //Ensure they are no longer pending to be removed
                    return expandedRoom;
                }
            }

            var newExpandedRoom = new ExpandedRoom
            {
                ExitCount = room.ExitCount,
                IsSmallRoom = room.IsSmallRoom,
                SkylightCount = room.SkylightCount,
                NonSkylightCount = room.NonSkylightCount,
                CoolingWallCount = room.CoolingWallCount,
                NonCoolingWallCount = room.NonCoolingWallCount,
                Location = room.Location,
                PosInRoom = room.PosInRoom,
                AnyChunkUnloaded = room.AnyChunkUnloaded
            };
            expandedRooms.Add(newExpandedRoom);
            newExpandedRoom.Initialize();

            return newExpandedRoom;
        }

        internal RoomRegistry registry;

        public override void StartServerSide(ICoreServerAPI api)
        {
            ServerApi = api;
            registry = api.ModLoader.GetModSystem<RoomRegistry>();
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAll();
            }
            api.World.RegisterGameTickListener(CheckRoomDisposal, 5_000);
        }

        public void CheckRoomDisposal(float _)
        {
            foreach(var room in expandedRooms)
            {
                if(!room.PendingRemoval) continue;
                if(room.removalCount++ > 2)
                {
                    expandedRooms.Remove(room);
                    room.Dispose();
                }

            }
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll();
            Instance = null;
            base.Dispose();
        }
    }
}
