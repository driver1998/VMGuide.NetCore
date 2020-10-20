using System.Collections.Generic;
using System.IO;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml;

namespace VMGuide.FileFormat
{
    public class XmlHelper
    {
        public string NamespaceURI { get; private set; }
        public string Path { get; private set; }

        protected XmlDocument xml;
        protected XmlNamespaceManager nsmgr;

        public XmlHelper (string path)
        {
            xml = new XmlDocument();
            xml.Load(path);
            nsmgr = new XmlNamespaceManager(xml.NameTable);
            
            Path = path;
            NamespaceURI = null;
        }

        public XmlHelper(string path, string xmlns) {
            xml = new XmlDocument();
            xml.Load(path);
            nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("v", xmlns);

            Path = path;
            NamespaceURI = xmlns;
        }

        // add namespace specifier to xpath that without
        protected string XPathAddNamespace(string xpath)
        {
            if (NamespaceURI == null) return xpath;
            if (xpath == null || xpath.StartsWith("v:")) return xpath;
            xpath = xpath.Replace("/", "/v:");
            xpath = "v:" + xpath;
            return xpath;
        }

        public virtual string GetValue(string xpath, string defaultValue)
        {
            XmlNode node = xml.SelectSingleNode(XPathAddNamespace(xpath), nsmgr);
            var value = node?.InnerText;
            if (String.IsNullOrEmpty(value)) return defaultValue;
            else return value;
        }

        public virtual string GetAttribute(string xpath, string name, string defaultValue)
        {
            XmlNode node = xml.SelectSingleNode(XPathAddNamespace(xpath), nsmgr);
            var value = node?.Attributes[name]?.Value;
            if (String.IsNullOrEmpty(value)) return defaultValue;
            else return value;
        }

        public virtual List<string> GetAttributes(string xpath, string name)
        {
            XmlNodeList nodeList;
            var list = new List<string>();
            xpath = XPathAddNamespace(xpath);

            nodeList = xml.SelectNodes(xpath, nsmgr);
            foreach (XmlNode node in nodeList)
                list.Add(node.Attributes[name]?.Value);
            
            return list;
        }

        public virtual void SetValue(string xpath, string value) {
            xpath = XPathAddNamespace(xpath);
            var node = xml.SelectSingleNode(xpath, nsmgr);
            if (node == null)
                node = CreateTree(xpath);
            node.InnerText = value;
        }

        public virtual void SetAttribute(string xpath, string name, string value)
        {
            xpath = XPathAddNamespace(xpath);

            var node = xml.SelectSingleNode(xpath, nsmgr);
            if (node == null)
                node = CreateTree(xpath);
            if (node.Attributes[name] == null)
                node.Attributes.Append(xml.CreateAttribute(name));

            node.Attributes[name].Value = value;
        }

        // create the full tree based on the provided xpath
        protected XmlNode CreateTree (string XPath)
        {
            var elements = XPathAddNamespace(XPath).Split('/');

            XmlNode parent = xml; 
            foreach (string element in elements)
            {
                var node = parent.SelectSingleNode(element, nsmgr);
                if (node == null)
                {
                    // CreateNode needs a name specified without namespace
                    string name;
                    if (element.StartsWith("v:")) 
                        name = element.Remove(0, 2);
                    else
                        name = element;

                    var child = xml.CreateNode(XmlNodeType.Element, name, NamespaceURI);
                    parent.AppendChild(child);
                    parent = parent.SelectSingleNode(element, nsmgr);
                }
                else
                    parent = node;
            }
            return parent;
        }
        
        public void Save() => xml.Save(Path);
    }

}
