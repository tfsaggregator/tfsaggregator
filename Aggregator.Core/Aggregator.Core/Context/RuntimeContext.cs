using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

using Aggregator.Core.Configuration;
using Aggregator.Core.Extensions;
using Aggregator.Core.Facade;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace Aggregator.Core.Context
{
    /// <summary>
    /// Manages the global inter-call status
    /// </summary>
    public class RuntimeContext : IRuntimeContext
    {
        private const string CacheKey = "runtime:";
#pragma warning disable CA2213 // Disposable fields should be disposed
        private static readonly MemoryCache Cache = new MemoryCache("TFSAggregator2");
#pragma warning restore CA2213 // Disposable fields should be disposed

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeContext"/> class.
        /// </summary>
        protected RuntimeContext()
        {
            // default
            this.HasErrors = true;
        }

        /// <summary>
        /// Return a proper context
        /// </summary>
        /// <returns></returns>
        public static RuntimeContext GetContext(
            Func<string> settingsPathGetter,
            IRequestContext requestContext,
            ILogEvents logger,
            Func<IRuntimeContext, IWorkItemRepository> repoBuilder,
            Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder)
        {
            string settingsPath = settingsPathGetter();
            string cacheKey = CacheKey + settingsPath;
            var runtime = (RuntimeContext)Cache.Get(cacheKey);
            if (runtime == null)
            {
                logger.HelloWorld();

                logger.LoadingConfiguration(settingsPath);

                var settings = TFSAggregatorSettings.LoadFromFile(settingsPath, logger);
                runtime = MakeRuntimeContext(settingsPath, settings, requestContext, logger, repoBuilder, scriptLibraryBuilder);

                if (!runtime.HasErrors)
                {
                    var itemPolicy = new CacheItemPolicy();
                    itemPolicy.Priority = CacheItemPriority.NotRemovable;
                    itemPolicy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string>() { settingsPath }));

                    Cache.Set(cacheKey, runtime, itemPolicy);
                }

                logger.ConfigurationLoaded(settingsPath);
            }
            else
            {
                logger.UsingCachedConfiguration(settingsPath);
            }

            runtime = runtime.Clone() as RuntimeContext;

            // as it changes at each invocation, must be set again here
            runtime.RequestContext = requestContext;
            runtime.workItemRepository = null;
            return runtime;
        }

        public static RuntimeContext MakeRuntimeContext(
            string settingsPath,
            TFSAggregatorSettings settings,
            IRequestContext requestContext,
            ILogEvents logger,
            Func<IRuntimeContext, IWorkItemRepository> repoBuilder,
            Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder)
        {
            var runtime = new RuntimeContext();

            runtime.Logger = logger;
            runtime.RequestContext = requestContext;
            runtime.SettingsPath = settingsPath;
            runtime.Settings = settings;
            runtime.RateLimiter = new RateLimiter(runtime);
            logger.MinimumLogLevel = runtime.Settings?.LogLevel ?? LogLevel.Normal;
            runtime.repoBuilder = repoBuilder;
            runtime.scriptLibraryBuilder = scriptLibraryBuilder;

            runtime.HasErrors = settings == null;
            return runtime;
        }

        public bool HasErrors { get; private set; }

        private readonly List<string> errorList = new List<string>();

        public IEnumerator<string> Errors
        {
            get
            {
                return this.errorList.GetEnumerator();
            }
        }

        public RateLimiter RateLimiter { get; private set; }

        public IRequestContext RequestContext { get; private set; }

        public string SettingsPath { get; private set; }

        public TFSAggregatorSettings Settings { get; private set; }

        public string Hash
        {
            get
            {
                return this.Settings.Hash;
            }
        }

        public ILogEvents Logger { get; private set; }

        private Func<IRuntimeContext, IScriptLibrary> scriptLibraryBuilder;

        public ScriptEngine GetEngine()
        {
            const string EngineCacheKey = "engine:";

            string cacheKey = EngineCacheKey + this.SettingsPath;
            var engine = (ScriptEngine)Cache.Get(cacheKey);
            if (engine == null)
            {
                System.Diagnostics.Debug.WriteLine("No cached engine for thread {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
                IScriptLibrary library = this.scriptLibraryBuilder(this);
                engine = ScriptEngine.MakeEngine(this.Settings.ScriptLanguage, this.Logger, this.Settings.Debug, library);

                List<Script.ScriptSourceElement> sourceElements = this.GetSourceElements();

                engine.Load(sourceElements);

                var itemPolicy = new CacheItemPolicy();
                itemPolicy.Priority = CacheItemPriority.NotRemovable;
                itemPolicy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string>() { this.SettingsPath }));

                Cache.Set(cacheKey, engine, itemPolicy);
            }

            return engine;
        }

        private List<Script.ScriptSourceElement> GetSourceElements()
        {
            var sourceElements = new List<Script.ScriptSourceElement>();
            var snippetElements = this.Settings.Snippets.ToList().ConvertAll(
                (snippet) =>
                {
                    return new Script.ScriptSourceElement()
                    {
                        Name = snippet.Name,
                        Type = Script.ScriptSourceElementType.Snippet,
                        SourceCode = snippet.Script
                    };
                });
            sourceElements.AddRange(snippetElements);
            var ruleElements = this.Settings.Rules.ToList().ConvertAll(
                (rule) =>
                {
                    return new Script.ScriptSourceElement()
                    {
                        Name = rule.Name,
                        Type = Script.ScriptSourceElementType.Rule,
                        SourceCode = rule.Script
                    };
                });
            sourceElements.AddRange(ruleElements);
            var functionElements = this.Settings.Functions.ToList().ConvertAll(
                (function) =>
                {
                    return new Script.ScriptSourceElement()
                    {
                        Name = string.Empty,
                        Type = Script.ScriptSourceElementType.Function,
                        SourceCode = function.Script
                    };
                });
            sourceElements.AddRange(functionElements);
            return sourceElements;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var requestUri = this.RequestContext.GetProjectCollectionUri();
            var uri = requestUri.ApplyServerSetting(this);

            // get credentials for connecting to TFS/VSTS from configuration
            WorkItemRepository.AuthenticationToken token = new WorkItemRepository.WindowsIntegratedAuthenticationToken();

            if (!string.IsNullOrWhiteSpace(this.Settings.PersonalToken))
            {
                token = new WorkItemRepository.PersonalAuthenticationToken(this.Settings.PersonalToken);
            }
            else if (!string.IsNullOrWhiteSpace(this.Settings.BasicUsername))
            {
                token = new WorkItemRepository.BasicAuthenticationToken(
                    this.Settings.BasicUsername,
                    this.Settings.BasicPassword);
            }
            else if (this.Settings.AutoImpersonate)
            {
                token = new WorkItemRepository.ImpersonateAuthenticationToken(
                    this.RequestContext.GetIdentityToImpersonate(uri));
            }

            return new ConnectionInfo(uri, token);
        }

        // isolate type constructor to facilitate Unit testing
        private Func<IRuntimeContext, IWorkItemRepository> repoBuilder;

        protected virtual IWorkItemRepository CreateWorkItemRepository()
        {
            var requestUri = this.RequestContext.GetProjectCollectionUri();
            var uri = requestUri.ApplyServerSetting(this);

            // before trying to connect to the TFS/VSTS specified by the URI prepare the environment
            if (this.Settings.IgnoreSslErrors)
            {
                // HACK this applies to other policies
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
            }

            var newRepo = this.repoBuilder(this);
            var ci = this.GetConnectionInfo();
            this.Logger.WorkItemRepositoryBuilt(uri, ci.Token);
            return newRepo;
        }

        private IWorkItemRepository workItemRepository;

        public IWorkItemRepository WorkItemRepository
        {
            get
            {
                if (this.workItemRepository == null)
                {
                    this.workItemRepository = this.CreateWorkItemRepository();
                }

                return this.workItemRepository;
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
