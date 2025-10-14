using System;
using System.Collections.Generic;
using Python.Runtime;

namespace Snowman.Utilities;

public class Helpers
{
    public static List<TBase> PyListToPolymorphicList<TBase>(PyList? pyList, string listName)
    {
        var result = new List<TBase>();
        
        if (pyList is null) return result;
        
        foreach (var item in pyList)
        {
            try
            {
                var obj = (TBase?)item.AsManagedObject(typeof(TBase));

                if (obj != null)
                {
                    result.Add(obj);
                }
            }

            catch (InvalidCastException e)
            {
                throw new ArgumentException($"{listName} contains types incompatible with {typeof(TBase)}.", nameof(pyList), e);
            }

        }
        
        return result;
    }
}
