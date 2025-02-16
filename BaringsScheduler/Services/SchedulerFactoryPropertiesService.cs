namespace BaringsScheduler.Services;

using System.Collections.Specialized;

internal sealed class SchedulerFactoryPropertiesService
{
    internal static NameValueCollection GetFactoryProperties(string quartzDatabaseConnectionString) =>
        new()
        {
            ["quartz.jobStore.useProperties"] = "true",
            ["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
            ["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz",
            ["quartz.jobStore.tablePrefix"] = "QRTZ_",
            ["quartz.jobStore.dataSource"] = "myDS",
            ["quartz.serializer.type"] = "newtonsoft",
            ["quartz.dataSource.myDS.connectionString"] = quartzDatabaseConnectionString,
            ["quartz.dataSource.myDS.provider"] = "SqlServer",
            ["quartz.threadPool.maxConcurrency"] = "10"
        };
}
