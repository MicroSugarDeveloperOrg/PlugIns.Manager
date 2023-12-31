﻿namespace PlugIn.Core.Enum;
public enum InitializationMode
{
    /// <summary>
    /// The module will be initialized when it is available on application start-up.
    /// </summary>
    WhenAvailable,

    /// <summary>
    /// The module will be initialized when requested, and not automatically on application start-up.
    /// </summary>
    OnDemand
}