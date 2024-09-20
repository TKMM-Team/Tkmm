namespace Tkmm.Core.Localization;

public class StringResources_Status
{
    private const string GROUP = "Status";

    public string Ready { get; } = GetStringResource(GROUP, nameof(Ready));
}
