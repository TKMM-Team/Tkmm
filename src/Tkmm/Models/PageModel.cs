using FluentAvalonia.UI.Controls;
using Tkmm.Components;

namespace Tkmm.Models;

public class PageModel
{
    public required Page Id { get; init; }
    
    public required string Title { get; set; }

    public object? Content { get; set; }

    public required Symbol Icon { get; set; }

    public string? Description { get; set; }

    public Action<object?>? OnPageFocused { get; init; }
}