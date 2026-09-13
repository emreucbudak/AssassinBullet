using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;

namespace AssassinBullet.ViewModels;

public abstract partial class BlockEditorViewModelBase : ViewModelBase
{
    private ConfigBlockViewModel? _selectedBlock;
    private readonly HashSet<ConfigBlockViewModel> _observedBlocks = [];
    public ObservableCollection<ConfigBlockViewModel> Blocks { get; } = [];
    public bool HasBlocks => Blocks.Count > 0;
    public bool HasSelection => SelectedBlock is not null;
    public ConfigBlockViewModel? SelectedBlock
    {
        get => _selectedBlock;
        set
        {
            if (!SetProperty(ref _selectedBlock, value)) return;
            OnPropertyChanged(nameof(HasSelection));
            OnSelectionChanged();
            RefreshCommands();
        }
    }

    protected BlockEditorViewModelBase()
    {
        Blocks.CollectionChanged += (_, _) =>
        {
            foreach (var removed in _observedBlocks.Where(block => !Blocks.Contains(block)).ToArray())
            {
                removed.PropertyChanged -= BlockChanged;
                _observedBlocks.Remove(removed);
            }
            foreach (var added in Blocks.Where(block => !_observedBlocks.Contains(block)))
            {
                added.PropertyChanged += BlockChanged;
                _observedBlocks.Add(added);
            }
            if (SelectedBlock is not null && !Blocks.Contains(SelectedBlock)) SelectedBlock = null;
            OnPropertyChanged(nameof(HasBlocks));
            OnBlocksChanged();
            RefreshCommands();
        };
    }

    private void BlockChanged(object? sender, PropertyChangedEventArgs e) =>
        OnBlockChanged((ConfigBlockViewModel)sender!, e);
    protected virtual void OnBlockChanged(ConfigBlockViewModel block, PropertyChangedEventArgs e) { }
    protected virtual void OnBlocksChanged() { }
    protected virtual void OnSelectionChanged() { }
    public abstract void ShowStatus(string message);
    protected bool CanEdit() => SelectedBlock is not null && Blocks.Contains(SelectedBlock);
    private bool CanMoveUp() => CanEdit() && Blocks.IndexOf(SelectedBlock!) > 0;
    private bool CanMoveDown() => CanEdit() && Blocks.IndexOf(SelectedBlock!) < Blocks.Count - 1;

    protected virtual void RefreshCommands()
    {
        RemoveBlockCommand.NotifyCanExecuteChanged();
        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void RemoveBlock()
    {
        if (!CanEdit()) return;
        var index = Blocks.IndexOf(SelectedBlock!);
        Blocks.RemoveAt(index);
        SelectedBlock = Blocks.Count == 0 ? null : Blocks[Math.Min(index, Blocks.Count - 1)];
        ShowStatus("Blok silindi.");
    }

    [RelayCommand(CanExecute = nameof(CanMoveUp))]
    private void MoveUp() => MoveSelected(-1);
    [RelayCommand(CanExecute = nameof(CanMoveDown))]
    private void MoveDown() => MoveSelected(1);

    private void MoveSelected(int offset)
    {
        if (!CanEdit()) return;
        var selected = SelectedBlock!;
        var index = Blocks.IndexOf(selected);
        if (index + offset < 0 || index + offset >= Blocks.Count) return;
        Blocks.Move(index, index + offset);
        SelectedBlock = selected;
        RefreshCommands();
        ShowStatus("Blok sırası güncellendi.");
    }
}
