namespace Extensions;

public static class EventGridResouce
{

    public static IResourceBuilder<Aspire.Hosting.Azure.AzureBicepResource> AddEventGridWithBicep(this IDistributedApplicationBuilder builder, [ResourceName] string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Extensions", "eventgrid.bicep");

        var bicep = File.ReadAllText(path);
        var eg = builder.AddBicepTemplateString(
            name,
            bicep
        );

        return eg;
    }
}
