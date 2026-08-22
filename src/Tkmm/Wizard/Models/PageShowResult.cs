namespace Tkmm.Wizard.Models;

public readonly record struct PageShowResult(bool Next, WizardRadioOption? Selected = null, string? Path = null)
{
    public void Deconstruct(out bool next, out WizardRadioOption? selected)
        => (next, selected) = (Next, Selected);

    public static implicit operator bool(PageShowResult result) => result.Next;
}
