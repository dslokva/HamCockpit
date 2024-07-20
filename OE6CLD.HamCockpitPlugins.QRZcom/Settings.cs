using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;

namespace OE6CLD.HamCockpitPlugins.QRZcom
{
    // option to choose between kilometers and miles (for distance calculation)
    public enum MyUnits
    {
        Kilometers,
        Miles
    }
    class Settings
    {
        [DisplayName("Background Color")]
        [Description("The background color of the panel.")]
        [DefaultValue(typeof(Color), "Control")]
        public Color BackColor { get; set; } = SystemColors.Control;


        [DisplayName("QRZ Password")]
        [Description("Enter the QRZ password.")]

        public string QRZpassword { get; set; }


        [DisplayName("QRZ Username")]
        [Description("Enter the QRZ username.")]
        public string QRZusername { get; set; }


        [DisplayName("My Grid Square")]
        [Description("Enter own QRA (Maidenhead) locator.")]
        public string MyGrid { get; set; }


        [DisplayName("Display Units")]
        [Description("Unit calculation in kilometers (default) or miles.")]
        public MyUnits MyUnit { get; set; }

    }
}
