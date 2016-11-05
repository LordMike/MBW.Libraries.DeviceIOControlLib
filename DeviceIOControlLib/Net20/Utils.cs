#if NET20
namespace System.Runtime.CompilerServices
{
    // http://stackoverflow.com/questions/1522605/using-extension-methods-in-net-2-0
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute { }
}
#endif