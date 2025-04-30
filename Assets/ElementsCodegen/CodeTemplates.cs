

namespace Elements.Codegen
{

    public class CodeTemplates
    {

        public string apiClientHookTemplate = @"
using UnityEngine;
using UnityEngine.Networking;

namespace Elements.Client
{
    public partial class ApiClient
    {

        partial void InterceptRequest(UnityWebRequest req, string path, RequestOptions options, IReadableConfiguration configuration){}

        partial void InterceptResponse(UnityWebRequest req, string path, RequestOptions options, IReadableConfiguration configuration, ref object responseData)
        {
            if(responseData != null && responseData.GetType() == typeof(Model.SessionCreation))
            {
                var session = (Model.SessionCreation)responseData;
                ElementsClient.SetSession(session);
            }
        }

    }

}
";

        public string elementsClientTemplate = @"
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace Elements.Client
{

    public class ElementsClient
    {

        private static Api.DefaultApi api;

        private static Model.Session session;

        private static bool shouldCacheSession;

        public static Api.DefaultApi Api
        {
            get {

                if(api == null)
                {
                    Debug.LogError(""Api not initliazed.You must call Elements.Initialize with the root url of your Elements instance first."");
                }

                return api;
            }
        }

        /// <summary>
        /// Initializes the API with the rool url.
        /// Will save session data to Application.persistentDataPath/Application.productName/ElementsSessionCache.json
        /// </summary>
        /// <param name=""rootUrl"">The root url of the instance of Elements that you are running.</param>
        public static void Initialize(string rootUrl)
        {
            Initialize(rootUrl, true);
        }

        /// <summary>
        /// Initializes the API with the rool url.
        /// Will save session data to Application.persistentDataPath/Application.productName/ElementsSessionCache.json
        /// </summary>
        /// <param name=""rootUrl"">The root url of the instance of Elements that you are running.</param>
        /// <param name=""cacheSession"">If true, will attempt to load the session data from, and save it to disk as appropriate.
        /// If not, the session will be only be stored in memory and you will have to log in every time unless you manually store the session yourself.</param>
        public static void Initialize(string rootUrl, bool cacheSession)
        {
            api = new Api.DefaultApi(rootUrl);
            shouldCacheSession = cacheSession;

            if (cacheSession)
            {
                LoadSessionData();
            }
        }

        /// <summary>
        /// Adds the session header to subsequent requests
        /// </summary>
        /// <param name=""session"">The SessionCreation object returned by the Elements server.</param>
        public static void SetSession(Model.SessionCreation sessionCreation)
        {
            Api.Configuration.ApiKey.Add(""Elements-SessionSecret"", sessionCreation.SessionSecret);

            session = sessionCreation.Session;

            if (shouldCacheSession)
            {
                SaveSessionData(sessionCreation);
            }
        }

        /// <summary>
        /// Determines if an non-expired session has been loaded into memory.
        /// </summary>
        /// <returns>true if the session is active</returns>
        public static bool IsSessionActive()
        {
            return session != null && session.Expiry > System.DateTime.UtcNow.Millisecond;
        }

        /// <summary>
        /// Removes the session token from the Api Configuration headers and clears the session in memory.
        /// </summary>
        public static void ClearSession()
        {
            session = null;
            Api.Configuration.ApiKey.Remove(""Elements-SessionSecret"");
        }

        private static string GetSessionCachePath()
        {
            return Path.Combine(Application.persistentDataPath, ""ElementsSessionCache.json"");
        }

        private static void SaveSessionData(Model.SessionCreation session)
        {
            var json = JsonConvert.SerializeObject(session);

            File.WriteAllTextAsync(GetSessionCachePath(), json);
        }

        private static void LoadSessionData()
        {
            var path = GetSessionCachePath();

            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                var sessionCreation = JsonConvert.DeserializeObject<Model.SessionCreation>(text);

                session = sessionCreation.Session;
                Api.Configuration.ApiKey.Add(""Elements-SessionSecret"", sessionCreation.SessionSecret);
            }
        }
    }
}
";

    }



}


