# SimpleCloudFoundry - ASP.NET Core Sample Application 
ASP.NET Core sample app illustrating how to use [Config Server for Pivotal Cloud Foundry](https://docs.pivotal.io/spring-cloud-services/config-server/) as a configuration source.

# Pre-requisites

1. Installed Pivotal CloudFoundry 1.7
2. Installed Spring Cloud Services 1.0.9
3. .NET Core SDK
4. Web tools installed and on PATH, (e.g. npm, gulp, etc).  
Note: If your on Windows and you have VS2015 Update 1, you can add these to your path: `C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\WebTemplates\DNX\CSharp\1033\StarterWeb\node_modules\.bin` and `C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External` and you should get what you need.

# Setup Config Server
You must first create an instance of the Config Server service in a org/space.

1. cf target -o myorg -s development
2. cd src/SimpleCloudFoundry
3. cf create-service p-config-server standard myConfigServer -c ./config-server.json

# Publish App & Push

1. cf target -o myorg -s development
2. cd src/SimpleCloudFoundry
3. dotnet restore --configfile nuget.config 
4. Publish app to a directory selecting the framework and runtime you want to run on. 
(e.g. `dotnet publish --output $PWD/publish --configuration Release --framework netcoreapp1.0 --runtime ubuntu.14.04-x64`)
5. Push the app using the appropriate manifest.
 (e.g. `cf push -f manifest.yml -p $PWD/publish` or `cf push -f manifest-windows.yml -p $PWD/publish`)

Windows Note: If you are pushing to a windows stack, and you are using self-signed certificates you are likely to run into SSL certificate validation issues when pushing this app. You have two choices to fix this:

1. If you have created your own ROOT CA and from it created a certificate that you have installed in HAProxy/Ext LB, then you can install the ROOT CA on the windows cells and you would be good to go.
2. Disable certificate validation for the Spring Cloud Config Server Client.  You can do this by editing `appsettings.json` and add `spring:cloud:config:validate_certificates=false`. This only works on Windows, it will not work on CoreCLR/Linux.

Note: We have experienced this [problem](https://github.com/dotnet/cli/issues/3283) when using the RC2 SDK and when publishing to a relative directory... workaround is to use full path.

# What to expect
The cf push will create an app in the space by the name `foo` and will bind the `myConfigServer` service instance to the app. You can hit the app @ `https://foo.x.y.z/`.

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
2016-06-01T09:14:14.38-0600 [CELL/0]     OUT Creating container
2016-06-01T09:14:15.93-0600 [CELL/0]     OUT Successfully created container
2016-06-01T09:14:17.14-0600 [CELL/0]     OUT Starting health monitoring of container
2016-06-01T09:14:18.01-0600 [APP/0]      OUT info: SteelToe.Extensions.Configuration.ConfigServer.ConfigServerConfigurationProvider[0]
2016-06-01T09:14:18.01-0600 [APP/0]      OUT       Fetching config from server at: https://config-92e894b5-17e2-4b94-941e-a544c6488de7.apps.testcloud.com
2016-06-01T09:14:19.59-0600 [APP/0]      OUT info: SteelToe.Extensions.Configuration.ConfigServer.ConfigServerConfigurationProvider[0]
2016-06-01T09:14:19.59-0600 [APP/0]      OUT       Located environment: foo, development, master, 
2016-06-01T09:14:19.59-0600 [APP/0]      OUT info: SteelToe.Extensions.Configuration.ConfigServer.ConfigServerConfigurationProvider[0]
2016-06-01T09:14:19.59-0600 [APP/0]      OUT       Fetching config from server at: https://config-92e894b5-17e2-4b94-941e-a544c6488de7.apps.testcloud.com
2016-06-01T09:14:20.46-0600 [APP/0]      OUT info: SteelToe.Extensions.Configuration.ConfigServer.ConfigServerConfigurationProvider[0]
2016-06-01T09:14:20.46-0600 [APP/0]      OUT       Located environment: foo, development, master, 
2016-06-01T09:14:20.93-0600 [APP/0]      OUT dbug: Microsoft.AspNetCore.Hosting.Internal.WebHost[3]
2016-06-01T09:14:20.93-0600 [APP/0]      OUT       Hosting starting
2016-06-01T09:14:21.04-0600 [APP/0]      OUT dbug: Microsoft.AspNetCore.Hosting.Internal.WebHost[4]
2016-06-01T09:14:21.04-0600 [APP/0]      OUT       Hosting started
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Hosting environment: development
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Content root path: /home/vcap/app
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Now listening on: http://*:8080
2016-06-01T09:14:21.04-0600 [APP/0]      OUT Application started. Press Ctrl+C to shut down.
2016-06-01T09:14:21.41-0600 [CELL/0]     OUT Container became healthy

```

