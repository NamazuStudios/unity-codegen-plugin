

namespace Elements.Codegen
{

    public class CodeTemplates
    {

        public string apiClientHookTemplate = @"
using UnityEngine;
using UnityEngine.Networking;

namespace {namespace}.Client
{
    public partial class ApiClient
    {

        partial void InterceptRequest(UnityWebRequest req, string path, RequestOptions options, IReadableConfiguration configuration){}

        partial void InterceptResponse(UnityWebRequest req, string path, RequestOptions options, IReadableConfiguration configuration, ref object responseData)
        {
            if(responseData != null && responseData.GetType() == typeof(Model.SessionCreation))
            {
                var session = (Model.SessionCreation)responseData;
                ElementsClient.SetSessionCreation(session);
            }
        }

    }

}
";

        public string elementsClientTemplate = @"
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace {namespace}.Client
{

    public class ElementsClient
    {

        private static Api.DefaultApi api;

        private static Model.Session session;

        private static string sessionToken;

        private static bool shouldCacheSession;

        private static bool initialized;

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
            api.ApiClient.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            api.ApiClient.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            //Sometimes the JSON deserializer won't apply the above rules to null values
            api.ApiClient.SerializerSettings.Error = (sender, errorArgs) =>
            { 
                errorArgs.ErrorContext.Handled = true;
            };

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
        public static void SetSessionCreation(Model.SessionCreation sessionCreation)
        {
            session = sessionCreation.Session;
            sessionToken = sessionCreation.SessionSecret;
            Api.Configuration.ApiKey[""Elements-SessionSecret""] = sessionCreation.SessionSecret;

            if (shouldCacheSession)
            {
                SaveSessionData();
            }
        }

        /// <summary>
        /// Overwrites the session info in memory, and saves to disk if caching is enabled.
        /// </summary>
        /// <param name=""s"">The Session to assign</param>
        public static void SetSession(Model.Session s)
        {
            session = s;

            if (shouldCacheSession)
            {
                SaveSessionData();
            }
        }

        /// <summary>
        /// Gets the active session info
        /// </summary>
        /// <returns>The Session object</returns>
        public static Model.Session GetSession()
        {
            return session;
        }

        /// <summary>
        /// Adds a profile to the session and saves to disk if caching is enabled.
        /// </summary>
        /// <param name=""p"">The profile to set in the session</param>
        public static void SetProfile(Model.Profile p)
        {
            session.Profile = p;

            if (shouldCacheSession)
            {
                SaveSessionData();
            }
        }

        /// <summary>
        /// Determines if an non-expired session has been loaded into memory.
        /// </summary>
        /// <returns>true if the session is active</returns>
        public static bool IsSessionActive()
        {
            return session != null && session.Expiry > System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Returns a value indicating if ElementsClient.Initialize has been called before.
        /// </summary>
        /// <returns>true if ElementsClient has been initialized</returns>
        public static bool IsInitialized()
        {
            return initialized;
        }

        /// <summary>
        /// Clears the session from memory and deletes it from disk.
        /// </summary>
        public static void ClearSession()
        {
            LogOut();
            DeleteSessionData();
        }

        /// <summary>
        /// Removes the session token from the Api Configuration headers and clears the session in memory.
        /// </summary>
        public static void LogOut()
        {
            session = null;
            sessionToken = null;

            Api.Configuration.ApiKey[""Elements-SessionSecret""] = null;
        }


        private static string GetSessionCachePath()
        {
            return Path.Combine(Application.persistentDataPath, ""ElementsSessionCache.json"");
        }

        private static void SaveSessionData()
        {
            var sessionCreationData = new Model.SessionCreation
            {
                Session = session,
                SessionSecret = sessionToken
            };

            var json = JsonConvert.SerializeObject(sessionCreationData);

            File.WriteAllText(GetSessionCachePath(), json);
        }

        private static void LoadSessionData()
        {
            var path = GetSessionCachePath();

            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                var sessionCreation = JsonConvert.DeserializeObject<Model.SessionCreation>(text);

                session = sessionCreation.Session;
                sessionToken = sessionCreation.SessionSecret;

                //We don't want to assign an expired token
                if(IsSessionActive())
                {
                    Api.Configuration.ApiKey[""Elements-SessionSecret""] = sessionCreation.SessionSecret;
                }
            }
        }

        private static void DeleteSessionData()
        {
            var path = GetSessionCachePath();

            if (File.Exists(path))
            {
                File.Delete(path);
            }

        }
    }
}
";

    }



}


