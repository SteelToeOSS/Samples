# SimpleCloudFoundry - ASP.NET 5 Sample Application 
ASP.NET 5 sample app illustrating how to use [Config Server for Pivotal Cloud Foundry](http://docs.pivotal.io/spring-cloud-services/config-server/) as a configuration source.

# Pre-requisites

1. Installed Pivotal CloudFoundry 1.7
2. Installed Spring Cloud Services 1.0.9
3. Web tools installed and on PATH, (e.g. npm, gulp, etc).  
Note: If your on Windows and you have VS2015 Update 1, you can add these to your path: `C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\WebTemplates\DNX\CSharp\1033\StarterWeb\node_modules\.bin` and `C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External` and you should get what you need.

# Setup Config Server
You must first create an instance of the Config Server service in a org/space.

1. `cf target -o myorg -s development`
2. `cd Configuration/src/SimpleCloudFoundry`
3. `cf create-service p-config-server standard myConfigServer -c ./config-server.json`

# Find the correct asp.net runtime
1. Ensure `dnvm` and `dnu` are installed properly
1. `dnvm list`  (This will output the currently installed runtimes, pick a linux coreclr runtime e.g. 1.0.0-rc1-update2)
1. Find where the runtime is installed. e.g. `sudo find / -type d | grep "1.0.0-rc1-update2"`
1. `export NET_RUNTIME=/Users/myuser/.dnx/runtimes/dnx-coreclr-linux-x64.1.0.0-rc1-update2`
1. Ensure `dnvm` is using the correct runtime e.g. `dnvm use 1.0.0-rc1-update2 -r coreclr`

# Publish App & Push

1. `cf target -o myorg -s development`
2. `cd Configuration/src/SimpleCloudFoundry`
3. `dnu restore`
4. `cd ..`
5. Publish app to a directory selecting the runtime you want to run on. 
(e.g. `dnu publish --out ./publish --configuration Release  --runtime $NET_RUNTIME SimpleCloudFoundry/`)
6. Push the app using the appropriate manifest. Checkout [CF notes](https://github.com/aminjam/Samples/tree/sccs-vcap-binding/Configuration/src/SimpleCloudFoundry#cf-notes) for troubleshooting tips.
 (e.g. `cf push -f SimpleCloudFoundry/manifest.yml -p ./publish` or `cf push -f SimpleCloudFoundry/manifest-windows.yml -p ./publish`)

Windows Note: If you are pushing to a windows stack, and you are using self-signed certificates you are likely to run into SSL certificate validation issues when pushing this app. You have two choices to fix this:

1. If you have created your own ROOT CA and from it created a certificate that you have installed in HAProxy/Ext LB, then you can install the ROOT CA on the windows cells and you would be good to go.
2. Disable certificate validation for the Spring Cloud Config Server Client.  You can do this by editing `appsettings.json` and add `spring:cloud:client:validate_certificates=false`. This only works on Windows, it will not work on CoreCLR/Linux.

### CF Notes:
We have experienced this [problem](https://github.com/aspnet/KestrelHttpServer/issues/341) with Kestrel running behind a proxy (e.g. HAProxy/Nginx, etc.). As a result, currently this app is configured to run using the `Microsoft.AspNet.Server.WebListener`; which only runs on Windows. If you'd like to try it on Linux, you can change `project.json` to use Kestrel and see what happens. We will change this when moving to RC2 bits. We have successfully pushed this app only using the `coreclr` runtime.
# What to expect
The cf push will create an app in the space by the name `foo` and will bind the `myConfigServer` service instance to the app. You can hit the app @ `http://foo.x.y.z/`. The application name in `SimpleCloudFoundry/manifest.yml` must be `foo`, but you can change the `host` if the name is taken.

The Config Servers Git repository has been set to: `https://github.com/spring-cloud-samples/config-repo`

Use the menus at the top of the app to see various output:

* `CloudFoundry Settings` - should show `VCAP_APPLICATION` and `VCAP_SERVICES` configuration data.
* `Config Server Settings` - should show the settings used by the client when communicating to the config server.  These have been picked up from the service binding.
* `Config Server Data` - this is the configuration data returned from the Config Servers Git Repo. It will be some of the data from `foo.properties`, `foo-development.properties` and `application.yml` found in the Git repo.
* `Reload` - will cause a reload of the configuration data from the server.

# Observe Logs
To see the logs as you startup and use the app: `cf logs foo`

On a Linux cell, you should see something like this during startup:
```
2016-05-03T10:26:59.86-0600 [CELL/0]     OUT Creating container
2016-05-03T10:27:00.25-0600 [CELL/0]     OUT Successfully created container
2016-05-03T10:27:09.06-0600 [CELL/0]     OUT Starting health monitoring of container
2016-05-03T10:27:09.14-0600 [APP/0]      OUT Adding /app/.dnx/runtimes/dnx-coreclr-linux-x64.1.0.0-rc1-final/bin to process PATH
2016-05-03T10:27:17.73-0600 [APP/0]      OUT info: SteelToe.Extensions.Configuration.ConfigServer.ConfigServerConfigurationProvider[0]
2016-05-03T10:27:17.73-0600 [APP/0]      OUT       Fetching config from server at: https://config-de211817-2e99-4c57-89e8-31fa7ca6a276.apps.testcloud.com
2016-05-03T10:27:20.21-0600 [APP/0]      OUT info: SteelToe.Extensions.Configuration.ConfigServer.ConfigServerConfigurationProvider[0]
2016-05-03T10:27:20.21-0600 [APP/0]      OUT       Located environment: foo, development, master, 
2016-05-03T10:27:20.59-0600 [APP/0]      OUT verb: Microsoft.AspNet.Hosting.Internal.HostingEngine[4]
2016-05-03T10:27:20.59-0600 [APP/0]      OUT       Hosting starting
2016-05-03T10:27:20.59-0600 [APP/0]      OUT verb: Microsoft.AspNet.Hosting.Internal.HostingEngine[4]
2016-05-03T10:27:20.59-0600 [APP/0]      OUT       Hosting starting
2016-05-03T10:27:20.66-0600 [APP/0]      OUT verb: Microsoft.AspNet.Hosting.Internal.HostingEngine[5]
2016-05-03T10:27:20.66-0600 [APP/0]      OUT       Hosting started
2016-05-03T10:27:20.66-0600 [APP/0]      OUT verb: Microsoft.AspNet.Hosting.Internal.HostingEngine[5]
2016-05-03T10:27:20.66-0600 [APP/0]      OUT       Hosting started
2016-05-03T10:27:20.66-0600 [APP/0]      OUT Hosting environment: development
2016-05-03T10:27:20.66-0600 [APP/0]      OUT Now listening on: http://0.0.0.0:8080
2016-05-03T10:27:20.66-0600 [APP/0]      OUT Application started. Press Ctrl+C to shut down.
2016-05-03T10:27:21.13-0600 [CELL/0]     OUT Container became healthy

```

