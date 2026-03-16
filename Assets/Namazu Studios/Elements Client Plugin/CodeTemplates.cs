#if UNITY_EDITOR

namespace Elements.Codegen
{

    public class CodeTemplates
    {

        public string apiClientHookTemplate = @"
using UnityEngine.Networking;

namespace {namespace}.Client
{
    public partial class ApiClient
    {

        /// <summary>
        /// The instance of the ElementsClient that owns this instance of ApiClient
        /// </summary>
        public ElementsClient Owner { get; set; }

        // Uncomment and implement to add custom logic before every API request.
        // partial void InterceptRequest(UnityWebRequest req, string path, RequestOptions options, IReadableConfiguration configuration) { }

        partial void InterceptResponse(UnityWebRequest req, string path, RequestOptions options, IReadableConfiguration configuration, ref object responseData)
        {
            if(responseData != null && responseData.GetType() == typeof(Model.SessionCreation))
            {
                var session = (Model.SessionCreation)responseData;
                Owner?.SetSessionCreation(session);
            }
        }

    }

}
";

        public string elementsClientTemplate = @"
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace {namespace}.Client
{

    public class ElementsClient
    {
        public static ElementsClient Default { get; private set; }

        // Shortcut for core Elements API at /api/rest
        public Api.ElementsCoreApi Api => GetApi<Api.ElementsCoreApi>();

        private readonly string instanceRoot;
        private readonly bool shouldCacheSession;
        private readonly string cacheKey;

        private Model.Session session;
        private string sessionToken;
        
        // Registry of APIs and their configs
        private readonly Dictionary<Type, object> apis = new();
        private readonly HashSet<IReadableConfiguration> configurations = new(); // actual type is in your generated Element API

        public const string AUTH_HEADER = ""Elements-SessionSecret"";
        public const string PROFILE_HEADER = ""Elements-ProfileId"";

        public ElementsClient(string instanceRootUrl, bool cacheSession = true, string cacheKey = ""default"")
        {
            instanceRoot = instanceRootUrl;
            shouldCacheSession = cacheSession;
            this.cacheKey = cacheKey;

            // Construct core API at /api/rest and register it
            var coreBaseUrl = CoreBaseUrl;
            var coreApi = new Api.ElementsCoreApi(coreBaseUrl);
            coreApi.ExceptionFactory = null;
            RegisterApi(coreApi);

            if (cacheSession)
            {
                LoadSessionData();
            }
        }

        private string CoreBaseUrl =>
            instanceRoot.TrimEnd('/') + ""/api/rest"";

        private string AppBaseUrl(string appName) =>
            instanceRoot.TrimEnd('/') + ""/app/rest/"" + appName.Trim('/');

        public static void InitializeDefault(string instanceRootUrl, bool cacheSession = true)
        {
            if (Default == null)
            {
                Default = new ElementsClient(instanceRootUrl, cacheSession);
            }
        }

#region API registration & access

        /// <summary>
        /// Register any generated API instance. Auth headers are automatically applied.
        /// </summary>
        public void RegisterApi<TApi>(TApi api) where TApi : class
        {
            var apiType = typeof(TApi);
            apis[apiType] = api;
            
            var config = ExtractConfiguration(api);
            if (config != null && configurations.Add(config))
            {
                ConfigureConfiguration(config);
            }

            var apiClient = ExtractApiClient(api);
            if (apiClient != null)
            {
                ConfigureApiClientSerializer(apiClient);
                apiClient.Owner = this;
            }
        }

        /// <summary>
        /// Get a registered API instance.
        /// </summary>
        public TApi GetApi<TApi>() where TApi : class
        {
            if (apis.TryGetValue(typeof(TApi), out var api))
            {
                return (TApi)api;
            }

            throw new InvalidOperationException(
                $""API of type {typeof(TApi).Name} has not been registered. "" +
                ""Use RegisterApi or CreateAppApi first."");
        }

        /// <summary>
        /// Create and register an app API at /app/rest/{appServePrefix}
        /// as defined by dev.getelements.elements.app.serve.prefix in your custom Element. You can also see
        /// this in the element info in the admin console.
        /// </summary>
        public TApi CreateAppApi<TApi>(string appServePrefix) where TApi : class, IApiAccessor
        {
            var baseUrl = AppBaseUrl(appServePrefix);
            var api = (TApi)Activator.CreateInstance(typeof(TApi), baseUrl);
            api.ExceptionFactory = null; // Set this after creation if you prefer to use it. The default from OpenAPI is buggy :(
            RegisterApi(api);
            return api;
        }

        /// <summary>
        /// If you use code stripping with IL2CPP, it is possible that a constructor can be removed if it isn't
        /// explicitly called. This circumvents that issue by providing a func that is used to call new on the
        /// concrete type. For example: client.CreateAppApi<MyGameApi>(""my-game"", url => new MyGameApi(url));
        /// </summary>
        public TApi CreateAppApi<TApi>(string appServePrefix, Func<string, TApi> factory)
            where TApi : class, IApiAccessor
        {
            var api = factory(AppBaseUrl(appServePrefix));
            api.ExceptionFactory = null;
            RegisterApi(api);
            return api;
        }
        

#endregion

#region  Session / profile handling


        public void SetSessionCreation(Model.SessionCreation sessionCreation)
        {
            session = sessionCreation.Session;
            sessionToken = sessionCreation.SessionSecret;

            ApplySessionHeadersToAllConfigs();

            if (shouldCacheSession)
            {
                SaveSessionData();
            }
        }

        public void SetSession(Model.Session s)
        {
            session = s;

            if (shouldCacheSession)
            {
                SaveSessionData();
            }
        }

        public Model.Session GetSession() => session;
        public string GetSessionToken() => sessionToken;

        public void SetProfile(Model.Profile p)
        {
            if (session == null)
            {
                Debug.LogWarning(""No session set when trying to set profile."");
                return;
            }

            session.Profile = p;

            if (shouldCacheSession)
            {
                SaveSessionData();
            }

            ApplySessionHeadersToAllConfigs();
        }

        public bool IsSessionActive()
        {
            return session != null && session.Expiry >
                   System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void ClearSession()
        {
            LogOut();
            DeleteSessionData();
        }

        public void LogOut()
        {
            session = null;
            sessionToken = null;

            foreach (var cfgObj in configurations)
            {
                ClearSessionHeadersOnConfig(cfgObj);
            }
        }
        
#endregion

#region Caching

        private string GetSessionCachePath() => 
            Path.Combine(Application.persistentDataPath, $""ElementsSessionCache_{cacheKey}.json"");

        private void SaveSessionData()
        {
            var sessionCreationData = new Model.SessionCreation
            {
                Session = session,
                SessionSecret = sessionToken
            };

            var json = JsonConvert.SerializeObject(sessionCreationData);
            File.WriteAllText(GetSessionCachePath(), json);
        }

        private void LoadSessionData()
        {
            var path = GetSessionCachePath();

            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                var sessionCreation = JsonConvert.DeserializeObject<Model.SessionCreation>(text);

                session = sessionCreation.Session;
                sessionToken = sessionCreation.SessionSecret;

                if (IsSessionActive())
                {
                    ApplySessionHeadersToAllConfigs();
                }
            }
        }

        private void DeleteSessionData()
        {
            var path = GetSessionCachePath();

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        
#endregion

#region Internal helpers

        private IReadableConfiguration ExtractConfiguration(object api)
        {
            var prop = api.GetType().GetProperty(""Configuration"",
                BindingFlags.Public | BindingFlags.Instance);

            return (IReadableConfiguration)prop?.GetValue(api);
        }

        private ApiClient ExtractApiClient(object api)
        {
            var prop = api.GetType().GetProperty(""ApiClient"",
                BindingFlags.Public | BindingFlags.Instance);

            return (ApiClient)prop?.GetValue(api);
        }

        private void ConfigureApiClientSerializer(ApiClient client)
        {
            // Using dynamic to avoid hard-coding the exact ApiClient type

            client.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            client.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            client.SerializerSettings.Converters = new List<JsonConverter>
            {
                new Newtonsoft.Json.Converters.StringEnumConverter()
            };

            // EventHandler<ErrorEventArgs>? Error
            client.SerializerSettings.Error = (EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>)((sender, errorArgs) =>
            {
                errorArgs.ErrorContext.Handled = true;
            });
        }

        private void ConfigureConfiguration(IReadableConfiguration configObj)
        {
            // Apply current session headers, if we already have a session
            if (session != null && sessionToken != null)
            {
                ApplySessionHeaders(configObj);
            }
        }

        private void ApplySessionHeadersToAllConfigs()
        {
            foreach (var cfg in configurations)
            {
                ApplySessionHeaders(cfg);
            }
        }

        private void ApplySessionHeaders(IReadableConfiguration config)
        {
            if (session == null || sessionToken == null) return;

            config.ApiKey[AUTH_HEADER] = sessionToken;
            config.DefaultHeaders[AUTH_HEADER] = sessionToken;

            if (session.Profile?.Id != null)
            {
                config.DefaultHeaders[PROFILE_HEADER] = session.Profile.Id;
            }
            else
            {
                config.DefaultHeaders.Remove(PROFILE_HEADER);
            }
        }

        private void ClearSessionHeadersOnConfig(IReadableConfiguration config)
        {
            config.ApiKey[AUTH_HEADER] = null;
            config.DefaultHeaders.Remove(AUTH_HEADER);
            config.DefaultHeaders.Remove(PROFILE_HEADER);
        }
        
#endregion

    }
}
";

    }



}

#endif