using Voxera.Domain.Enums;

namespace Voxera.Domain.Entities;

public class IvrMenu : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? GreetingAudioPath { get; private set; }
    public string? GreetingText { get; private set; }  // TTS fallback
    public int Timeout { get; private set; } = 5;
    public int MaxRetries { get; private set; } = 3;
    public bool IsActive { get; private set; } = true;
    public string? TimeZone { get; private set; } = "Europe/Istanbul";

    // Navigation
    public Company? Company { get; private set; }
    public ICollection<IvrOption> Options { get; private set; } = new List<IvrOption>();
    public ICollection<IvrSchedule> Schedules { get; private set; } = new List<IvrSchedule>();

    protected IvrMenu() { }

    public static IvrMenu Create(Guid companyId, string name, string? greetingText = null)
    {
        return new IvrMenu { CompanyId = companyId, Name = name, GreetingText = greetingText };
    }
}

public class IvrOption : BaseEntity
{
    public Guid IvrMenuId { get; private set; }
    public string Digit { get; private set; } = string.Empty;  // 0-9, *, #
    public string Description { get; private set; } = string.Empty;
    public IvrActionType ActionType { get; private set; }
    public string? ActionTarget { get; private set; }  // Extension, Queue ID, etc.
    public int SortOrder { get; private set; }

    public IvrMenu? IvrMenu { get; private set; }

    protected IvrOption() { }

    public static IvrOption Create(Guid ivrMenuId, string digit, string description, IvrActionType actionType, string? target)
    {
        return new IvrOption { IvrMenuId = ivrMenuId, Digit = digit, Description = description, ActionType = actionType, ActionTarget = target };
    }
}

public class IvrSchedule : BaseEntity
{
    public Guid IvrMenuId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DayOfWeek[] ActiveDays { get; private set; } = Array.Empty<DayOfWeek>();
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public IvrActionType ActionType { get; private set; }
    public string? ActionTarget { get; private set; }
    public bool IsHoliday { get; private set; } = false;

    public IvrMenu? IvrMenu { get; private set; }

    protected IvrSchedule() { }
}
