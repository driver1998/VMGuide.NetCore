using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using VMGuide.VirtualMachine;
using VMGuide.FileFormat;
using LnkParser;

namespace VMGuide.Utils
{
    public class VirtualPCUtils
    {
        // Virtual PC 2004/2007
        private static IEnumerable<IVirtualMachine> SearchVirtualMachineV6()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appdata, "Microsoft/Virtual PC/Virtual Machines");

            if (Directory.Exists(folder))
            {
                return Directory.GetFiles(folder, "*.lnk")
                    .Select(p => new LnkFile(p).TargetLocation)
                    .Where(p => File.Exists(p) && Path.GetExtension(p) == ".vmc")
                    .Select(p => new VirtualPCVirtualMachine(p))
                    .ToList();
            }
            return new List<IVirtualMachine>();
        }

        // Windows Virtual PC for Windows 7
        private static IEnumerable<IVirtualMachine> SearchVirtualMachineV7()
        {
            var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var folder = Path.Combine(profile, "Virtual Machines");
            if (Directory.Exists(folder))
            {
                try 
                {
                    return Directory.GetFiles(folder, "*.vmcx")
                        .Select(vmcx => new VirtualPCFile(vmcx).GetValue(@"vm_description/vmc_path", ""))
                        .Where(p => File.Exists(p) && Path.GetExtension(p) == ".vmc")
                        .Select(vmc => new VirtualPCVirtualMachine(vmc))
                        .ToList();
                } catch (XmlException ex) {
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLine(ex.StackTrace);
                }
            }
            return new List<IVirtualMachine>();
        }
        
        public static IEnumerable<IVirtualMachine> SearchVirtualMachine()
        {
            var v6 = SearchVirtualMachineV6();
            var v7 = SearchVirtualMachineV7();
            return v6.Concat(v7);
        }
    }
}