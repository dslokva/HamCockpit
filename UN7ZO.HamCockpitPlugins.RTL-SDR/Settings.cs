using System;
using System.ComponentModel;
using VE3NEA.HamCockpit.PluginHelpers;

namespace UN7ZO.HamCockpitPlugins.RTL_SDR
{

    enum SampleRatesVariants : uint {
        [Description("1.024 MSPS")]
        SAMPLE_FREQ_1MSPS = 1024000,

        [Description("2.4 MSPS")]
        SAMPLE_FREQ_2_40MSPS = 2400000,

        [Description("2.56 MSPS")]
        SAMPLE_FREQ_2_56MSPS = 2560000,

        [Description("3.2 MSPS")]
        SAMPLE_FREQ_3_2MSPS = 3200000
    }

    class Settings {
        private int _deviceVGAGain;
        private int _deviceLNAGain;
        private int _deviceFreqCorrecton;

        [DisplayName("0. Available devices")]
        [Description("Please select a device.")]
        [TypeConverter(typeof(DeviceListValueConverter))]
        public int SelectedDeviceIndex { get; set; }

        [DisplayName("1. Sampling Rate")]
        [Description("Receiver's output sampling rate")]
        [TypeConverter(typeof(EnumDescriptionConverter))]
        [DefaultValue(SampleRatesVariants.SAMPLE_FREQ_2_56MSPS)]
        public SampleRatesVariants SamplingRate { get; set; } = SampleRatesVariants.SAMPLE_FREQ_2_56MSPS;

        [DisplayName("2. Device RTL AGC")]
        [Description("RTL AGC option is enabled")]
        [DefaultValue(false)]
        public bool RTLAGC { get; set; }

        [DisplayName("3. Tuner AGC")]
        [Description("Tuner AGC option is enabled.")]
        [DefaultValue(false)]
        public bool TunerAGC { get; set; }

        [DisplayName("4. Offset Tuning")]
        [Description("Offset Tuning option is enabled.")]
        [DefaultValue(false)]
        public bool OffsetTuning { get; set; }

        [DisplayName("5. VGA Gain Level")]
        [Description("Please select VGA gain level from -200 to 200. Value should be in tenths of a dB, -30 means -3.0 dB")]
        [DefaultValue(0)]
        public int VGAGain {
            get => _deviceVGAGain; set {
                if (value < -200 || value > 200)
                    value = 0;

                _deviceVGAGain = value;
            }
        }

        [DisplayName("6. LNA Gain Level")]
        [Description("Please select LNA gain level from -200 to 200. Value should be in tenths of a dB, -30 means -3.0 dB")]
        [DefaultValue(0)]
        public int LNAGain {
            get => _deviceLNAGain; set {
                if (value < -200 || value > 200)
                    value = 0;

                _deviceLNAGain = value;
            }
        }

        [DisplayName("7. Frequency correction, ppm")]
        [Description("Please select Frequency correction value from -1000 to 1000.")]
        [DefaultValue(0)]
        public int FreqCorrection {
            get => _deviceFreqCorrecton; set {
                if (value < -1000 || value > 1000)
                    value = 0;

                _deviceFreqCorrecton = value;
            }
        }

        [DisplayName("Last frequency")]
        [Description("Receiver's last used frequency")]
        [DefaultValue(105000000)]
        [Browsable(false)]
        public int LastFrequency { get; set; } = 105000000;
    }
}
