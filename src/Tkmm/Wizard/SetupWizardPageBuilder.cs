using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Projektanker.Icons.Avalonia;
using Tkmm.Wizard.Helpers;
using Tkmm.Wizard.Models;

namespace Tkmm.Wizard;

public class SetupWizardPageBuilder(ContentPresenter presenter, bool isFirstPage = false)
{
    private readonly SetupWizardPage _page = new(isFirstPage);
    private StackPanel? _mainPanel;
    private StackPanel? _footerPanel;
    private string? _description;
    private string? _note;
    private IReadOnlyList<WizardRadioOption>? _options;
    private string? _footer;
    private string? _groupName;
    private WizardPathField? _pathField;
    private Control? _control;

    public SetupWizardPageBuilder WithTitle(TkLocale title) => WithTitle(Locale[title]);
    public SetupWizardPageBuilder WithTitle(string title) { _page.Title = title; return this; }
    public SetupWizardPageBuilder WithDescription(TkLocale description) => WithDescription(Locale[description]);
    public SetupWizardPageBuilder WithDescription(string description) => Set(out _description, description);
    public SetupWizardPageBuilder WithNotes(TkLocale? note) => note is null ? this : WithNotes(Locale[note.Value]);
    public SetupWizardPageBuilder WithNotes(string note) => Set(out _note, note);
    public SetupWizardPageBuilder WithOptions(IReadOnlyList<WizardRadioOption> options) => Set(out _options, options);
    public SetupWizardPageBuilder WithGroupName(string groupName) => Set(out _groupName, groupName);
    public SetupWizardPageBuilder WithFooter(TkLocale footer) => WithFooter(Locale[footer]);
    public SetupWizardPageBuilder WithFooter(string footer) => Set(out _footer, footer);
    public SetupWizardPageBuilder WithControl(Control control) => Set(out _control, control);

    public SetupWizardPageBuilder WithFolderPicker(TkLocale browseTitle, string? initialPath = null, TkLocale? header = null)
        => WithFolderPicker(Locale[browseTitle], initialPath, header is { } h ? Locale[h] : null);

    public SetupWizardPageBuilder WithFolderPicker(string browseTitle, string? initialPath = null, string? header = null)
        => Set(out _pathField, new WizardPathField {
            Header = header,
            Text = initialPath ?? string.Empty,
            Browse = new WizardBrowseOptions { Title = browseTitle }
        });

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
        => WithContent(new TControl { DataContext = context });

    public SetupWizardPageBuilder WithActionContent(TkLocale content) => WithActionContent(Locale[content]);
    public SetupWizardPageBuilder WithActionContent(object? content) { _page.ActionContent = content; return this; }

    public async ValueTask<PageShowResult> Show()
    {
        if (!HasDeferredContent) {
            return new PageShowResult(await _page.Show(presenter));
        }
        
        ComposeDeferred();
        
        return await _page.Show(presenter)
            ? new PageShowResult(
                true,
                _options?.FirstOrDefault(o => o is { IsSelected: true, IsVisible: true, IsEnabled: true }),
                _pathField?.Text)
            : default;

    }

    private bool HasDeferredContent => _description is not null || _note is not null || _options is not null
           || _pathField is not null || _footer is not null || _control is not null;

    private void ComposeDeferred()
    {
        ArgumentException.ThrowIfNullOrEmpty(_page.Title);

        BeginContent();
        if (_description is not null) {
            AddText(_description, bottom: 10);
        }

        if (_note is not null) {
            AddText(_note, bottom: 8);
        }

        if (_options is not null) {
            ArgumentException.ThrowIfNullOrEmpty(_groupName);
            AddRadioGroup(_groupName, _options);
        }

        if (_pathField is not null) {
            AddPathField(_pathField);
        }

        if (_control is not null) {
            _mainPanel!.Children.Add(_control);
        }

        if (_footer is not null) {
            AddFooter(_footer);
        }
    }

    private void BeginContent(double spacing = 8)
    {
        _mainPanel = new StackPanel { Spacing = spacing };
        _footerPanel = new StackPanel { Spacing = spacing, Margin = new Thickness(0, 8, 0, 0) };
        var root = new Grid {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Children = { _mainPanel, _footerPanel }
        };
        Grid.SetRow(_footerPanel, 1);
        _page.Content = root;
    }

    private void AddText(string text, double bottom = 0)
    {
        _mainPanel!.Children.Add(new TextBlock {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, bottom)
        });
    }

    private void AddRadioGroup(string groupName, IReadOnlyList<WizardRadioOption> options)
    {
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

    private void AddPathField(WizardPathField field)
    {
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

        TextBox textBox = new() { Watermark = field.Watermark, TextWrapping = TextWrapping.NoWrap };
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(WizardPathField.Text)) { Mode = BindingMode.TwoWay });
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
            if (await WizardStorageHelper.BrowseAsync(field.Browse) is { } path) {
                field.Text = path;
            }
        };
        Grid.SetColumn(browse, 1);
        row.Children.Add(browse);

        block.Children.Add(row);
        _mainPanel!.Children.Add(block);
    }

    private void AddFooter(string text)
    {
        _footerPanel!.Children.Add(new TextBlock {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            FontStyle = FontStyle.Italic,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 8, 0, 0)
        });
    }

    private SetupWizardPageBuilder Set<T>(out T field, T value)
    {
        field = value;
        return this;
    }

    private void ResetComposable()
    {
        _mainPanel = null;
        _footerPanel = null;
    }
}