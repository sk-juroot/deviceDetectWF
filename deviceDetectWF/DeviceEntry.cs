using System;
using System.Security.Cryptography;
using System.Text;

namespace deviceDetectWF
{
        public sealed class DeviceEntry : IEquatable<DeviceEntry>
        {
            public string DeviceName { get; set; }
            public string DeviceId { get; set; }

            public override string ToString()
            {
             return DeviceName + " [" + DeviceId + "]";
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (!(obj is DeviceEntry objAsPart))
                    return false;
                return Equals(objAsPart);
            }

            public override int GetHashCode()
            {
                MD5 md5Hasher = MD5.Create();
                return BitConverter.ToInt32(md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(DeviceId)), 0);
            }

            public bool Equals(DeviceEntry other)
            {
                if (other == null)
                    return false;
                return (this.DeviceId.Equals(other.DeviceId));
            }
        }
    
}
