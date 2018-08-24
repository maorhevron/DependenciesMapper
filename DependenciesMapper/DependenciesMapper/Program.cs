using System;
using System.Collections.Generic;
using System.IO;

namespace DependenciesMapper
{
    class Program
    {
        static IDictionary<string, IList<string>> fileDictionary = new Dictionary<string, IList<string>>();
        static string outputPath;

        static void Main(string[] args)
        {
            initOutputPath();
            string rootStringDir = "D:\\TFS\\Cornerstone\\DevRelease\\Cornerstone.Tincan";
            MapDependencies(rootStringDir);

            rootStringDir = "D:\\TFS\\Cornerstone\\DevRelease\\CornerstoneApp\\RestService";
            MapDependencies(rootStringDir);

            exportDependenciesToFile();
            exportLibrariesListToFile();
        }

        private static void initOutputPath()
        {
            //The current working directory defined as the output path (inside the OutputFiles dir)
            outputPath = AppDomain.CurrentDomain.BaseDirectory;
            outputPath = outputPath.Remove(outputPath.IndexOf("bin"));
            outputPath += "OutputFiles\\";
        }

        private static void MapDependencies(string rootStringDir)
        {
            IList<string> files = DirSearch(rootStringDir);
            foreach (string file in files)
            {
                ReadFileAndStore(file);
            }
        }

        private static IList<string> DirSearch(string sDir)
        {
            IList<string> files = new List<string>(); 
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        files.Add(f);
                    }
                    foreach(string file in DirSearch(d))
                    {
                        files.Add(file);
                    }
                }

            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return files;
        }

        private static void ReadFileAndStore(string fileName)
        {
            if (!fileName.EndsWith(".cs"))
            {
                return;
            }

            using (var reader = new StreamReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if(line.Contains("namespace"))
                    {
                        return;
                    }

                    string trimmed = line.Trim();
                    if(trimmed.StartsWith("using"))
                    {
                        IList<string> users;
                        string clazzName = trimmed.Split(' ')[1];
                        if(fileDictionary.ContainsKey(clazzName))
                        {
                            users = fileDictionary[clazzName];
                            users.Add(fileName);
                            fileDictionary[clazzName] = users;
                        }
                        else
                        {
                            users = new List<string>();
                            users.Add(fileName);
                            fileDictionary.Add(clazzName, users);
                        }
                    }
                }
            }
        }

        private static void exportDependenciesToFile()
        {
            using (StreamWriter sw = new StreamWriter(outputPath + "Dependencies.txt", true))
            {
                foreach (KeyValuePair<string, IList<string>> entry in fileDictionary)
                {
                    sw.WriteLine("Library: " + entry.Key);
                    foreach (string clazz in entry.Value)
                    {
                        sw.WriteLine("\t\t" + clazz);
                    }

                    sw.WriteLine();
                }
            }
        }
        private static void exportLibrariesListToFile()
        {
            using (StreamWriter sw = new StreamWriter(outputPath + "LibrariesList.txt", true))
            {
                foreach (KeyValuePair<string, IList<string>> entry in fileDictionary)
                {
                    sw.WriteLine(entry.Key);
                }
            }
        }
    }
}
