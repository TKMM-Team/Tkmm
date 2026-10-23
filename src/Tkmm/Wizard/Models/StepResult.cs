namespace Tkmm.Wizard.Models;

public readonly record struct StepResult(bool WentBack, string? NextStepId)
{
    public static StepResult Back() => new(true, null);
    public static StepResult Next(string stepId) => new(false, stepId);
    public static StepResult Done() => new(false, WizardSteps.Done);
}