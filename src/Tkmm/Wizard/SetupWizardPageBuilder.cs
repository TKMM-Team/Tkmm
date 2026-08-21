using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Projektanker.Icons.Avalonia;

namespace Tkmm.Wizard;

public class SetupWizardPageBuilder(ContentPresenter presenter, bool isFirstPage = false)
{
    private readonly SetupWizardPage _page = new(isFirstPage);
    private StackPanel? _mainPanel;
    private StackPanel? _footerPanel;

    public SetupWizardPageBuilder WithTitle(TkLocale title) => WithTitle(Locale[title]);

    public SetupWizardPageBuilder WithTitle(string title)
    {
        _page.Title = title;
        return this;
    }

    public SetupWizardPageBuilder WithContent(TkLocale content) => WithContent(Locale[content]);

    public SetupWizardPageBuilder WithContent(object? content)
    {
        if (content is StyledElement { Parent: ContentControl parent }) {
            parent.Content = null;
        }
        
        ResetComposable();
        _page.Content = content;
        return this;
    }

    public SetupWizardPageBuilder WithContent<TControl>(object? context = null) where TControl : Control, new()
    {
        ResetComposable();
        _page.Content = new TControl { DataContext = context };
        return this;
    }

    public SetupWizardPageBuilder WithActionContent(TkLocale content) => WithActionContent(Locale[content]);

    public SetupWizardPageBuilder WithActionContent(object? content)
    {
        _page.ActionContent = content;
        return this;
    }

    public SetupWizardPageBuilder BeginContent(double spacing = 8)
    {
        _mainPanel = new StackPanel { Spacing = spacing };
        _footerPanel = new StackPanel { Spacing = spacing, Margin = new Thickness(0, 8, 0, 0) };
        var root = new Grid { RowDefinitions = new RowDefinitions("*,Auto") };
        Grid.SetRow(_mainPanel, 0);
        Grid.SetRow(_footerPanel, 1);
        root.Children.Add(_mainPanel);
        root.Children.Add(_footerPanel);
        _page.Content = root;
        return this;
    }

    public SetupWizardPageBuilder AddText(
        string text,
        Thickness? margin = null,
        FontStyle fontStyle = FontStyle.Normal,
        IBrush? foreground = null)
    {
        EnsureContent();
        TextBlock block = new() {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            FontStyle = fontStyle,
            Margin = margin ?? default
        };
        if (foreground is not null) {
            block.Foreground = foreground;
        }

        _mainPanel!.Children.Add(block);
        return this;
    }

    public SetupWizardPageBuilder AddControl(Control control)
    {
        EnsureContent();
        _mainPanel!.Children.Add(control);
        return this;
    }

    public void AddRadioGroup(string groupName, IReadOnlyList<WizardRadioOption> options)
    {
        EnsureContent();
        foreach (var option in options) {
            RadioButton radio = new() {
                GroupName = groupName,
                Content = option.Content,
                IsEnabled = option.IsEnabled,
                IsVisible = option.IsVisible,
                DataContext = option
            };
            radio.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(WizardRadioOption.IsSelected)) {
                Mode = BindingMode.TwoWay
            });
            _mainPanel!.Children.Add(radio);
        }
    }

    public SetupWizardPageBuilder AddPathField(WizardPathField field)
    {
        EnsureContent();
        StackPanel block = new() { Spacing = 5 };
        if (!string.IsNullOrEmpty(field.Header)) {
            block.Children.Add(new TextBlock {
                Text = field.Header,
                Margin = new Thickness(5, 0, 0, 0)
            });
        }

        Grid row = new() {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            DataContext = field
        };

        TextBox textBox = new() {
            Watermark = field.Watermark,
            TextWrapping = TextWrapping.NoWrap
        };
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(WizardPathField.Text)) {
            Mode = BindingMode.TwoWay
        });
        Grid.SetColumn(textBox, 0);
        row.Children.Add(textBox);

        Button browse = new() {
            Margin = new Thickness(5, 0, 0, 0),
            Width = 32,
            Height = 32,
            Padding = new Thickness(4),
            Content = new Icon { Value = "fa-regular fa-folder-open" }
        };
        browse.Click += async (_, _) => {
            if (await BrowseAsync(field.Browse) is { } path) {
                field.Text = path;
            }
        };
        Grid.SetColumn(browse, 1);
        row.Children.Add(browse);

        block.Children.Add(row);
        _mainPanel!.Children.Add(block);
        return this;
    }

    public void WithMutedFooter(string text)
    {
        EnsureContent();
        _footerPanel!.Children.Add(new TextBlock {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            FontStyle = FontStyle.Italic,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 8, 0, 0)
        });
    }

    public ValueTask<bool> Show() => _page.Show(presenter);

    private void EnsureContent()
    {
        if (_mainPanel is null) {
            BeginContent();
        }
    }

    private void ResetComposable()
    {
        _mainPanel = null;
        _footerPanel = null;
    }

    private static async Task<string?> BrowseAsync(WizardBrowseOptions options)
        => await App.XamlRoot.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = options.Title,
            AllowMultiple = options.AllowMultiple
        }) switch {
            [var target] => target.TryGetLocalPath(),
            _ => null
        };
}

public sealed partial class WizardRadioOption : ObservableObject
{
    public required string Content { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public bool IsEnabled { get; init; } = true;
    public bool IsVisible { get; init; } = true;
    public object? Tag { get; init; }
}

public sealed class WizardBrowseOptions
{
    public string? Title { get; init; }
    public bool AllowMultiple { get; init; }
}

public sealed partial class WizardPathField : ObservableObject
{
    public string? Header { get; init; }

    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    public string? Watermark { get; init; }
    public required WizardBrowseOptions Browse { get; init; }
}