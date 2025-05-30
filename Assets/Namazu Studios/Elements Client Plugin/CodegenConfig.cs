using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Elements.Codegen
{
    public class CodegenConfig
    {
        public string packageName = "Elements.Client";

        public string packageVersion = "1.0.0";

        public string urlRoot = "http://localhost:8080";

        public string applicationName = "MyApplication";

        public string relativeDirectory = "Generated";

        public string GetPath()
        {
            string configPath = Path.Combine(Application.dataPath, relativeDirectory);

            return configPath;
        }

        public virtual string ToJson()
        {            
            return JsonUtility.ToJson(this);
        }        
    }

    public class SwaggerCodegenConfig : CodegenConfig
    {
        public string outputDir;

        public bool isApplication;

        public override string ToJson()
        {
            //We tack on the outputDir so that it can be read as part of the
            //Swagger config
            outputDir = GetPath();

            //We want to use a json serializer with reflection in this
            //particular case
            return JsonUtility.ToJson(this);
        }
    }

    public static class CodegenConfigExtensions
    {
        public static SwaggerCodegenConfig ToSwaggerCodegenConfig(this CodegenConfig config)
        {
            var swaggerConfig = new SwaggerCodegenConfig
            {
                applicationName = config.applicationName,
                urlRoot = config.urlRoot,
                packageName = config.packageName,
                packageVersion = config.packageVersion,
                relativeDirectory = config.relativeDirectory
            };

            return swaggerConfig;
        }
    }
}
