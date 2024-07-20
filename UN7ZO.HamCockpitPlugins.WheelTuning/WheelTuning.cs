using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VE3NEA.HamCockpit.PluginAPI;

namespace UN7ZO.HamCockpitPlugins.WheelTuning {
    [Export(typeof(IPlugin))]

    class WheelTuning : IPlugin, IDisposable {
        internal readonly IPluginHost host;
        internal Settings settings = new Settings();
        private StepSizePanel stepSizePanel = new StepSizePanel();
        internal Int64 CurrentFrequency;
        private MouseWheelHook _wheelHook;

        [ImportingConstructor]
        WheelTuning([Import(typeof(IPluginHost))] IPluginHost host) {
            this.host = host;          
            stepSizePanel.plugin = this;

            _wheelHook = new MouseWheelHook();

            ToolStrip.AutoSize = false;
            ToolStrip.Items.Add(new ToolStripControlHost(stepSizePanel));
            ToolStrip.Height = 32;
            ToolStrip.Width = 135;
        }

        private void StatusChanged(object sender, EventArgs e) {
            if (this.Enabled)
                if (host.DspPipeline.Active) {
                    _wheelHook.SetUpHook();
                    _wheelHook.MouseWheelEvent += Control_MouseWheel;
                } else {
                    _wheelHook.MouseWheelEvent -= Control_MouseWheel;
                    _wheelHook.ClearHook();
                }
        }

        private void TunedEventhandler(object sender, EventArgs e) {
            CurrentFrequency = host.DspPipeline.Tuner.GetDialFrequency();
        }

        private void Control_MouseWheel(object sender, MouseEventArgs e) {
            if (!host.DspPipeline.Active) return;

            int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            int mouseTuneHzStep = (int)settings.MouseTuneHzStep;
            long freq = CurrentFrequency;
            bool needToStep = true;

            if (settings.MouseTuneRoundHz) {
                long clearFreq;

                if (mouseTuneHzStep < 1000)
                    clearFreq = freq / 1000 * 1000;
                else
                    clearFreq = freq / 10000 * 10000;

                var diffHz = freq - clearFreq;

                if (numberOfTextLinesToMove > 0) {
                    //positive - scroll forward
                    if (diffHz < mouseTuneHzStep) {
                        needToStep = false;
                        freq = clearFreq + mouseTuneHzStep;
                    }

                    if (diffHz > mouseTuneHzStep) {
                        var stepCounts = (diffHz / mouseTuneHzStep) + 1;
                        freq = clearFreq + (mouseTuneHzStep * stepCounts);
                        needToStep = false;
                    }
                } else {
                    //negative - scroll back
                    if (diffHz > mouseTuneHzStep) {
                        double stepsBackCount = (double)diffHz / (double)mouseTuneHzStep;
                        if (Math.Truncate(stepsBackCount) > 1)
                            stepsBackCount -= 1;
                        freq = (long)(clearFreq + (mouseTuneHzStep * Math.Truncate(stepsBackCount)));
                        needToStep = false;
                    }

                    if (diffHz < mouseTuneHzStep && diffHz != 0) {
                        freq = clearFreq;
                        needToStep = false;
                    }
                }
            }

            if (needToStep)
                if (numberOfTextLinesToMove > 0) {
                    freq += mouseTuneHzStep;  //positive - scroll forward
                } else {
                    freq -= mouseTuneHzStep; //negative - scroll back
                }

            host.DspPipeline.Tuner.SetDialFrequency(freq);
            
        }

        #region IPlugin
        public string Name => "Mouse Tune";
        public string Author => "UN7ZO";
        public bool Enabled { get; set; }
        public object Settings { get => settings; set => ApplySettings(value as Settings); }
        public ToolStrip ToolStrip { get; } = new ToolStrip();
        public ToolStripItem StatusItem => null;

        private void ApplySettings(Settings value) {
            settings = value;

            if (stepSizePanel != null) {
                stepSizePanel.RefreshStepSizeControl();
                stepSizePanel.Invalidate();
            }

            if (Enabled) {
                host.DspPipeline.Tuner.Tuned += TunedEventhandler;
                host.DspPipeline.StatusChanged += StatusChanged;
            } else {
                host.DspPipeline.Tuner.Tuned -= TunedEventhandler;
                host.DspPipeline.StatusChanged -= StatusChanged;
            }

        }
        #endregion

        public void Dispose() {
            _wheelHook.MouseWheelEvent -= Control_MouseWheel;
            stepSizePanel.Dispose();
            ToolStrip.Dispose();
        }
    }
}
