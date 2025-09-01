using TEMPLATE.Cookbook.Settings;
using RecipeEngine;
using RecipeEngine.Modules.Wrench.Helpers;


// ReSharper disable once CheckNamespace
public static class Program
{
    public static int Main(string[] args)
    {
        var settings = new TEMPLATESettings();

        // ReSharper disable once UnusedVariable
        var engine = EngineFactory
            .Create()
            .ScanAll()
            .WithWrenchModule(settings.Wrench)
            .GenerateAsync().Result;
        return engine;
    }
}
