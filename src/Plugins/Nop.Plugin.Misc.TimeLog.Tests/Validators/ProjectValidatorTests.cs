using Moq;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Plugin.Misc.TimeLog.Validators;
using Nop.Services.Localization;
using Xunit;

namespace Nop.Plugin.Misc.TimeLog.Tests.Validators;

/// <summary>
/// Unit tests for <see cref="ProjectValidator"/> (TT-048) - covers all 4 field rules (TT-032/T-023):
/// Name required, StartDate required, EndDate must be on/after StartDate when present, StatusId must
/// be a defined <see cref="ProjectStatus"/> value (including a crafted out-of-range integer).
/// </summary>
public class ProjectValidatorTests
{
    private static ProjectValidator CreateValidator()
    {
        var localizationServiceMock = new Mock<ILocalizationService>();
        localizationServiceMock
            .Setup(x => x.GetResourceAsync(It.IsAny<string>()))
            .ReturnsAsync((string key) => key);

        return new ProjectValidator(localizationServiceMock.Object);
    }

    private static Project CreateValidProject()
    {
        return new Project
        {
            Name = "A project",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = null,
            StatusId = (int)ProjectStatus.NotStarted
        };
    }

    [Fact]
    public async Task ValidateAsync_AllFieldsValid_PassesValidation()
    {
        var validator = CreateValidator();
        var project = CreateValidProject();

        var result = await validator.ValidateAsync(project);

        Assert.True(result.IsValid);
    }

    #region Rule 1: Name required

    [Fact]
    public async Task ValidateAsync_NameEmpty_FailsNameRequired()
    {
        var validator = CreateValidator();
        var project = CreateValidProject();
        project.Name = string.Empty;

        var result = await validator.ValidateAsync(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Project.Name));
    }

    [Fact]
    public async Task ValidateAsync_NameNull_FailsNameRequired()
    {
        var validator = CreateValidator();
        var project = CreateValidProject();
        project.Name = null;

        var result = await validator.ValidateAsync(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Project.Name));
    }

    #endregion

    #region Rule 2: StartDate required

    [Fact]
    public async Task ValidateAsync_StartDateDefault_FailsStartDateRequired()
    {
        var validator = CreateValidator();
        var project = CreateValidProject();
        project.StartDate = default;

        var result = await validator.ValidateAsync(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Project.StartDate));
    }

    #endregion

    #region Rule 3: EndDate >= StartDate when present

    [Fact]
    public async Task ValidateAsync_EndDateBeforeStartDate_FailsValidation()
    {
        var validator = CreateValidator();
        var project = CreateValidProject();
        project.StartDate = new DateTime(2026, 2, 1);
        project.EndDate = new DateTime(2026, 1, 31);

        var result = await validator.ValidateAsync(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Project.EndDate));
    }

    [Fact]
    public async Task ValidateAsync_EndDateEqualsStartDate_PassesValidation()
    {
        var validator = CreateValidator();
        var project = CreateValidProject();
        project.StartDate = new DateTime(2026, 2, 1);
        project.EndDate = new DateTime(2026, 2, 1);

        var result = await validator.ValidateAsync(project);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_EndDateNull_PassesValidation()
    {
        var validator = CreateValidator();
        var project = CreateValidProject();
        project.EndDate = null;

        var result = await validator.ValidateAsync(project);

        Assert.True(result.IsValid);
    }

    #endregion

    #region Rule 4: StatusId must be a defined ProjectStatus value

    [Fact]
    public async Task ValidateAsync_StatusIdOutOfRange_FailsStatusInvalid()
    {
        var validator = CreateValidator();
        var project = CreateValidProject();
        //crafted request - not one of the 6 defined ProjectStatus ordinals (0-5)
        project.StatusId = 999;

        var result = await validator.ValidateAsync(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Project.StatusId));
    }

    [Theory]
    [InlineData(ProjectStatus.NotStarted)]
    [InlineData(ProjectStatus.Inprogress)]
    [InlineData(ProjectStatus.OnHold)]
    [InlineData(ProjectStatus.Completed)]
    [InlineData(ProjectStatus.Cancelled)]
    [InlineData(ProjectStatus.Retired)]
    public async Task ValidateAsync_EachDefinedStatus_PassesValidation(ProjectStatus status)
    {
        var validator = CreateValidator();
        var project = CreateValidProject();
        project.Status = status;

        var result = await validator.ValidateAsync(project);

        Assert.True(result.IsValid);
    }

    #endregion
}
