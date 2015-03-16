using System;
using TFSAggregator.ConfigTypes;

namespace TFSAggregator
{
    using TfsAggregator;

    public static class Operations
    {
        public static double Perform(this OperationEnum operation, double? aggregateValue, double sourceValue)
        {

            if (operation == OperationEnum.Sum)
                return (aggregateValue ?? 0) + sourceValue;
            if (operation == OperationEnum.Subtract)
                return (aggregateValue ?? 0) - sourceValue;
            if (operation == OperationEnum.Multiply)
            {
                return ((aggregateValue ?? 1) * sourceValue);
            }
            if (operation == OperationEnum.Divide)
            {
                if (!aggregateValue.HasValue)
                {
                    return sourceValue;
                }
                else if (sourceValue.SafeEquals(0d))
                {
                    return 0;
                }
                else
                {
                    return (aggregateValue.Value / sourceValue);
                }
            }

            throw new InvalidOperationException("Unsupported Aggregation Operation");
        }

        public static int Perform(this OperationEnum operation, int? aggregateValue, int sourceValue)
        {

            if (operation == OperationEnum.Sum)
                return (aggregateValue ?? 0) + sourceValue;
            if (operation == OperationEnum.Subtract)
                return (aggregateValue ?? 0) - sourceValue;
            if (operation == OperationEnum.Multiply)
            {
                return ((aggregateValue ?? 1) * sourceValue);
            }
            if (operation == OperationEnum.Divide)
            {
                if (!aggregateValue.HasValue)
                {
                    return sourceValue;
                }
                else if (sourceValue.Equals(0))
                {
                    return 0;
                }
                else
                {
                    return (aggregateValue.Value / sourceValue);
                }
            }

            throw new InvalidOperationException("Unsupported Aggregation Operation");
        }
    }
}