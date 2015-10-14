using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Runtime.Caching;
using System.Xml.Schema;
using Aggregator.Core.Configuration;
using Aggregator.Core.Interfaces;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Monitoring
{
    /// <summary>
    /// Levels of logging.
    /// </summary>
    /// <remarks>While this enumeration is not used within Core, it is read by the configuration class <see cref="Aggregator.Core.Configuration.TFSAggregatorSettings"/>.</remarks>
    public enum LogLevel
    {
        Critical = 1,
        Error = 2,
        Warning = 3,
        Information = 5,
        Normal = Information,
        Verbose = 10,
        Diagnostic = 99,
    }

    public interface ILogger
    {
        LogLevel MinimumLogLevel { get; set; }
    }

    /// <summary>
    /// Core Clients will be called on this interface to log events and errors.
    /// </summary>
    /// <remarks>The methods must *not* raise exceptions.</remarks>
    public interface ILogEvents : ILogger
    {
        IRuleLogger ScriptLogger { get; set; }

        void ScriptLog(LogLevel level, string ruleName, string message);

        void ConfigurationLoaded(string policyFile);

        void StartingProcessing(IRequestContext context, INotification notification);

        void ProcessingCompleted(ProcessingResult result);

        void WorkItemWrapperTryOpenException(IWorkItem workItem, Exception e);

        void ResultsFromScriptRun(string scriptName, Collection<PSObject> results);

        void ResultsFromScriptRun(string scriptName, object result);

        void ScriptHasError(string scriptName, int line, int column, string errorCode, string errorText);

        void ScriptHasWarning(string scriptName, int line, int column, string errorCode, string errorText);

        void Saving(IWorkItem workItem, bool isValid, bool shouldLimit);

        void InvalidConfiguration(XmlSeverityType severity, string message, int lineNumber, int linePosition);

        void UnreferencedRule(string ruleName);

        void ApplyingPolicy(string name);

        void ApplyingRule(string name);

        void BuildingScriptEngine(string scriptLanguage);

        void RunningRule(string name, IWorkItem workItem);

        void FailureLoadingScript(string scriptName);

        void AttemptingToMoveWorkItemToState(IWorkItem workItem, string orginalSourceState, string destState);

        void WorkItemIsValidToSave(IWorkItem workItem);

        void WorkItemIsInvalidInState(IWorkItem workItem, string destState);

        void LoadingConfiguration(string settingsPath);

        void PolicyShouldHaveAScope(string name);

        void UsingCachedConfiguration(string settingsPath);

        void ConfigurationChanged(string settingsPath, CacheEntryRemovedReason removedReason);

        void AddingWorkItemLink(int sourceId, WorkItemLinkTypeEnd destLinkType, int destId);

        void WorkItemLinkAlreadyExists(int sourceId, WorkItemLinkTypeEnd destLinkType, int destId);

        void AddingHyperlink(int id, string destination, string comment);

        void HyperlinkAlreadyExists(int id, string destination, string comment);

        void NoPolicesApply();

        void PolicyScopeMatchResult(PolicyScope scope, ScopeMatchResult result);

        void RuleScopeMatchResult(RuleScope scope, ScopeMatchResult result);

        void WorkItemRepositoryBuilt(Uri uri, IdentityDescriptor toImpersonate);

        void TemplateScopeConfigurationRequiresAtLeastNameOrType();
    }
}
