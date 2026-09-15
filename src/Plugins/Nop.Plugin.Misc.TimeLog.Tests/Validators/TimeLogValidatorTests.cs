using Moq;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Plugin.Misc.TimeLog.Services;
using Nop.Plugin.Misc.TimeLog.Validators;
using Nop.Services.Localization;
using Xunit;
using TimeLogEntity = Nop.Plugin.Misc.TimeLog.Domain.TimeLog;

namespace Nop.Plugin.Misc.TimeLog.Tests.Validators;

/// <summary>
/// Unit tests for <see cref="TimeLogValidator"/> - covers all 4 field rules (TT-007) plus
/// the boundary values called out in TT-017: Time = 0, Time = 24, Time = -0.01, Time = 24.01,
/// and Project rejection when inactive or nonexistent.
/// </summary>
public class TimeLogValidatorTests
{
    private const int ActiveProjectId = 1;
    private const int InactiveProjectId = 2;
    private const int NonexistentProjectId = 999;

    private static TimeLogValidator CreateValidator()
    {
        var localizationServiceMock = new Mock<ILocalizationService>();
        localizationServiceMock
            .Setup(x => x.GetResourceAsync(It.IsAny<string>()))
            .ReturnsAsync((string key) => key);

        var projectServiceMock = new Mock<IProjectService>();
        projectServiceMock
            .Setup(x => x.GetProjectByIdAsync(ActiveProjectId))
            .ReturnsAsync(new Project { Id = ActiveProjectId, Name = "Active Project", Active = true });
        projectServiceMock
            .Setup(x => x.GetProjectByIdAsync(InactiveProjectId))
            .ReturnsAsync(new Project { Id = InactiveProjectId, Name = "Inactive Project", Active = false });
        projectServiceMock
            .Setup(x => x.GetProjectByIdAsync(NonexistentProjectId))
            .ReturnsAsync((Project)null);

        return new TimeLogValidator(localizationServiceMock.Object, projectServiceMock.Object);
    }

    private static TimeLogEntity CreateValidTimeLog()
    {
        return new TimeLogEntity
        {
            ProjectId = ActiveProjectId,
            Task = "Development",
            Time = 8m,
            Date = DateTime.UtcNow.Date
        };
    }

    [Fact]
    public async Task ValidateAsync_AllFieldsValid_PassesValidation()
    {
        var validator = CreateValidator();
        var timeLog = CreateValidTimeLog();

        var result = await validator.ValidateAsync(timeLog);

        Assert.True(result.IsValid);
    }

    #region Rule 1: Project required + must exist + must be Active

    [Fact]
    public async Task ValidateAsync_ProjectIdZero_FailsProjectRequired()
    {
        var validator = CreateValidator();
        var timeLog = CreateValidTimeLog();
        timeLog.ProjectId = 0;

        var result = await validator.ValidateAsync(timeLog);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TimeLogEntity.ProjectId));
    }

    [Fact]
    public async Task ValidateAsync_ProjectInactive_FailsValidation()
    {
        var validator = CreateValidator();
        var timeLog = CreateValidTimeLog();
        timeLog.ProjectId = InactiveProjectId;

        var result = await validator.ValidateAsync(timeLog);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TimeLogEntity.ProjectId));
    }

    [Fact]
    public async Task ValidateAsync_ProjectDoesNotExist_FailsValidation()
    {
        var validator = CreateValidator();
        var timeLog = CreateValidTimeLog();
        timeLog.ProjectId = NonexistentProjectId;

        var result = await validator.ValidateAsync(timeLog);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TimeLogEntity.ProjectId));
    }

    #endregion

    #region Rule 2: Task required, non-empty

    [Fact]
    public async Task ValidateAsync_TaskEmpty_FailsTaskRequired()
    {
        var validator = CreateValidator();
        var timeLog = CreateValidTimeLog();
        timeLog.Task = string.Empty;

        var result = await validator.ValidateAsync(timeLog);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TimeLogEntity.Task));
    }

    [Fact]
    public async Task ValidateAsync_TaskNull_FailsTaskRequired()
    {
        var validator = CreateValidator();
        var timeLog = CreateValidTimeLog();
        timeLog.Task = null;

        var result = await validator.ValidateAsync(timeLog);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TimeLogEntity.Task));
    }

    #endregion

    #region Rule 3: Time 0-24 inclusive

    [Theory]
    [InlineData(0)]
    [InlineData(24)]
    [InlineData(12.5)]
    public async Task ValidateAsync_TimeWithinInclusiveBoundaries_PassesValidation(double time)
    {
        var validator = CreateValidator();
        var timeLog = CreateValidTimeLog();
        timeLog.Time = (decimal)time;

        var result = await validator.ValidateAsync(timeLog);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_TimeJustBelowZero_FailsTimeOutOfRange()
    {
        var validator = CreateValidator();
        var timeLog = CreateValidTimeLog();
        timeLog.Time = -0.01m;

        var result = await validator.ValidateAsync(timeLog);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TimeLogEntity.Time));
    }

    [Fact]
    public async Task ValidateAsync_TimeJustAboveTwentyFour_FailsTimeOutOfRange()
    {
        var validator = CreateValidator();
        var timeLog = CreateValidTimeLog();
        timeLog.Time = 24.01m;

        var result = await validator.ValidateAsync(timeLog);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TimeLogEntity.Time));
    }

    #endregion

    #region Rule 4: Date required

    [Fact]
    public async Task ValidateAsync_DateDefault_FailsDateRequired()
    {
        var validator = CreateValidator();
        var timeLog = CreateValidTimeLog();
        timeLog.Date = default;

        var result = await validator.ValidateAsync(timeLog);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TimeLogEntity.Date));
    }

    #endregion
}
