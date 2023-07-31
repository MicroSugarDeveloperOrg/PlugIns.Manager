namespace PlugIn.Core.Ioc;

public interface IIocStorage 
{
    IServiceRegistry BuildRegistry();
    IServiceProvider BuildProvider();
}
 