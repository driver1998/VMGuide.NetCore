using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using VMGuide.Utils;
using VMGuide.VirtualMachine;
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
    
    public class ObservableList<T> : ObservableCollection<T> {
        public ObservableList() : base() {}
        public ObservableList(IEnumerable<T> collection) : base(collection) {}
        public ObservableList(IList<T> list) : base(list) {}

        public void AddRange(IEnumerable<T> range) {
            CheckReentrancy();

            foreach(var item in range) Items.Add(item);

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T>(range)));
        }
    }

    public class NotifyChanged<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NotifyChanged() { _value = default(T); }
        public NotifyChanged(T defaultValue) { _value = defaultValue; }

        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (!value.Equals(_value)) {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        public override string ToString() => _value.ToString();
    }

    public class VirtualMachineLockedException: Exception {
        public VirtualMachineLockedException():
            base("This virtual machine is locked by a running hypervisor.") {}
    }

    public class VirtualMachineNotFoundException: Exception {
        public VirtualMachineNotFoundException(string path):
            base($"Virtual Machine not found.\nPath: {path}") {}
    }
    
    public class HypervisorNotSupportedException: Exception {
        public HypervisorNotSupportedException():
            base("This hypervisor is not supported.") {}
    }

    public class FileExtensionAttribute: Attribute {
        public string Extension { get; private set; }
        public FileExtensionAttribute(string ext) {
            Extension = ext;
        }
    }

    public static class Core
    {
        public static IEnumerable<IVirtualMachine> SearchVirtualMachine()
        {
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetTypes()
                .Where(p => p.Namespace == "VMGuide.Utils" && p.Name.EndsWith("Utils"))
                .Select(t => t.GetMethod("SearchVirtualMachine"))
                .SelectMany(m => (IEnumerable<IVirtualMachine>)m.Invoke(null, null))
                .ToList();
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

        public static string GetTypeDescription(this object o) {
            var type = o.GetType();
            var attr = type.GetCustomAttribute<DescriptionAttribute>();
            return (attr?.Description) ?? type.Name;
        }

        public static IEnumerable<string> GetSupportedFileExtensions() {
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetTypes()
                .Where(p => p.GetInterface("IVirtualMachine") != null)
                .Select(p => p.GetCustomAttribute<FileExtensionAttribute>())
                .Where(p => p != null)
                .Select(p => p?.Extension);
        }
    }
}
