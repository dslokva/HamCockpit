using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VE3NEA.HamCockpit.PluginHelpers;

namespace UN7ZO.HamCockpitPlugins.WheelTuning {

    enum WheelTuneStep : int {

        [Description("25 Hz")]
        TS_25 = 25,

        [Description("50 Hz")]
        TS_50 = 50,

        [Description("100 Hz")]
        TS_100 = 100,

        [Description("250 Hz")]
        TS_250 = 250,

        [Description("500 Hz")]
        TS_500 = 500,

        [Description("1000 Hz")]
        TS_1000 = 1000,

        [Description("3000 Hz")]
        TS_3000 = 3000
    }

    class Settings {
        [DisplayName("Mouse wheel tune rounding")]
        [Description("Round to nearest tune step value, if frequency was set by mouse click before")]
        [DefaultValue(true)]
        public bool MouseTuneRoundHz { get; set; } = true;

        [DisplayName("Mouse wheel tune step")]
        [Description("Select tuning step in Hz when using mouse wheel on Bandscope")]
        [DefaultValue(WheelTuneStep.TS_250)]
        [TypeConverter(typeof(EnumDescriptionConverter))]
        public WheelTuneStep MouseTuneHzStep { get; set; } = WheelTuneStep.TS_250;
    }
}
