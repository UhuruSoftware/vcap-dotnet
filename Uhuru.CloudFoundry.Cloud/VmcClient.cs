using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace CloudFoundry.Net
{
    public class VmcClient : IClient
    {
        public bool Target(string apiUrl)
        {
            string output = RunVMC(String.Format("target {0}", apiUrl), true);
            return output.ToLower().StartsWith("succesfully targeted");
        }

        public bool AddUser(string email, string password)
        {
            string output = RunVMC(String.Format("add-user --email {0} --passwd {1}", email, password), true);
            return output.ToLower().StartsWith("creating new user: ok");
        }

        public bool DeleteUser(string email)
        {
            string output = RunVMC(String.Format("delete-user {0}", email), true);
            return output.ToLower().StartsWith("deleting user: ok");
        }

        public bool Login(string email, string password)
        {
            RunVMC("logout");
            string output = RunVMC(String.Format("login --email {0} --passwd {1}", email, password), true);
            return output.ToLower().StartsWith("successfully logged");
        }

        public List<App> Apps()
        {
            List<App> apps = new List<App>();

            List<string> output = RunVMC("apps");

            VmcParser parser = new VmcParser(output);

            if (parser.StructuredOut.ContainsKey("_default"))
            {
                List<Dictionary<string, string>> table = parser.StructuredOut["_default"];
                foreach (Dictionary<string, string> row in table)
                {
                    App app = new App();
                    app.RunningInstances = row["Health"];
                    app.Instances = row["#"];
                    app.Name = row["Application"];
                    app.Services = row["Services"];
                    app.Uris = row["URLS"];
                    apps.Add(app);
                }
            }

            return apps;
        }

        public List<Framework> Frameworks()
        {
            List<Framework> frameworks = new List<Framework>();

            List<string> output = RunVMC("frameworks");

            VmcParser parser = new VmcParser(output);

            if (parser.StructuredOut.ContainsKey("_default"))
            {
                List<Dictionary<string, string>> table = parser.StructuredOut["_default"];
                foreach (Dictionary<string, string> row in table)
                {
                    Framework framework = new Framework();
                    framework.Name = row["Name"];
                    frameworks.Add(framework);
                }
            }

            return frameworks;
        }

        public List<Runtime> Runtimes()
        {
            List<Runtime> runtimes = new List<Runtime>();

            List<string> output = RunVMC("runtimes");

            VmcParser parser = new VmcParser(output);

            if (parser.StructuredOut.ContainsKey("_default"))
            {
                List<Dictionary<string, string>> table = parser.StructuredOut["_default"];
                foreach (Dictionary<string, string> row in table)
                {
                    Runtime runtime = new Runtime();
                    runtime.Name = row["Name"];
                    runtime.Description = row["Description"];
                    runtime.Version = row["Version"];
                    runtimes.Add(runtime);
                }
            }

            return runtimes;
        }

        public List<Service> Services()
        {
            List<Service> services = new List<Service>();

            List<string> output = RunVMC("services");

            VmcParser parser = new VmcParser(output);

            if (parser.StructuredOut.ContainsKey("SystemServices"))
            {
                List<Dictionary<string, string>> table = parser.StructuredOut["SystemServices"];
                foreach (Dictionary<string, string> row in table)
                {
                    Service service = new Service();
                    service.Vendor = row["Service"];
                    service.Description = row["Description"];
                    service.Version = row["Version"];
                    services.Add(service);
                }
            }

            return services;
        }

        public List<ProvisionedService> ProvisionedServices()
        {
            List<ProvisionedService> services = new List<ProvisionedService>();

            List<string> output = RunVMC("services");

            VmcParser parser = new VmcParser(output);

            if (parser.StructuredOut.ContainsKey("ProvisionedServices"))
            {
                List<Dictionary<string, string>> table = parser.StructuredOut["ProvisionedServices"];
                foreach (Dictionary<string, string> row in table)
                {
                    ProvisionedService service = new ProvisionedService();
                    service.Name = row["Name"];
                    service.Type = row["Service"];
                    services.Add(service);
                }
            }

            return services;
        }

        public List<User> Users()
        {
            List<User> apps = new List<User>();

            List<string> output = RunVMC("users");

            VmcParser parser = new VmcParser(output);

            if (parser.StructuredOut.ContainsKey("_default"))
            {
                List<Dictionary<string, string>> table = parser.StructuredOut["_default"];
                foreach (Dictionary<string, string> row in table)
                {
                    User user = new User();
                    user.Email = row["Email"];
                    user.IsAdmin = row["Admin"];
                    user.Apps = row["Apps"];
                    apps.Add(user);
                }
            }

            return apps;
        }

        public string RunVMC(string parameters, bool firstLineOnly)
        {
            List<string> lines = RunVMC(parameters);

            if (firstLineOnly)
            {
                if (lines.Count > 0)
                {
                    return lines[0];
                }
                else
                {
                    return String.Empty;
                }
            }
            else
            {
                return String.Join("\r\n", lines.ToArray());
            }
        }

        public List<string> RunVMC(string parameters)
        {
            if (!Directory.Exists(@"uhuruvmc"))
            {
                Directory.CreateDirectory(@"uhuruvmc");
            }
            ProcessStartInfo psi = new ProcessStartInfo();
            string fn = Guid.NewGuid().ToString("N");
            psi.Arguments = parameters + @" >> uhuruvmc\" + fn + ".txt";
            psi.FileName = @"vmc";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;

            Process vmcProcess = Process.Start(psi);

            if (!vmcProcess.WaitForExit(15000))
            {
                return new List<string>() { "uhuru vmc call failed" };
            }

            if (!File.Exists(@"uhuruvmc\" + fn + ".txt"))
            {
                return new List<string>() { "uhuru vmc call failed" };
            }

            List<string> lines = File.ReadAllLines(@"uhuruvmc\" + fn + ".txt").ToList();
            File.Delete(@"uhuruvmc\" + fn + ".txt");

            return lines;
        }

    }

    class VmcParser
    {
        public Dictionary<string, List<Dictionary<string, string>>> StructuredOut = new Dictionary<string,List<Dictionary<string,string>>>();

        public VmcParser(List<string> output)
        {
            string currentSection = "_default";
            string firstCharQueue = "0123456789";
            List<string> tableHeader = new List<string>();

            foreach (string row in output)
            {
                string line = row == String.Empty ? " " : row;

                if (!StructuredOut.ContainsKey(currentSection))
                {
                    StructuredOut[currentSection] = new List<Dictionary<string, string>>();
                }

                if (line[0] == '=')
                {
                    currentSection = line.Replace("=", "").Replace(" ", "");
                    continue;
                }

                if (line[0] == '|')
                {
                    if (firstCharQueue[firstCharQueue.Length-1] == '+' &&
                        firstCharQueue[firstCharQueue.Length-2] != '|')
                    {
                        List<string> pieces = line.Split('|').ToList();
                        pieces.RemoveAt(0);
                        pieces.RemoveAt(pieces.Count - 1);
                        tableHeader = new List<string>();
                        foreach (string col in pieces)
                        {
                            tableHeader.Add(col.Trim());
                        }
                    }
                    else
                    {
                        StructuredOut[currentSection].Add(new Dictionary<string, string>());
                        
                        List<string> pieces = line.Split('|').ToList();
                        pieces.RemoveAt(0);
                        pieces.RemoveAt(pieces.Count - 1);
                        for (int i=0; i < pieces.Count; i++)
                        {
                            StructuredOut[currentSection][StructuredOut[currentSection].Count - 1].Add(tableHeader[i], pieces[i].Trim());
                        }
                    }
                }

                firstCharQueue += line[0];
            }
        }
    }
}
