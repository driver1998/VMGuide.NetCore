namespace VMGuide.FileFormat
{
    public class VirtualPCFile: XmlHelper
    {
        public VirtualPCFile(string path): base(path) {}
        
        public bool GetValue(string xpath, bool defaultValue) {
            var str = base.GetValue(xpath, null);
            if (!bool.TryParse(str, out bool value)) return defaultValue;
            else return value;
        }

        public void SetValue(string xpath, string type, string value)
        {
            base.SetValue(xpath, value);
            base.SetAttribute(xpath, "type", type); 
        }
        public override void SetValue(string xpath, string value)
            => SetValue(xpath, "string", value);
        public void SetValue(string xpath, bool value)
            => SetValue(xpath, "boolean", value.ToString().ToLower());

    }
}