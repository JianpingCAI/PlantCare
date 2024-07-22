namespace PlantCare.App.Tests.Common
{
    public class ServiceProviderFixture : IDisposable
    {
        public ServiceProvider ServiceProvider { get; private set; }

        public ServiceProviderFixture()
        {
            ServiceProvider = ServiceProviderFactory.Create();
        }

        public void Dispose()
        {
            ServiceProvider.Dispose();
        }
    }
}