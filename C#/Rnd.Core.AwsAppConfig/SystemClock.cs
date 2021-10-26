using System;

namespace Checkout.Configuration.AppConfig
{
    interface ISystemClock
    {
        DateTime UtcNow { get; }
    }

    class SystemClock : ISystemClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
