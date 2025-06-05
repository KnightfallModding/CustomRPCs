using SCG = System.Collections.Generic;
using Il2CppSCG = Il2CppSystem.Collections.Generic;

public static class ListExtensions
{
    public static void AddIfMissing<T>(this SCG.List<T> list, T item)
    {
        if (!list.Contains(item)) list.Add(item);
    }
    public static void AddIfMissing<T>(this Il2CppSCG.List<T> list, T item)
    {
        if (!list.Contains(item)) list.Add(item);
    }
}