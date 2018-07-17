using System;
using System.Collections.Generic;

namespace Pixelbyte.Json
{
    public class TypeComparer : IEqualityComparer<Type>
    {
        bool IEqualityComparer<Type>.Equals(Type x, Type y) => x == y;
        int IEqualityComparer<Type>.GetHashCode(Type obj) => obj.GetHashCode();
    }
}
