using HiEndsCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiEndsCore.Models;
using HiEndsCore.Helper;
using System.Data;
using System.Xml.XPath;
using System.Xml;

namespace HiEndsExtractor.DataDriver
{
    public class XmlDataExtractor: IDataExtractor
    {
        public string Name => "Xml";
        public T ExtractData<T>(string sourcePath, ExtractionTemplate template)
        {
            // Load the document and set the root element.  
            XmlDocument doc = new XmlDocument();
            doc.Load(sourcePath);
            XmlNode root = doc.DocumentElement;

            //// Add the namespace.  
            //XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            //nsmgr.AddNamespace("bk", "urn:newbooks-schema");

            // Select and display the first node in which the author's
            // last name is Kingsolver.  
            XmlNode node = root.SelectSingleNode("");

            Console.WriteLine(node.InnerXml);
            XmlReader xmlReader = new XmlNodeReader(node);
            DataSet dataSet = new DataSet();
            dataSet.ReadXml(xmlReader);
            
            return (T)(object)dataSet.Tables[0];
        }

        public T TransformData<T>(T data, ExtractionTemplate template, string filter)
        {
            throw new NotImplementedException();
        }

        public T MergeToExistingData<T>(T data, string existingData, DataType type)
        {
            throw new NotImplementedException();
        }

        public string LoadDataAsString<T>(T data, DataType type)
        {
            throw new NotImplementedException();
        }
    }
}
