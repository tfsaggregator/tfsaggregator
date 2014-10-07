using System;
using TFSAggregator.ConfigTypes;

namespace TFSAggregator
{
    using TfsAggregator;

    public static class Operations
    {
    
        public static double Perform(this OperationEnum operation, double aggregateValue, double sourceValue)
        {
            if (operation == OperationEnum.Sum)
                return aggregateValue + sourceValue;
            if (operation == OperationEnum.Subtract)
                return aggregateValue - sourceValue;
            if (operation == OperationEnum.Multiply)
            {
                if (aggregateValue.SafeEquals(0.0d))
                {
                    aggregateValue = +1;
                }
                return (aggregateValue*sourceValue);
            }
            if (operation == OperationEnum.Divide)
            {
                if (aggregateValue.SafeEquals(0.0d))
                {
                    aggregateValue = +1;
                    return (sourceValue / aggregateValue);
                }
                else
                {
                    return (aggregateValue / sourceValue);
                
                }
            }
            throw new InvalidOperationException("Unsupported Aggregation Operation");
        }

    }
}