using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace Uhuru.CloudFoundry.DEA
{
    class StagingInfo
    {
        public static string getStartCommand(string stagingInfoFile)
        {
            string startCommand;

            using (var stream = new StreamReader(stagingInfoFile))
            {
                var yaml = new YamlStream();
                yaml.Load(stream);

                var startCommandScalar = new YamlScalarNode("start_command");
                var elements = ((YamlMappingNode)yaml.Documents[0].RootNode).Children;

                startCommand = elements[startCommandScalar].ToString();
            }

            return startCommand;
        }
    }
}
