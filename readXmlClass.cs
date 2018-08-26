using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.Data;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace readConfigXml
{
     [DisplayName("Read Xml or Json")]
    public class readXmlClass : CodeActivity
    {
       

        public enum fileType
        {
            Xml, Json
        }

        [Category("Input")]
        [RequiredArgument]
        [DisplayName("File Type")]
        public fileType TypeOfFile { get; set; }

        [Category("Input")]
        [RequiredArgument]
        [DisplayName("File Path")]
        public InArgument<string> FilePath { get; set; }

        [Category("Output")]
        [DisplayName("Result Dictionary")]
        public OutArgument<Dictionary<String,String>> ResultDictionary { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string File_Path = FilePath.Get(context);

            //DataTable dt = new DataTable();
            //dt.Columns.Add(new DataColumn("Name", typeof(String)));
            //dt.Columns.Add(new DataColumn("Value", typeof(String)));

            Dictionary<String, String> dt = new Dictionary<String, String>();

            fileType file_Type = TypeOfFile;
            if (file_Type == fileType.Xml)
            {
                XDocument xDoc = XDocument.Load(File_Path);
                XContainer xContainer = xDoc;
                String fullNodeName = String.Empty;
                readXMLNodes(xContainer, ref dt, fullNodeName);
                ResultDictionary.Set(context, dt);
            }
            else
            {
                JToken objJToken;
                using (StreamReader file = File.OpenText(File_Path))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                     objJToken = JToken.ReadFrom(reader);
                  
                }
                readJsonNodes(objJToken, ref dt, String.Empty);
                ResultDictionary.Set(context, dt);
            }
        }

        protected void readXMLNodes(XContainer inXmlElement, ref Dictionary<String,String> dt, string fullNodeName)
        {
            if (inXmlElement.Elements().Count() > 0)
            {
                if (inXmlElement.Parent != null)
                {
                    if (string.IsNullOrEmpty(fullNodeName))
                        fullNodeName = ((XElement)inXmlElement).Name.ToString();
                    else
                        fullNodeName = fullNodeName + "_" + ((XElement)inXmlElement).Name;
                }

                foreach (XElement x in inXmlElement.Elements())
                {

                    readXMLNodes(x, ref dt, fullNodeName);
                }
            }
            else
            {
                XElement xElem = (XElement)inXmlElement;
                if (xElem != null)
                {
                    //DataRow dr = dt.NewRow();
                    //dr["Name"] = fullNodeName + "_" + xElem.Name;
                    //dr["Value"] = xElem.Value;
                    //dt.Rows.Add(dr);

                    if(dt.ContainsKey(fullNodeName + "_" + xElem.Name))
                    {
                        int i = 1;
                        
                        while(dt.ContainsKey(fullNodeName + "_" + xElem.Name+"_"+i.ToString()))
                        {
                            i += 1;
                        }

                        dt[fullNodeName + "_" + xElem.Name+"_"+i.ToString()] = xElem.Value;
                    }
                    else
                    {
                        dt[fullNodeName + "_" + xElem.Name] = xElem.Value;
                    }

                }
            }
        }

        protected void readJsonNodes(JToken objJtoken, ref Dictionary<String, String> dt, string fullNodeName)
        {
            JToken newJToken;
            if (objJtoken is JProperty)
            {
                newJToken = ((JProperty)objJtoken).Value;

                if (string.IsNullOrEmpty(fullNodeName))
                    fullNodeName = ((JProperty)objJtoken).Name.ToString();
                else
                    fullNodeName = fullNodeName + "_" + ((JProperty)objJtoken).Name.ToString();
            }
            else
            {
                newJToken = objJtoken;
            }


            if (newJToken is JArray)
            {
                JArray objJArray = (JArray)newJToken;
                foreach (JObject j in objJArray)
                {
                    readJsonNodes(j, ref dt, fullNodeName);
                }
            }
            else if (newJToken is JObject)
            {
                JObject objJson = (JObject)newJToken;
                foreach (JProperty j in objJson.Properties())
                {
                    readJsonNodes(j, ref dt, fullNodeName);
                }
            }
            else
            {
                JProperty jP = (JProperty)objJtoken;
                if (jP != null)
                {
                    //DataRow dr = dt.NewRow();
                    //dr["Name"] = fullNodeName;
                    //dr["Value"] = jP.Value;
                    //dt.Rows.Add(dr);

                    if (dt.ContainsKey(fullNodeName))
                    {
                        int i = 1;

                        while (dt.ContainsKey(fullNodeName + "_" + i.ToString()))
                        {
                            i += 1;
                        }

                        dt[fullNodeName + "_" + i.ToString()] = jP.Value.ToString();
                    }
                    else
                    {
                        dt[fullNodeName] = jP.Value.ToString();
                    }
                }
            }
        }
    }
}
