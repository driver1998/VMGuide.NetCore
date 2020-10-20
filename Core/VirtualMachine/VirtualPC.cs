using System;
using System.IO;
using VMGuide.FileFormat;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VMGuide.VirtualMachine
{
    [Description("Virtual PC")]
    [FileExtension(".vmc")]
    public class VirtualPCVirtualMachine : IVirtualMachine, INotifyPropertyChanged
    {
        public string Path { get; private set; }
        public string Name { get; private set; }

        private const string DefaultCMOS = 
            "000040500025378002FFFF000000000000000000000000000030304C07070707" +
            "0434FFFF2085807F00000000700801800D880000000000000000000000000090" +
            "1A32E252580050E999E62401002784004A2080244000000000085AACFE103254" +
            "7698BAE400000000000003000000000000000000000000000000000000000000" +
            "0000000000000000000000000000000000000000000000000000000000000000" +
            "0000000000000000000000000000000000000000000000000000000000000000" +
            "0000000000000000000000000000000000000000000000000000000000000000" +
            "000000000000000000000000000000000000";

        private VirtualPCFile xml;

        public VirtualPCVirtualMachine(string path)
        {
            if (!File.Exists(path)) throw new VirtualMachineNotFoundException(path);
            
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
        }
        
        const string XPATH_CMOS = "preferences/hardware/bios/cmos";
        const string XPATH_TIME_BYTES = "preferences/hardware/bios/time_bytes";
        const string XPATH_CREATOR = "preferences/properties/creator/build";
        const string XPATH_TIME_SYNC = "preferences/integration/microsoft/components/host_time_sync/enabled";
        const string XPATH_TIME_SYNC_V7 = "preferences/hardware/bios/time_sync_at_boot";

        public event PropertyChangedEventHandler PropertyChanged;

        public void Load() {
            if (IsLocked) throw new VirtualMachineLockedException();
            xml = new VirtualPCFile(Path);
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")  
        {  
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }  

        [Description("BIOS Date Lock")]
        public bool DateLock
        {
            get => !timeSync;
            set
            {
                if (value == DateLock) return;
                timeSync = !value;
                if (timeSync) date = DateTime.Now;
            }
        }

        [Description("BIOS Date")]
        public DateTime BIOSDate
        { 
            get => date;
            set 
            {
                if (value.Date == BIOSDate) return;
                timeSync = false;
                date = value;
            }
        }

        private bool timeSync
        {
            get => xml.GetValue(IsVersion7? XPATH_TIME_SYNC_V7 : XPATH_TIME_SYNC, true);
            set
            {
                if (value == timeSync) return;
                if (IsLocked) throw new VirtualMachineLockedException();
                
                xml.SetValue(IsVersion7? XPATH_TIME_SYNC_V7 : XPATH_TIME_SYNC, timeSync);
                xml.Save();

                NotifyPropertyChanged("DateLock");
            }
        }

        private DateTime date
        {
            get
            {
                var cmos = xml.GetValue(XPATH_CMOS, "");
                var timeBytes = xml.GetValue(XPATH_TIME_BYTES, "");

                var yearString  = cmos.Substring(72, 2) + timeBytes.Substring (18, 2);
                var monthString = timeBytes.Substring(16, 2);
                var dayString   = timeBytes.Substring(14, 2);

                DateTime value;

                try {
                    var year  = int.Parse(yearString);
                    var month = int.Parse(monthString);
                    var day   = int.Parse(dayString);
                    value = new DateTime(year, month, day);
                } 
                catch (FormatException) {
                    value = DateTime.Now.Date;
                }

                return value;
            }
            set
            {
                if (value == date) return;
                if (IsLocked) throw new VirtualMachineLockedException();

                value = value.Date + DateTime.Now.TimeOfDay;
                var cmos = xml.GetValue(XPATH_CMOS, DefaultCMOS);

                var yearString    = value.Year  .ToString().PadLeft(2, '0');
                var monthString   = value.Month .ToString().PadLeft(2, '0');
                var dayString     = value.Day   .ToString().PadLeft(2, '0');
                var hourString    = value.Hour  .ToString().PadLeft(2, '0');
                var minuteString  = value.Minute.ToString().PadLeft(2, '0');
                var secondString  = value.Second.ToString().PadLeft(2, '0');

                string timeBytes =
                    secondString + "00"   +
                    minuteString + "00"   +
                    hourString   + "0000" +
                    dayString    +
                    monthString  +
                    yearString.Substring(2, 2);

                cmos = cmos.Substring(0, 72) + yearString.Substring(0, 2) + cmos.Substring(74);

                xml.SetValue(XPATH_CMOS, "bytes", cmos);
                xml.SetValue(XPATH_TIME_BYTES, "bytes", timeBytes);
                xml.Save();

                NotifyPropertyChanged("BIOSDate");
            }
        }

        // Detect Windows Virtual PC for Windows 7
        public bool IsVersion7 => xml.GetValue(XPATH_CREATOR, "").StartsWith("6.1");
        public bool IsLocked => false;

    }
}
