using Bookmarkaa.Managers;
using Bookmarkaa.Models;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Bookmarkaa.Helpers.Controls;

public class ListViewEx : ListView
{
    private System.Windows.Point _dragStartPoint;
    private AdornerLayer? _adornerLayer;
    private DropIndicatorAdorner? _adorner;

    public bool IsSortingEnabled { get; set; } = false;

    public ListViewEx()
    {
        AllowDrop = true;
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        if (!IsSortingEnabled)
            return;

        _dragStartPoint = e.GetPosition(null);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        
        base.OnMouseMove(e);
        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        if (!IsSortingEnabled)
            return;

        Vector diff = _dragStartPoint - e.GetPosition(null);

        if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        if (SelectedItem != null)
        {
            DragDrop.DoDragDrop(this, SelectedItem, DragDropEffects.Move);
        }
    }

    protected override void OnDragOver(DragEventArgs e)
    {
        base.OnDragOver(e);

        if (!IsSortingEnabled)
            return;

        var item = GetItemUnderMouse(e.GetPosition(this));
        if (item == null)
            return;

        ShowIndicator(item, e.GetPosition(item));
    }

    protected override void OnDragLeave(DragEventArgs e)
    {
        base.OnDragLeave(e);
        RemoveIndicator();
    }

    protected override void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);

        RemoveIndicator();

        Bookmark? bookmark = (Bookmark?)e.Data.GetData(typeof(Bookmark));
        if (bookmark != null)
        { 
            OnDropItem(e, bookmark);
        }
        else if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            OnDropFile(e);
        }
    }

    protected void OnDropItem(DragEventArgs e, Bookmark bookmark)
    {
        if (ItemsSource is not IList list)
            return;

        var targetItem = GetItemUnderMouse(e.GetPosition(this));
        if (targetItem == null || bookmark == targetItem.DataContext)
            return;

        int fromIndex = list.IndexOf(bookmark);
        int toIndex = list.IndexOf(targetItem.DataContext);

        bool insertAfter = e.GetPosition(targetItem).Y > targetItem.ActualHeight / 2;
        if (insertAfter)
            toIndex++;

        if (fromIndex == toIndex || fromIndex + 1 == toIndex)
            return;

        list.Remove(bookmark);
        list.Insert(toIndex > fromIndex ? toIndex - 1 : toIndex, bookmark);

        SelectedItem = bookmark;
    }

    protected void OnDropFile(DragEventArgs e)
    {
        if (ItemsSource is not IList list)
            return;

        string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
        if (files != null && files.Length > 0)
        {
            string folder = FileExplorerProcess.GetFolderPath(files[0]);
            if (folder != string.Empty)
            {
                list.Add(new Bookmark(folder));
            }
        }
    }

    private ListViewItem? GetItemUnderMouse(Point position)
    {
        DependencyObject? element = InputHitTest(position) as DependencyObject;

        while (element != null && element is not ListViewItem)
        {
            try
            {
                element = VisualTreeHelper.GetParent(element);
            }
            catch
            {
                return null;
            }

        }

        return element as ListViewItem;
    }

    private void ShowIndicator(ListViewItem item, Point position)
    {
        RemoveIndicator();

        bool isAbove = position.Y < item.ActualHeight / 2;
        _adornerLayer = AdornerLayer.GetAdornerLayer(item);

        if (_adornerLayer == null)
            return;

        _adorner = new DropIndicatorAdorner(item, isAbove);
        _adornerLayer.Add(_adorner);
    }

    private void RemoveIndicator()
    {
        if (_adornerLayer != null && _adorner != null)
        {
            _adornerLayer.Remove(_adorner);
            _adorner = null;
        }
    }
}
