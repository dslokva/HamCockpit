using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace UN7ZO.HamCockpitPlugins.WheelTuning {
    public partial class StepSizePanel: UserControl {
        internal WheelTuning plugin;

        public StepSizePanel() {
            InitializeComponent();
            RefreshStepSizeControl();
        }

        public void RefreshStepSizeControl() {
            cboxStepSize.Items.Clear();

            foreach (var item in Enum.GetValues(typeof(WheelTuneStep))) {
                var dna = getDescriptionAttribute(item);

                if (dna != null)
                    cboxStepSize.Items.Add(dna.Description);
            }

            cboxStepSize.Items.RemoveAt(0);
            if (plugin != null) {
                stepSizeLabel.BorderStyle = plugin.settings.MouseTuneRoundHz ? BorderStyle.FixedSingle : BorderStyle.None;

                string selectedStep = getDescriptionAttribute(plugin.settings.MouseTuneHzStep).Description;
                cboxStepSize.SelectedIndex = cboxStepSize.Items.IndexOf(selectedStep);

            }
        }

        private static DescriptionAttribute getDescriptionAttribute(object item) {
            Type enumType = typeof(WheelTuneStep);
            FieldInfo fi = enumType.GetField(Enum.GetName(enumType, item));
            DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
            return dna;
        }

        private T getValueFromDescription<T>(string description) where T : Enum {
            foreach (var field in typeof(T).GetFields()) {
                if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute) {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
            }

            return default(T);
        }

        private void cboxStepSize_SelectedIndexChanged(object sender, EventArgs e) {
            plugin.settings.MouseTuneHzStep = getValueFromDescription<WheelTuneStep>(cboxStepSize.Text);
        }

        private void stepSizeLabel_Click(object sender, EventArgs e) {
            plugin.settings.MouseTuneRoundHz = !plugin.settings.MouseTuneRoundHz;
            stepSizeLabel.BorderStyle = plugin.settings.MouseTuneRoundHz ? BorderStyle.FixedSingle : BorderStyle.None;
        }
    }
}
