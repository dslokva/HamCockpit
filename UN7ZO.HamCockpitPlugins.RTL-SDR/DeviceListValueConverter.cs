using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpRTL;

namespace UN7ZO.HamCockpitPlugins.RTL_SDR {
    public struct DeviceValueEntry {
        public int Id;
        public string Name;
        public DeviceValueEntry(int id, string name) { Id = id; Name = name; }
    }

    /// <exclude />
    public class DeviceListValueConverter : StringConverter {
        internal DeviceValueEntry[] valuesTable = null;
        
        /// <exclude />
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }

        /// <exclude />
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            if (valuesTable == null || valuesTable.Length == 0)
                ListAvailableDevices();
            return new StandardValuesCollection(valuesTable.Select(s => s.Id).ToArray());
        }

        /// <exclude />
        protected void ListAvailableDevices() {
            valuesTable = new DeviceValueEntry[0];

            uint deviceCount = Native.rtlsdr_get_device_count();
            if (deviceCount == 0) {
                Debug.WriteLine("No Available Devices");
            } else {
                valuesTable = new DeviceValueEntry[deviceCount];
            }

            for (int i = 0; i < deviceCount; i++) {
                string devName = Native.rtlsdr_get_device_name((uint) i);
                Debug.WriteLine("Device({0}): {1}", i, devName);
                valuesTable[i] = new DeviceValueEntry(i, devName + " ("+i+")");
            }

        }

        /// <exclude />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (valuesTable == null || valuesTable.Length == 0) ListAvailableDevices();
            return valuesTable.Where(s => s.Name == value as string)?.Select(s => s.Id)?.First();
        }

        /// <exclude />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (valuesTable == null || valuesTable.Length == 0) ListAvailableDevices();

            try {
                return valuesTable.Where(s => s.Id == (int)value).Select(s => s.Name).First();
            } catch {
                return "<please select>";
            }
        }
    }
}
