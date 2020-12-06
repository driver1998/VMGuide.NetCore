using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using VMGuide.FileFormat;

namespace VMGuide.VirtualMachine
{ 
    [Description("VMware")]
    [FileExtension(".vmx")]
    public class VMwareVirtualMachine : IVirtualMachine, INotifyPropertyChanged
    { 
        private VMwareFile vmx;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Path { get; private set; }
        public string Name { get; private set; }

        public VMwareVirtualMachine(string path)
        {
            if (!File.Exists(path)) throw new VirtualMachineNotFoundException(path);

            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")  
        {  
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }  

        public void Load() {
            vmx = new VMwareFile(Path);

            NetAdapter = new ObservableList<NotifyChanged<VMwareNetAdapter>>();
            
            PropertyChangedEventHandler handler = (s, e) => {
                Console.WriteLine($"{s} PropertyChanged");
                Save();
            };

            NetAdapter.AddRange(
                vmx.GetMatchedValues("ethernet\\d+.present", false)
                    .Where(p => p.Value == true)
                    .Select(p => {
                        var key = p.Key.Replace(".present", ".virtualDev");
                        var value = vmx.GetValue(key, VMwareNetAdapter.vmxnet);

                        var notify = new NotifyChanged<VMwareNetAdapter>(value);
                        notify.PropertyChanged += handler;

                        return notify;
                    })
            );

            NetAdapter.CollectionChanged += (s, e) => {
                Console.WriteLine($"{s} CollectionChanged");
                Save();
            };
        }

        public void Save() {
            if (IsLocked) throw new VirtualMachineLockedException();

            // NetAdapter
            var current = vmx.GetMatchedValues("ethernet\\d+.present", false);
            foreach (var p in current) {
                vmx.SetValue(p.Key, false);
            }
            for (int i=0; i<NetAdapter.Count; i++) {
                vmx.SetValue($"ethernet{i}.present", true);
                vmx.SetValue($"ethernet{i}.virtualDev", NetAdapter[i].ToString());
            }

            vmx.Save();
        }

        [Description("BIOS Date Lock")]
        [FormProperty]
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
        [FormProperty]
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

        [Description("ACPI Support")]
        [FormProperty]
        public bool ACPI
        {
            get => vmx.GetValue("acpi.present", true);
            set
            {
                if (value == ACPI) return;
                if (IsLocked) throw new VirtualMachineLockedException();

                vmx.SetValue("acpi.present", value);
                vmx.Save();

                NotifyPropertyChanged();
            } 
        }

        [Description("Firmware")]
        [FormProperty]
        public VMwareFirmware Firmware
        {
            get => vmx.GetValue("firmware", VMwareFirmware.bios);
            set
            {
                if (value == Firmware) return;
                if (IsLocked) throw new VirtualMachineLockedException();

                vmx.SetValue("firmware", value);
                vmx.Save();

                NotifyPropertyChanged();
            }
        }

        [Description("Sound Card")]
        [FormProperty]
        public VMwareSound Sound
        {
            get
            {
                var sound = vmx.GetValue("sound.virtualDev", VMwareSound.es1371);
                var enabled = vmx.GetValue("sound.present", false);
                return enabled ? sound : VMwareSound.none;
            }
            set
            {
                if (value == Sound) return;
                if (IsLocked) throw new VirtualMachineLockedException();

                var enabled = (value != VMwareSound.none);
                vmx.SetValue("sound.present", enabled);
                if (enabled) vmx.SetValue("sound.virtualDev", value);
                vmx.Save();

                NotifyPropertyChanged();
            }
        }
    
        [Description("Network Adapters")]
        [FormProperty]
        public ObservableList<NotifyChanged<VMwareNetAdapter>> NetAdapter { get; private set; }

        private bool timeSync
        {
            get => vmx.GetValue("time.synchronize.continue", true);
            set 
            {
                if (IsLocked) throw new VirtualMachineLockedException();

                vmx.SetValue("tools.syncTime", value);
                vmx.SetValue("time.synchronize.continue", value);
                vmx.SetValue("time.synchronize.restore", value);
                vmx.SetValue("time.synchronize.resume.disk", value);
                vmx.SetValue("time.synchronize.shrink", value);
                vmx.Save();

                NotifyPropertyChanged("DateLock");
            }
        }

        private DateTime date
        {
            get => vmx.GetValue("rtc.starttime", DateTime.Now).Date;
            set
            {
                if (IsLocked) throw new VirtualMachineLockedException();

                if (timeSync)
                    vmx.RemoveValue("rtc.starttime");
                else
                    vmx.SetValue("rtc.starttime", value.Date + DateTime.Now.TimeOfDay);
                vmx.Save();

                NotifyPropertyChanged("BIOSDate");
            }
        }

        public bool IsLocked => Directory.Exists(Path + ".lck");
    }

    
    public enum VMwareFirmware {
        [Description("BIOS")]
        bios,
        [Description("UEFI")]
        uefi
    }
    public enum VMwareSound {
        [Description("Sound Blaster PCI ES1371")]
        es1371,
        [Description("Sound Blaster 16")]
        sb16,
        [Description("HD Audio")]
        hdaudio,
        [Description("None")]
        none
    }
    public enum VMwareNetAdapter {
        [Description("AMD PCnet-PCI II Am79C970A")]
        vlance,
        [Description("Intel E1000")]
        e1000,
        [Description("Intel E1000e")]
        e1000e,
        [Description("VMware VMXNet")]
        vmxnet,
        [Description("VMware VMXNet3")]
        vmxnet3
    }        
}
