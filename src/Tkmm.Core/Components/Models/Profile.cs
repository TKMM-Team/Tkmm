﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Tkmm.Core.Components.Models;

public partial class Profile(string name) : ObservableObject
{
    [ObservableProperty]
    private string _name = name;

    public ObservableCollection<ProfileMod> Mods { get; } = [];

    [JsonConstructor]
    public Profile(string name, ObservableCollection<ProfileMod> mods) : this(name)
    {
        Mods = mods;
    }
}