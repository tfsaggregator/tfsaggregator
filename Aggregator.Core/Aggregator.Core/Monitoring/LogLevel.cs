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
}