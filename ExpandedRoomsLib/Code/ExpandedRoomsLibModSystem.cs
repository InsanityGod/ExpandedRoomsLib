using ExpandedRoomsLib.Code.Rooms;
using ExpandedRoomsLib.Code.Rooms.Behaviors;
using ExpandedRoomsLib.Code.Rooms.Behaviors.Temperature;
using ExpandedRoomsLib.Config;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ExpandedRoomsLib.Code
{
    public class ExpandedRoomsLibModSystem : ModSystem
    {
        private const string ConfigName = "ExpandedRoomsLib.json";

        public static ICoreServerAPI ServerApi { get; private set; }

        public static RoomRegistry RoomRegistry { get; private set; }

        public static HashSet<Type> RoomBehaviorTypes { get; } = new();

        public static void RegisterRoomBehavior<T>() where T : RoomBehavior => RoomBehaviorTypes.Add(typeof(T));

        public static HashSet<ExpandedRoom> ExpandedRooms { get; } = new();

        private Harmony harmony;

        internal static Room RoomCleanupCode(Room room, ICoreAPI api)
        {
            if(room is not ExpandedRoom expandedRoom) return room;

            var matchingRoom = ExpandedRooms.FirstOrDefault(expandedRoom => expandedRoom.Location.Equals(expandedRoom.Location));
            matchingRoom ??= ExpandedRooms.FirstOrDefault(expandedRoom => expandedRoom.Location.Intersects(expandedRoom.Location)); //TODO test wether this is a smart thing to do (probably not)

            if (matchingRoom != null)
            {
                Console.WriteLine("Migrated Room"); //TODO ensure correct location is migrated
                //Ensure they are no longer pending to be removed
                matchingRoom.PendingRemoval = false;
                matchingRoom.removalCount = 0;

                //Update fields
                matchingRoom.Volume = expandedRoom.Volume;
                matchingRoom.SurfaceCount = expandedRoom.SurfaceCount;

                matchingRoom.IsSmallRoom = expandedRoom.IsSmallRoom;
                matchingRoom.SkylightCount = expandedRoom.SkylightCount;
                matchingRoom.NonSkylightCount = expandedRoom.NonSkylightCount;
                matchingRoom.CoolingWallCount = expandedRoom.CoolingWallCount;
                matchingRoom.NonCoolingWallCount = expandedRoom.NonCoolingWallCount;
                matchingRoom.Location = expandedRoom.Location;
                matchingRoom.PosInRoom = expandedRoom.PosInRoom;
                matchingRoom.AnyChunkUnloaded = expandedRoom.AnyChunkUnloaded;

                return matchingRoom;
            }

            ExpandedRooms.Add(expandedRoom);
            expandedRoom.Initialize(api);
            return expandedRoom;
        }

        public static ModConfig Config { get; private set; }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            if(Config == null)
            {
                Config = api.LoadModConfig<ModConfig>(ConfigName);
                if(Config == null)
                {
                    Config = new ModConfig();
                    api.StoreModConfig(Config, ConfigName);
                }
            }
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            ServerApi = api;
            RoomRegistry = api.ModLoader.GetModSystem<RoomRegistry>();
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAll();
            }
            api.World.RegisterGameTickListener(CheckRoomDisposal, Config.RoomDisposalCheckInterval);
            
            if(Config.TemperatureEnabled) RegisterRoomBehavior<RoomTemperatureBehavior>();
            //TODO Oxygen not included
        }

        public static void CheckRoomDisposal(float _)
        {
            foreach (var room in ExpandedRooms)
            {
                if (!room.PendingRemoval) continue;
                if (room.removalCount++ > 2)
                {
                    if(RoomRegistry.GetRoomForPosition(room.Location.Center.AsBlockPos) == room) continue;

                    ExpandedRooms.Remove(room);
                    room.Dispose();
                }

            }
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll();
            RoomRegistry = null;
            ServerApi = null;
            Config = null;
            RoomBehaviorTypes.Clear();
            ExpandedRooms.Clear();
            base.Dispose();
        }
    }
}
