using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using SharpRTL.Types;
using VE3NEA.HamCockpit.DspFun;
using VE3NEA.HamCockpit.PluginAPI;

namespace UN7ZO.HamCockpitPlugins.RTL_SDR {
    [Export(typeof(IPlugin))]
    [Export(typeof(ISignalSource))]
    class RTLSDR: IPlugin, ISignalSource {

        private Settings settings = new Settings();
        private readonly RingBuffer buffer;
        private RtlDevice device;
        private bool started;
        private readonly SynchronizationContext context = SynchronizationContext.Current;
        //private byte[] accumBuffer;
        //private int bufAccumCount = 0;

        public RTLSDR() {
            // Initialize the Manager instance.
            try {
                Debug.WriteLine("RTLSDR Constuctor called");
                started = false;

                int rate = (int)settings.SamplingRate;
                Console.WriteLine("RTLSDR settings called. SampleRate = " + rate);

                buffer = new RingBuffer(rate);
                Format = new SignalFormat(rate, true, false, 1, -(int)(rate * 0.47), (int)(rate * 0.47), 0);
                //accumBuffer = new byte[16384];

                EventHandler<SamplesAvailableEventArgs> p = (o, e) => SamplesAvailable?.Invoke(this, e);
                buffer.SamplesAvailable += p;

            } catch (Exception ex) {
                Debug.WriteLine("Error: "+ ex.Message);                
            }
        }

        #region IPlugin
        public string Name => "RTL-SDR";
        public string Author => "UN7ZO";
        public bool Enabled { get; set; }
        public object Settings { get => settings; set => ApplySettings(value as Settings); }
        private void ApplySettings(Settings newSettings) { settings = newSettings; }
        public ToolStrip ToolStrip => null;
        public ToolStripItem StatusItem => null;
        #endregion

        #region ISignalSource 
        public void Initialize() {
            device = new RtlDevice((uint) settings.SelectedDeviceIndex);
            device.SamplesAvailable += Device_SamplesAvailable;
        }

        public bool Active { get => IsActive(); set => SetActive(value); }

        private bool IsActive() {
            Console.WriteLine("IsActive called. Returned value: "+ started);
            return started;
        }

        public event EventHandler<StoppedEventArgs> Stopped;

        private void SetActive(bool value) {
            if (value == Active) return;

            if (value) {
               
               if (device != null) { 
                  device.SampleRate = (uint)settings.SamplingRate;
                  device.Frequency = (uint)settings.LastFrequency;
                  Debug.WriteLine("Frequency to set: " + settings.LastFrequency);
                  
                  device.UseTunerAGC = settings.TunerAGC;
                  device.UseRtlAGC = settings.RTLAGC;
                  device.LNAGain = settings.LNAGain;
                  device.VGAGain = settings.VGAGain;
                  device.FrequencyCorrection = settings.FreqCorrection;
                  device.UseOffsetTuning = settings.OffsetTuning;
                  
                  device.Start();

                  Tuned?.Invoke(this, new EventArgs());
                  started = true;
               }

            } else {
                started = false;
                StopRTLDevice();
            }
        }

        #endregion

        #region ISampleStream
        public SignalFormat Format { get; private set; }

        public int Read(float[] buffer, int offset, int count) {
            Console.WriteLine("Read called. Wanted count = "+ count);
            return this.buffer.Read(buffer, offset, count); 
        }
        public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

        private void Device_SamplesAvailable(ref byte[] data, int length) {
            try {
                buffer.WriteInt8(data, length);

                //for (var i = 0; i < length-1; i++) {
                //    accumBuffer[bufAccumCount] = data[i];
                //    bufAccumCount++;
                //}
                //bufAccumCount++;
                ////try to accumulate 10 portion of samples from RTL device
                //if (bufAccumCount == 163840) {
                //    buffer.WriteInt8(accumBuffer, accumBuffer.Length);
                //    bufAccumCount = 0;
                //    Array.Clear(accumBuffer, 0, accumBuffer.Length);
                //}
            } catch (Exception e) {
                StopRTLDevice();
                var exception = new Exception($"Unable to read I/Q data from SDR:\n\n{e.Message}");
                context.Post(s => Stopped?.Invoke(this, new StoppedEventArgs(exception)), null);
            }
        }

        private void StopRTLDevice() {
            if (device != null) {
                device.Stop();
                device.SamplesAvailable -= Device_SamplesAvailable;
                device.Dispose();
            }
        }

        #endregion

        #region ITuner
        public long GetDialFrequency(int channel = 0) {
            return device.Frequency;
        }

        public void SetDialFrequency(long frequency, int channel = 0)
        {
            device.Frequency = (uint)frequency;
            Tuned?.Invoke(this, new EventArgs());
            settings.LastFrequency = (int) frequency;
        }
        public event EventHandler Tuned;
        #endregion


    }
}
