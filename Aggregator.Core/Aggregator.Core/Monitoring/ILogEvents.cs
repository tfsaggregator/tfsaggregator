using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Runtime.Caching;
using System.Xml.Schema;
using Aggregator.Core.Configuration;
using Aggregator.Core.Facade;
using Aggregator.Core.Interfaces;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Monitoring
{
    /// <summary>
    /// Core Clients will be called on this interface to log events and errors.
    /// </summary>
    /// <remarks>The methods must *not* raise exceptions.</remarks>
    public interface ILogEvents : ILogger
    {
        IRuleLogger ScriptLogger { get; set; }

        void HelloWorld();

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

        void WorkItemRepositoryBuilt(Uri uri, WorkItemRepository.AuthenticationToken token);

        void TemplateScopeConfigurationRequiresAtLeastName();

        void FieldValidationFailedInvalidDataType(int id, string referenceName, Type systemType, Type valueType, object value);

        void FieldValidationFailedValueNotAllowed(int id, string referenceName, object value);

        void FieldValidationFailedFieldNotEditable(int id, string referenceName, object value);

        void FieldValidationFailedFieldRequired(int id, string referenceName);

        void FieldValidationFailedAssignmentToHistory(int id);

        void LibrarySendMail(string from, string to, string subject, string body);

        void Connecting(ConnectionInfo ci);

        void ReadingGlobalList(string collectionName, string globalListName);

        void AddingToGlobalList(string name, string globalListName, string item);

        void RemovingFromGlobalList(string name, string globalListName, string item);

        void RemovingWorkItemLink(WorkItemLink item);

        void WorkItemLinkNotFound(IWorkItemLinkExposed link);

        void UsingFakeGetEmailAddress(string userName, string defaultValue);

        void UsingFakeSendMail();

        void WhatIfSave(IWorkItem workItem);
    }
}
