# JBoss 7 Standalone As Windows Service

This is a simple Windows Service built in C# that manages the **JBoss 7 Standalone Server** in terms of starting and shutting down the server.

# Requirements

- Define an environment variable called `JBOSS_HOME` that holds the absolute path where JBoss is installed (click [here](https://docs.jboss.org/jbossas/docs/Installation_Guide/beta422/html/setting_JBOSS_HOME_windows.html) for see how to setup). This variable it's used by the service for resolve the path of the batch files `standalone.bat` and `jboss-cli.bat`.

- Ensure you have the command line utility `Installutil` which comes with .NET framework.

## Where you find **Installutil** tool in your computer?

You can find it in this path:

```
%windir%\Microsoft.NET\Framework[64]\<dotnet_framework_version>
```

Where `windir` is a system environment variable that points to the Windows OS files, and `dotnet_framework_version` is the .NET framework version installed in your machine.

# Installation

Clone this repository, and build it in your machine. Open a command prompt as Administrator, navigate to the `JBoss` project compilation output folder: `\Jboss\bin\{Debug | Release}` (you can enter either `Debug` or `Release` based on your build configuration), and run the command below to install the service in your machine:

```console
> installutil JBoss.exe
```

By the way, I don't recommend register the service from the assembly located in your `bin` folder. Instead, copy the assembly and store it in a new folder where you can isolate your service development. 

In case you want to uninstall, ensure the service is not running, and run the command below:

```console
> installutil /u JBoss.exe
```

## How the service works internally?

Once you have installed the service, you're able to start using right away! :smiley: Check below what happen when you start or stop the service:

- When the service is starting, it's invoking the `standalone.bat` batch file for start `JBoss` in the background.
- When the service is being stoped, it's calling the `jboss-cli.bat` batch file with the arguments `--connect command=:shutdown` that sends a signal to the instance running of `JBoss` to shutdown.

## Service troubleshooting

If you face any problem when either starting or stoping the service, please refer to the Windows logs for more details.

# Issues

If you encounter any issue while using it, don't doubt in creating an issue over here. I'd appreciate deliver its solution the soon as possible! (and also get help from the community :grin:).

## Known issues

- In case the `jboss-cli.bat` file isn't found, the `JBoss` can't be stopped even though the main process is killed (the process that started the `JBoss` when the service itself began).
