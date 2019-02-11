using System;
using System.Collections.Generic;

namespace deviceDetectWF
{
    public class DevicesList: List<DeviceEntry>
    {
        public string Name { get; }
        public int Id { get; }
        public List<DeviceEntry> Devices { get; set; } = new List<DeviceEntry>();

        public DevicesList(string eventType)
        {
            Name = eventType + " " + DateTime.Now;
            Id = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
        }

    }
}
