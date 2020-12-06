using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VMGuide.VirtualMachine;
using VMGuide.FileFormat;

namespace VMGuide.Searcher
{
    
    public static class VMwareSearcher
    {
        public static IEnumerable<IVirtualMachine> SearchVirtualMachine()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            
            var sku = new[] {
                // VMware Workstation
                new { filename = "VMware/inventory.vmls",  itemKey = @"index\d+.id" },
                // VMware Player
                new { filename = "VMware/preferences.ini", itemKey = @"pref.mruVM\d+.filename" }
            };

            var files = sku.SelectMany(s => {
                var configFile = Path.Combine(appdata, s.filename);
                if (!File.Exists(configFile)) return new List<string>();
                
                return new VMwareFile(configFile)
                    .GetMatchedValues(s.itemKey)
                    .Select(p => p.Value)
                    .Where(p => File.Exists(p) && Path.GetExtension(p) == ".vmx");
            });

            return files.Distinct().Select(p => new VMwareVirtualMachine(p));
        }
    }
}