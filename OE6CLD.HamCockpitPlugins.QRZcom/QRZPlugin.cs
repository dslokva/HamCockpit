using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq; 
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Windows.Forms.VisualStyles;
using VE3NEA.HamCockpit.PluginAPI;

namespace OE6CLD.HamCockpitPlugins.QRZcom

    
{
    [Export(typeof(IPlugin))]
    [Export(typeof(IVisualPlugin))]

    public class QRZPlugin : IPlugin, IVisualPlugin
    {
        private QRZPanel MyPanel;
        private Settings settings = new Settings();

        public string Name => "QRZ Call Lookup";
        public string Author => "OE6CLD";
        public bool Enabled { get; set; }
        public ToolStrip ToolStrip => null;
        public ToolStripItem StatusItem => null;
        public object Settings { get => settings; set => ApplySettings(value); }

        // public object Settings { get => settings; set { } }

        public bool CanCreatePanel => MyPanel == null;

        public UserControl CreatePanel()
        {
            MyPanel = new QRZPanel();
            MyPanel.Name = GetMyTitleAndVersion();
            MyPanel.Plugin = this;
            MyPanel.BackColor = settings.BackColor;
           
            return MyPanel;
        }

        public void DestroyPanel(UserControl panel)
        {
            this.MyPanel = null;
        }


        private string GetMyTitleAndVersion()
        {
            // get the version number
            Assembly asm = Assembly.GetExecutingAssembly();
            AssemblyName an = asm.GetName();
            string version = an.Version.Major + "." + an.Version.Minor + "." + an.Version.Build + "." + an.Version.Revision;
            return "QRZ Call Lookup (v" + version + ")";
        }

        private void ApplySettings(object value)
        {
            settings = value as Settings;
            if (MyPanel != null)
            {
                MyPanel.BackColor = settings.BackColor;
            }
        }
 
    }
}
