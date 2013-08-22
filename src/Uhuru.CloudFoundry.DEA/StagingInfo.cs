using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;
using YamlDotNet.RepresentationModel.Serialization;

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

        public static string GetDetectedBuildpack(string stagingInfoFile)
        {
            string buildpack;

            using (var stream = new StreamReader(stagingInfoFile))
            {
                var yaml = new YamlStream();
                yaml.Load(stream);

                var startCommandScalar = new YamlScalarNode("detected_buildpack");
                var elements = ((YamlMappingNode)yaml.Documents[0].RootNode).Children;

                buildpack = elements[startCommandScalar].ToString();
            }

            return buildpack;
        }

        public static void SaveBuildpackInfo(string stagingInfoFile, string detectedBuildpack, string startCommand)
        {
            var info = new
            {
                detected_buildpack = detectedBuildpack,
                start_command = startCommand
            };

            using(TextWriter writer = File.CreateText(stagingInfoFile))
            {
                new Serializer().Serialize(writer, info);
            }
        }
    }
}
