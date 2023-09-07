using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceProcess;

// Attribute to specify that this class can be run by the InstallUtil.exe tool
[RunInstaller(true)]
public class ProjectInstaller : System.Configuration.Install.Installer
{
    // Declare variables for the process installer and the service installer
    private ServiceProcessInstaller process;
    private ServiceInstaller service;

    // Constructor
    public ProjectInstaller()
    {
        try
        {
            // Initialize the process installer
            process = new ServiceProcessInstaller();

            // Set the account under which the service will run
            process.Account = ServiceAccount.LocalSystem;

            // Initialize the service installer
            service = new ServiceInstaller();

            // Retrieve the service name from App.config or use a default value
            string serviceName = ConfigurationManager.AppSettings["ServiceName"] ?? "LSLibGrpcService";

            // Set the name of the service
            service.ServiceName = serviceName;

            // Set the start type of the service to Manual
            service.StartType = ServiceStartMode.Manual;

            // Add the installers to the collection. 
            // The framework will use these during the installation process.
            Installers.Add(process);
            Installers.Add(service);
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the setup
            // Assuming you have some logging mechanism in place
            // LogException(ex);
            throw; // Re-throw the exception to make the installation fail
        }
    }
}
