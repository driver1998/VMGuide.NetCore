using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using VMGuide.VirtualMachine;
using VMGuide.FileFormat;

namespace VMGuide.Searcher
{
    public static class VirtualBoxSearcher
    {
        public static IEnumerable<IVirtualMachine> SearchVirtualMachine()
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string configFile;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                configFile = Path.Combine(userProfile, ".VirtualBox/VirtualBox.xml");
            } else {
                configFile = Path.Combine(userProfile, ".config/VirtualBox/VirtualBox.xml");
            }
            
            if (File.Exists(configFile)) {
                try 
                {
                    return new VirtualBoxFile(configFile)
                        .GetAttributes("VirtualBox/Global/MachineRegistry/MachineEntry", "src")
                        .Where(p => File.Exists(p) && Path.GetExtension(p) == ".vbox")
                        .Select(p => new VirtualBoxVirtualMachine(p));
                } catch (XmlException ex) {
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLine(ex.StackTrace);
                }
            }
            return new List<IVirtualMachine>();
        }
    }
}