using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using VMGuide.FileFormat;

namespace VMGuide.VirtualMachine
{
    [Description("VirtualBox")]
    [FileExtension(".vbox")]
    public class VirtualBoxVirtualMachine : IVirtualMachine, INotifyPropertyChanged
    {
        public string Path { get; private set; }
        public string Name { get; private set; }
        VirtualBoxFile xml;

        public VirtualBoxVirtualMachine(string path)
        {
            if (!File.Exists(path))
                throw new VirtualMachineNotFoundException(path);

            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
        }
        
        private const string XPATH_ACPI = "VirtualBox/Machine/Hardware/BIOS/ACPI";
        private const string XPATH_FIRMWARE = "VirtualBox/Machine/Hardware/Firmware";
        private const string XPATH_TIME_OFFSET = "VirtualBox/Machine/Hardware/BIOS/TimeOffset";
        private const string XPATH_SOUND = "VirtualBox/Machine/Hardware/AudioAdapter";

        public event PropertyChangedEventHandler PropertyChanged;

        public void Load() {
            xml = new VirtualBoxFile(Path);
            Name = ((XmlHelper)xml).GetAttribute(@"VirtualBox/Machine", "name", Name);
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")  
        {  
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        [Description("BIOS Date Lock")]
        [FormProperty]
        public bool DateLock
        { 
            get => timeOffset != 0;
            set 
            {
                if (value) {
                    // 1ms offset, just to indicate there is one
                    if (timeOffset == 0) timeOffset = 1;
                } else {
                    timeOffset = 0;
                }
            }
        }

        [Description("BIOS Date")]
        [FormProperty]
        public DateTime BIOSDate
        {
            get => DateTime.Now.Date.AddMilliseconds(timeOffset);
            set
            {
                timeOffset = (long)((value.Date - DateTime.Now.Date).TotalMilliseconds);
            }
        }

        [Description("ACPI Support")]
        [FormProperty]
        public bool ACPI
        { 
            get => xml.GetAttribute(XPATH_ACPI, "enabled", true);
            set 
            {
                if (value == ACPI) return;
                if (IsLocked) throw new VirtualMachineLockedException();

                xml.SetAttribute(XPATH_ACPI, "enabled", value);
                xml.Save();

                NotifyPropertyChanged();
            }
        }
        
        [Description("Firmware")]
        [FormProperty]
        public VirtualBoxFirmware Firmware
        {
            get => xml.GetAttribute(XPATH_FIRMWARE, "type", VirtualBoxFirmware.BIOS);
            set
            {
                if (value == Firmware) return;
                if (IsLocked) throw new VirtualMachineLockedException();

                xml.SetAttribute(XPATH_FIRMWARE, "type", value);
                xml.Save();
                NotifyPropertyChanged();
            } 
        }

        [Description("Sound Card")]
        [FormProperty]
        public VirtualBoxSound Sound 
        {
            get
            {
                var sound = xml.GetAttribute(XPATH_SOUND, "controller", VirtualBoxSound.AC97);
                var enabled = xml.GetAttribute(XPATH_SOUND, "enabled", false);
                return enabled ? sound : VirtualBoxSound.NONE;
            }
            set
            {
                if (value == Sound) return;
                if (IsLocked) throw new VirtualMachineLockedException();

                var enabled = (value != VirtualBoxSound.NONE);
                xml.SetAttribute(XPATH_SOUND, "enabled", enabled);
                if (enabled) xml.SetAttribute(XPATH_SOUND, "controller", value);
                xml.Save();
                NotifyPropertyChanged();
            }
        }

        private long timeOffset 
        {
            get => xml.GetAttribute(XPATH_TIME_OFFSET, "value", 0L);
            set 
            {
                if (value == timeOffset) return;
                if (IsLocked) throw new VirtualMachineLockedException();

                xml.SetAttribute(XPATH_TIME_OFFSET, "value", value);
                xml.Save();

                if (value == 0)
                    NotifyPropertyChanged("BIOSDate");
                else
                    NotifyPropertyChanged("DateLock");
            }
        }

        public bool IsLocked => Process.GetProcessesByName("VboxSVC").Length > 0;
    }

    public enum VirtualBoxFirmware {
        [Description("BIOS")]
        BIOS,
        [Description("UEFI")]
        UEFI
    }

    public enum VirtualBoxSound {
        [Description("SoundBlaster 16")]
        SB16,
        [Description("Intel HD Audio")]
        HDA,
        [Description("ICH AC97")]
        AC97,
        [Description("None")]
        NONE
    }
}
