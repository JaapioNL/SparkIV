using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jaap
{
    public static class PtrContext
    {
        private static Dictionary<UInt32, object> _registeredPointers = new Dictionary<uint, object>();

        private static Dictionary<UInt32, List<uint>> _registeredPointerLocations = new Dictionary<uint, List<uint>>();

        public static void Clear()
        {
            _registeredPointers.Clear();
            _registeredPointerLocations.Clear();
        }

        public static bool Contains(UInt32 offset)
        {
            return _registeredPointers.ContainsKey(offset);
        }

        public static T Get<T>(UInt32 offset)
            where T : class
        {
            return _registeredPointers[offset] as T;
        }

        public static void Add(UInt32 offset, object objectPtr, uint from)
        {
            _registeredPointers.Add(offset, objectPtr);
            _registeredPointerLocations.Add(offset, new List<uint>() { from });
        }

        public static void AddUsage(UInt32 offset, uint from)
        {
            var list = _registeredPointerLocations[offset];
            list.Add(from);
        }
    }
}
