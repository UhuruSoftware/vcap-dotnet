using System;
namespace CloudFoundry.Net
{
    interface IClient
    {
        bool AddUser(string email, string password);
        System.Collections.Generic.List<App> Apps();
        bool DeleteUser(string email);
        System.Collections.Generic.List<Framework> Frameworks();
        bool Login(string email, string password);
        System.Collections.Generic.List<ProvisionedService> ProvisionedServices();
        System.Collections.Generic.List<Runtime> Runtimes();
        System.Collections.Generic.List<Service> Services();
        bool Target(string apiUrl);
        System.Collections.Generic.List<User> Users();
    }
}
