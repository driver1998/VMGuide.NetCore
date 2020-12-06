using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace VMGuide
{

    [Description("Unknown")]
    [FileExtension("")]
    public interface IVirtualMachine
    {
        string Path { get; }
        string Name { get; }
        Boolean IsLocked { get; }

        void Load();
    }
    
    public static class Core
    {
        public static IEnumerable<IVirtualMachine> SearchVirtualMachine()
        {
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetTypes()
                .Where(p => p.Namespace == "VMGuide.Searcher" && p.Name.EndsWith("Searcher"))
                .Select(t => t.GetMethod("SearchVirtualMachine"))
                .SelectMany(m => (IEnumerable<IVirtualMachine>)m.Invoke(null, null))
                .ToList();
        }

        public static IEnumerable<string> GetSupportedFileExtensions() {
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetTypes()
                .Where(p => p.GetInterface("IVirtualMachine") != null)
                .Select(p => p.GetCustomAttribute<FileExtensionAttribute>())
                .Where(p => p != null)
                .Select(p => p?.Extension);
        }

        public static IVirtualMachine OpenVirtualMachine(string path) {
            if (!File.Exists(path))
                throw new VirtualMachineNotFoundException(path);

            var asm = Assembly.GetExecutingAssembly();
            var ext = Path.GetExtension(path);

            var supportedType = asm.GetTypes()
                .Where(p => p.GetInterface("IVirtualMachine") != null)
                .Where(t => {
                    var attr = t.GetCustomAttribute<FileExtensionAttribute>();
                    return attr?.Extension == ext;
                })
                .FirstOrDefault();

            if (supportedType == null) throw new HypervisorNotSupportedException();
            return (IVirtualMachine)Activator.CreateInstance(supportedType, new[] { path });
        }
    }
    
    public class VirtualMachineLockedException : Exception {
        public VirtualMachineLockedException():
            base("This virtual machine is locked by a running hypervisor.") {}
    }

    public class VirtualMachineNotFoundException : Exception {
        public VirtualMachineNotFoundException(string path):
            base($"Virtual Machine not found.\nPath: {path}") {}
    }
    
    public class HypervisorNotSupportedException : Exception {
        public HypervisorNotSupportedException():
            base("This hypervisor is not supported.") {}
    }

    public class FileExtensionAttribute : Attribute {
        public string Extension { get; private set; }
        public FileExtensionAttribute(string ext) {
            Extension = ext;
        }
    }

}
