using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Tkmm.Models
{
    public class OptionModel : ObservableObject
    {
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public List<string> Values { get; set; }
        public List<string> NameValues { get; set; } // Friendly names for dropdown display.

        private string selectedValue;
        public string SelectedValue
        {
            get => selectedValue;
            set => SetProperty(ref selectedValue, value);
        }

        private int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (SetProperty(ref selectedIndex, value))
                {
                    if (Class?.ToLowerInvariant() == "dropdown" && value >= 0 && value < Values.Count)
                    {
                        SelectedValue = Values[value];
                        OnPropertyChanged(nameof(DisplaySelectedValue));
                    }
                }
            }
        }

        /// <summary>
        /// Computed property used for UI display.
        /// If NameValues is available, show the friendly name based on SelectedIndex; otherwise, show SelectedValue.
        /// </summary>
        public string DisplaySelectedValue =>
            (Class?.ToLowerInvariant() == "dropdown" && NameValues != null && NameValues.Count > SelectedIndex)
                ? NameValues[SelectedIndex]
                : SelectedValue;

        public string Class { get; set; }
        public string Section { get; set; }
        public List<string> ConfigClass { get; set; }
        public double Increments { get; set; }
        public bool Auto { get; set; }

        /// <summary>
        /// Computed property to choose between NameValues and Values for display.
        /// </summary>
        public IEnumerable<string> DisplayValues => (NameValues?.Count ?? 0) > 0 ? NameValues : Values;
    }
} 