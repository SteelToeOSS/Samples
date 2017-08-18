# Steeltoe MusicStore Sample Application
This repo tree contains a sample app illustrating how to use all of the Steeltoe components together in a ASP.NET Core application. This application is based on the ASP.NET Core reference app [MusicStore](https://github.com/aspnet/MusicStore) provided by Microsoft.

In creating this application, we took the Microsoft reference application and broke it up into multiple independent services:
* MusicStoreUI - provides the UI to the MusicStore application and all of its services.
* MusicStoreService - provides a RESTful API to the MusicStore and its backend Music database.
* OrderService - provides a RESTful API for Order processing service and its backend Order database. 
* ShoppingCartService - provides a RESTful api to a ShoppingCart service and its backend ShoppingCart database.

Note: The OrderService and ShoppingCartService are independent from the Music application and could be used in any other application requiring those basic services.

This application makes use of the following Steeltoe components:
* Spring Cloud Config Server Client for centralized application configuration
* Spring Cloud Eureka Server Client for service discovery
* Steeltoe Connector for connecting to MySql using EFCore 
* Optionally uses Steeltoe Redis Connector to connect to a Redis cache for Session storage. Note: This is required if you want to scale the MusicStoreUI component to multiple instances.
* Optionally uses Steeltoe Redis DataProtection provider to the cause the DataProtection KeyRing to be stored in a Redis cache. Note: This is also required if you want to scale the MusicStoreUI component to multiple instances.

The default is to NOT use a Redis cache for Session storage or DataProtection KeyRing storage. Details on how to enable Redis usage are provided below.

# Getting Started

* Clone the Samples repo. (i.e.  git clone https://github.com/SteeltoeOSS/Samples)

## Pre-requisites - Local

1. Installed .NET Core SDK.
2. You need running instances of the following external services on your local machine:

* Spring Cloud Config Server - listening @ `http://localhost:8888` 
* Spring Cloud Eureka Server - listening @ `http://localhost:8761/eureka/`
* MySql Database Server - listening @ `localhost:3306` username: `root`, password: `steeltoe` 
* Redis Cache - Optional, can be used for Session state backing store and KeyRing storage.

You have a three options to choose from in order to get these services up and running locally:

* Use pre-built [Steeltoe Docker images](https://hub.docker.com/r/steeltoeoss/servers/). 
* On Windows 10, use pre-built Steeltoe Windows Container images. (Note: Windows containers on Windows 10 and Windows Server 2016 is still in Beta. Consider it experimental)
* Install each service manually.

Currently, the simplest way to get these up and running is to use the first option above together with the provided `dockerrun-*.cmd, dockerrun-*.sh` files to startup those services locally on your machine.  

### Pre-requisites - Using Steeltoe Docker images

If you don't have Docker installed on your local machine, you can use:
* Windows 10 - [Docker for Windows](https://docs.docker.com/docker-for-windows/) 
* MacOS - [Docker for Mac](https://docs.docker.com/docker-for-mac/)

After installing either of the above, you need to enable [`File Sharing`](https://docs.docker.com/docker-for-windows/#/shared-drives) between your Docker VM and your local machine. 
On Windows 10, share the `C:` drive from you local machine with the VM. On MacOS, share your Home directory with the VM. This is necessary as the pre-built Config Server image has been pre-configured to read its configuration data from `~/steeltoe/config-repo` or `C:\steeltoe\config-repo` off of the local machine.

Once you have Docker installed and running you can use the provided command files to startup the various services.  For example to startup a Spring Cloud Config Server:

1. `cd Samples/MusicStore`
2. `start dockerrun-configserver.cmd` or `./dockerrun-configserver.sh`

This will create a directory `~/steeltoe/config-repo` or `c:\steeltoe\config-repo` if it doesn't exist and then fire up a Spring Cloud Config Server listening on port 8888. 

Likewise to startup a Spring Cloud Eureka Server:

1. `cd Samples/MusicStore`
2. `start dockerrun-eurekaserver.cmd or ./dockerrun-eurekaserver.sh`

This will fire up a Spring Cloud Eureka Server listening on port 8761.

And finally to startup a MySql Server.  Note: On MacOS you can NOT use MySQL. Instead, you must use Postgres as there currently are no MySql .NET providers supported on .NET Core. 

1. `cd Samples/MusicStore`
2. `start dockerrun-mysqlaserver.cmd` or `./dockerrun-mysqlaserver.sh`

This will fire up a MySql Server listening on port `3306` with username: `root` and password: `steeltoe`.

### Pre-requisites - Using Windows Containers
Details to be provided when Windows containers stabilize! 

# Building & Running MusicStore App - Locally

Once you have the pre-requisite services up and running then you are ready to build and run the various MusicStore services locally. Before starting up any of the services you first need to copy the MusicStore configuration files to the `\steeltoe\config-repo' so the running Config Server will have access to them.

1. `cd Samples/MusicStore/src/config`
2. `copy *.* c:/steeltoe/config-repo` or `cp *.* ~/steeltoe/config-repo`

Once thats complete, then you are ready to fire up the individual services. The simplest way to get these up and running is to use the provided `run*.cmd or run*.sh` files.

For example, to startup the MusicStoreService:

1. `cd Samples/MusicStore`
2. `runMusicStoreService.cmd` or `./runMusicStoreService.sh`

Its probably best to startup the` MusicStoreService`, `OrderService` and `ShoppingCartService` first and then follow up with the` MusicStoreUI` last.

The `run*.*` commands will `dotnet run -f netcoreapp1.1` (i.e. target .NET Core)

If all the services startup cleanly, you should be able to hit: http://localhost:5555/ to see the Music Store.

## Debugging/Developing Locally on Windows
You should have no problem using the provided solution to launch the individual services in the debugger and set break points and walk through code as needed.

# Pre-requisites - CloudFoundry

1. Install Pivotal CloudFoundry 1.7+
2. Install Spring Cloud Services 1.1.x+.
3. Install .NET Core SDK.
4. Install Redis service if you want to use Redis for Sesion storage and KeyRing storage.

# Setup Services on CloudFoundry

As mentioned above, the application is dependent on the following services:
* Spring Cloud Config Server 
* Spring Cloud Eureka Server 
* MySql Database Server - Default database used by all MusicStore services.
* Redis Cache - Optional! Note you have to specifically build/publish MusicStoreUI service to use Redis (Details below).
 
Note: Redis Cache is required if you want to scale the MusicStoreUI app to multiple instances (e.g. cf scale musicui -i 2+). It is not required to scale the other microservices.

Before pushing the application to CloudFoundry you need to create those services.  If you plan on using Redis, set the environment varialble USE_REDIS_CACHE=true before running these command.

1. `cf target -o myOrg -s mySpace`
2. `cd Samples/MusicStore`
3. Optionally - `SET USE_REDIS_CACHE=true` or `export USE_REDIS_CACHE=true`
4. `start createCloudFoundryServices.cmd` or `./createCloudFoundryServices.sh`

This will create all of the services needed by the application.  Specifically, it creates:
* mStoreConfig - Spring Cloud Config Server instance
* mStoreRegistry - Spring Cloud Eureka Server instance
* mStoreAccountsDB - MySql database instance for Users and Roles (Identity)
* mStoreOrdersDB - MySql database instance for Orders
* mStoreCartDB - MySql database instance for ShoppingCarts
* mStoreStoreDB - MySql database instance for MusicStore
* mStoreRedis(optionally) - Redis cache instance used by MusicStoreUI for storing Session state

Note: The Spring Cloud Config Server instance created by the above script configures the Config Server instance to use the git repo: https://github.com/SteeltoeOSS/musicStore-config.git.  This repo contains the same configuration files as those found in `Samples/MusicStore/config`.
No changes are required to the application configuration files before pushing the app to CloudFoundry. 

Note: If you wish to change what github repo the Config server instance uses, you can modify config-server.json before using the `createCloudFoundryServices` script above.

# Building & Pushing App - CloudFoundry

Once the services have been created and ready on CloudFoundry (i.e. check via `cf services`) then you can use the provided `push*.cmd or push*.sh` commands to startup the individual application services on CloudFoundry. For example to start the ShoppingCart service:

1. `cd Samples/MusicStore`
2. `pushShoppingCartService.cmd win7-x64 netcoreapp1.1` or `./pushShoppingCartService.sh ubuntu.14.04-x64 netcoreapp1.1`

Note: If you wish to use the Redis cache for storing Session state, you will have to set ENVIRONMENT variable `USE_REDIS_CACHE=true` AND modify the [`project.json`](https://github.com/SteeltoeOSS/Samples/blob/master/MusicStore/src/MusicStoreUI/project.json) file for the `MusicStoreUI` application before pushing it. 

To define `USE_REDIS_CACHE` at build/publish time modify the `buildOptions` section in [`project.json`](https://github.com/SteeltoeOSS/Samples/blob/master/MusicStore/src/MusicStoreUI/project.json) as follows:
```
  "buildOptions": {
    "emitEntryPoint": true,
    "preserveCompilationContext": true,
    "define": [ "DEMO", "TESTING", "USE_REDIS_CACHE" ]
  },
```
https://github.com/SteeltoeOSS/musicStore-config.git

Each of the `push*.*` scripts `dotnet publish` the MusicStore service targeting the `framework` and `runtime` you specify.  They then push the MusicStore service using the appropriate CloudFoundry manifest found in the projects directory (e.g. `manifest-windows.yml`, `manifest.yml` ). 

Note: If you are using self-signed certificates it is possible that you might run into SSL certificate validation issues when pushing these apps. The simplest way to fix this:

1. Disable certificate validation for the Spring Cloud Config Client.  You can do this by editing `appsettings.json` and add `spring:cloud:config:validate_certificates=false`. You will need to do this for each of the applications.

# Known Limitations

## Sample Databases
All MusicStore services (i.e. MusicStoreUI, OrderService, etc.) have their own database instance for persisting data.  When a MusicStore service is started locally, it will always drop and recreate its database upon startup. When a MusicStore service is started on CloudFoundry, only the first instance (i.e. CF_INSTANCE_INDEX=0) will drop and recreate its database.  Note then, the service is not fully ready until the first instance has finished initializing its database, even though other instances are ready.
