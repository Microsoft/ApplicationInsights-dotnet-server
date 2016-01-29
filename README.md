![Build Status](https://mseng.visualstudio.com/DefaultCollection/_apis/public/build/definitions/96a62c4a-58c2-4dbb-94b6-5979ebc7f2af/2678/badge)

# Visual Studio Application Insights SDK for .NET Web Applications

The code in this repository is the .NET web application SDK for Application Insights. [Application Insights][AILandingPage] is a service that lets you monitor your live application's performance and usage. This SDK sends telemetry to the service. It collects data such as web request timings and succcess rates, dependency calls, exceptions, and server performance counters. You can also use the SDK to send your own telemetry and add modules to collect logs. You can use this SDK in any .NET web application, hosted either on your own servers or on Microsoft Azure.

## Get the SDK

The SDK is installed on each project by the Application Insights tools in Visual Studio (2013 and later).

To [add Application Insights to your project in Visual Studio][AddInVS]:

* If you're creating a new project, check **Add Application Insights** in the New Project dialog.
* If it's an existing project, right-click your project in Solution Explorer and select **Add Application Insights** or **Update Application Insights**.
* If these options aren't available for your project type, use Extension Manager in Visual Studio to install or update the NuGet package. Create a [new Application Insights resource][CreateResource] in the Azure portal, obtain its instrumentation key, and insert that in ApplicationInsights.config.

Run your project, and then [open your Application Insights resource][WebDocumentation] in the [Azure Preview Portal][AzurePortal] and look for events.

## To build

* Visual Studio 2015 Community or Enterprise
* Clone the Git repository
* Open Visual Studio solution (devenv Web\Microsoft.ApplicationInsights.Web.sln)
* Build solution in Visual Studio

If you prefer using build scripts, run ```buildDebug.cmd``` or ```buildRelease.cmd```

## Branches
- [master][master] contains the *latest* published release located on [NuGet][WebNuGet].
- [develop][develop] contains the code for the *next* release.

## Contributing

We strongly welcome and encourage contributions to this project. Please read the [contributor's guide][ContribGuide]. If making a large change we request that you open an [issue][GitHubIssue] first. If we agree that an issue is a bug, we'll add the "bug" label, and issues that we plan to fix are labeled with an iteration number. We follow the [Git Flow][GitFlow] approach to branching.


[Azure]: https://azure.com/
[AILandingPage]: http://azure.microsoft.com/services/application-insights/
[AzurePortal]: https://portal.azure.com/
[WebDocumentation]: https://azure.microsoft.com/documentation/articles/app-insights-asp-net/#monitor
[master]: https://github.com/Microsoft/ApplicationInsights-server-dotnet/tree/master/
[develop]: https://github.com/Microsoft/ApplicationInsights-server-dotnet/tree/develop/
[GitFlow]: http://nvie.com/posts/a-successful-git-branching-model/
[ContribGuide]: https://github.com/Microsoft/ApplicationInsights-server-dotnet/blob/develop/CONTRIBUTING.md/
[GitHubIssue]: https://github.com/Microsoft/ApplicationInsights-server-dotnet/issues/
[WebNuGet]: https://www.nuget.org/packages/Microsoft.ApplicationInsights.Web/
[MyGet]:http://myget.org/gallery/applicationinsights/
[AddInVS]:https://azure.microsoft.com/documentation/articles/app-insights-asp-net/#ide
[CreateResource]: https://azure.microsoft.com/documentation/articles/app-insights-create-new-resource/

