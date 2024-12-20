using System;
using System.Collections.Generic;
using System.Reflection;

namespace Meditation.MetadataLoaderService.Services
{
    internal class AssemblyNameEqualityComparer : IEqualityComparer<AssemblyName>
    {
        public bool Equals(AssemblyName? x, AssemblyName? y)
        {
            // TODO: what properties do we want to match assemblies on? (currently only name + version)
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Name == y.Name && Equals(x.Version, y.Version);
        }

        public int GetHashCode(AssemblyName obj)
        {
            return HashCode.Combine(obj.Name, obj.Version);
        }
    }
}
