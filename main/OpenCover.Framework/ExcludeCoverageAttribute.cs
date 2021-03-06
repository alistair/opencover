using System;

namespace OpenCover.Framework
{
    /// <summary>
    /// This attribute can be used to hide code whilst testing
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Constructor)]
    [ExcludeFromCoverage]
    internal class ExcludeFromCoverageAttribute : Attribute
    {
    }
}