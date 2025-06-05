using System;

namespace CustomRPCs;

[AttributeUsage(AttributeTargets.Method)]
public class CustomRPC : Attribute
{
    public static Type TYPE => typeof(CustomRPC);
}