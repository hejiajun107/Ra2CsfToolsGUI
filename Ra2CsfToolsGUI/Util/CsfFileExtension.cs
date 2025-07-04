﻿using IniParser.Model;
using SadPencil.Ra2CsfFile;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Ra2CsfToolsGUI.Util
{
    public static class CsfFileExtension
    {
        public static void WriteYamlFile(this CsfFile csf, Stream stream)
        {
            if (csf == null)
            {
                throw new ArgumentNullException("csf");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var dic = new Dictionary<string, string>();
            dic.Add("SadPencil.Ra2CsfFile.Yaml:YamlVersion", 1.ToString(CultureInfo.InvariantCulture));
            dic.Add("SadPencil.Ra2CsfFile.Yaml:CsfVersion", csf.Version.ToString(CultureInfo.InvariantCulture));
            dic.Add("SadPencil.Ra2CsfFile.Yaml:CsfLang", ((int)csf.Language).ToString(CultureInfo.InvariantCulture));

            foreach (KeyValuePair<string, string> label in csf.Labels)
            {
                string key = label.Key;
                string value = label.Value;
                if (!CsfFile.ValidateLabelName(key))
                {
                    throw new Exception("Invalid characters found in label name \"" + key + "\".");
                }
                dic.Add(key, value);

            }

            var serializer = new SerializerBuilder()
                .WithDefaultScalarStyle(YamlDotNet.Core.ScalarStyle.Literal)
                .Build();
            var yaml = serializer.Serialize(dic);


            using (StreamWriter streamWriter = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                streamWriter.Write(yaml);
            }
        }

        public static CsfFile LoadFromYamlFile(Stream stream, CsfFileOptions options)
        {

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            CsfFile csfFile = new CsfFile(options);
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            var yaml = Encoding.UTF8.GetString(memoryStream.ToArray());
            var dic = deserializer.Deserialize<Dictionary<string, string>>(yaml);

            //if (!dic.ContainsKey(""))
            //{
            //    throw new Exception("Invalid SadPencil.Ra2CsfFile.Ini file. Missing section [SadPencil.Ra2CsfFile.Ini].");
            //}

           

            if (!dic.ContainsKey("SadPencil.Ra2CsfFile.Yaml:CsfVersion"))
            {
                throw new Exception("Invalid SadPencil.Ra2CsfFile.Yaml file. Missing key \"CsfVersion\" in SadPencil.Ra2CsfFile.Yaml:CsfVersion.");
            }

            string value = dic["SadPencil.Ra2CsfFile.Yaml:CsfVersion"];
            csfFile.Version = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            if (!dic.ContainsKey("SadPencil.Ra2CsfFile.Yaml:CsfLang"))
            {
                throw new Exception("Invalid SadPencil.Ra2CsfFile.Yaml file. Missing key \"CsfLang\" in SadPencil.Ra2CsfFile.Yaml:CsfLang.");
            }

            string value2 = dic["SadPencil.Ra2CsfFile.Yaml:CsfLang"];
            csfFile.Language = CsfLangHelper.GetCsfLang(Convert.ToInt32(value2, CultureInfo.InvariantCulture));
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
           
            foreach (var kvp in dic)
            {
                if (kvp.Key.StartsWith("SadPencil.Ra2CsfFile.Yaml"))
                    continue;
                dictionary.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<string, string> item in dictionary)
            {
                string key2 = item.Key;
                if (!CsfFile.ValidateLabelName(key2))
                {
                    throw new Exception("Invalid characters found in label name \"" + key2 + "\".");
                }

                key2 = CsfFile.LowercaseLabelName(key2);
                csfFile.AddLabel(key2, item.Value);
               
            }
            return csfFile;
        }


    }
}
