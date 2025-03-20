using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpandedRoomsLib.Config
{
    public class ModConfig
    {
        /// <summary>
        /// How often disposal check should run (in milliseconds)
        /// *Rooms only disappear after a few disposal checks (to ensure you can make small modifications without worrying)
        /// </summary>
        [Category("General")]
        [DisplayName("Disposal Check Interval")]
        [DefaultValue(5_000)]
        public int RoomDisposalCheckInterval {  get; set; } = 5_000;

        /// <summary>
        /// Wether the room temperature system should be enabled
        /// </summary>
        [Category("Room Temperature")]
        [DisplayName("Enabled")]
        [DefaultValue(true)]
        public bool TemperatureEnabled { get; set; } = true;
        
        /// <summary>
        /// How often the room should upate it's temperature (in milliseconds)
        /// </summary>
        [Category("Room Temperature")]
        [DisplayName("Update Interval")]
        [DefaultValue(5_000)]
        public int RoomTemperatureUpdateIntervalInMs { get; set; } = 5_000;
    }
}
