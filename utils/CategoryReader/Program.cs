using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategoryReader
{
    class Program
    {
        static void Main(string[] args)
        {
            //ned categories dir, json dir
            if (args.Length < 2)
            {
                Console.WriteLine("invalid input - need categories dir and json data file");
                return;
            }

            var catsDir = args[0];
            if (!Directory.Exists(catsDir))
            {
                Console.WriteLine("categories dir does not exist");
                return;
            }

            var jsonFile = args[1];
            if (!File.Exists(jsonFile))
            {
                Console.WriteLine("json data file does not exist");
            }

            var font = "i54";
            if (args.Length == 3)
            {
                font = args[2];
            }

            //read categories - file name is unique class name, dir is value
            //uses svg files and dir names of the icon54 package 
            var catsMap = new Dictionary<string, string>();
            foreach (var catDir in Directory.GetDirectories(catsDir))
            {
                var cat = Path.GetFileName(catDir); //this will nicely extract the actual dir name
                foreach (var file in Directory.GetFiles(catDir))
                {
                    //i54 uses dash, while i54com empty space. bloody inconsistence.
                    var replacement = string.Empty;
                    switch (font)
                    {
                        case "i54":
                            replacement = "-";
                            break;
                        case "i54com":
                            replacement = string.Empty;
                            break;
                        //possible future additions...
                    }
                    var className = Path.GetFileNameWithoutExtension(file).ToLower().Replace(" ", replacement);
                    if (catsMap.ContainsKey(className))
                    {
                        catsMap[className] += "_" + cat;
                    }
                    else
                    {
                        catsMap.Add(className, cat);
                    }
                }
            }

            //now need to spin through the json and read data
            var newJson = new List<string>();
            foreach (var line in File.ReadLines(jsonFile))
            {
                if (line.Contains("\"name\""))
                {
                    //	{"name":"i54c-volume-10", "iconCls": "x-i54c i54c-volume-10 i54c-3x", "fontCode": "f0d4", "group": "group_name"},
                    var fullClassName = line.Substring(line.IndexOf(":") + 2);
                    fullClassName = fullClassName.Substring(0, fullClassName.IndexOf(",") - 1);
                    var className = fullClassName.Substring(fullClassName.IndexOf("-") + 1);

                    if (catsMap.ContainsKey(className))
                    {
                        var groups = catsMap[className].Split('_');
                        foreach (var group in groups)
                        {
                            newJson.Add(line.Replace("group_name", group));
                        }
                    }
                    else
                    {
                        newJson.Add(line.Replace("group_name", "XX. Uncategorised"));
                    }
                }
            }

            //finally save categorised store data
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(jsonFile), Path.GetFileNameWithoutExtension(jsonFile) + "_categorised.json"), $"[{Environment.NewLine}\t{string.Join(Environment.NewLine + "\t", newJson)}{Environment.NewLine}]");
        }
    }
}
