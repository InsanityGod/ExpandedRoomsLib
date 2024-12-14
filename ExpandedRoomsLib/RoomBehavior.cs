using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace ExpandedRoomsLib
{
    public abstract class RoomBehavior
    {
        public ICoreServerAPI Api { get; protected set; }
        public virtual void Initialize(ICoreServerAPI api) => Api = api;


        /// <summary>
        /// Used to dispose whatever information should be disposed
        /// </summary>
        public virtual void Dispose()
        {
            //Empty stub
        }
    }
}
