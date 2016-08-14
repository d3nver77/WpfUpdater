using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UpdateCreator.Models;

namespace UpdateCreator.Helpers
{
    public class DragDropManager
    {
        public FrameworkElement DragDropSource
        {
            get { return this._dragDropSource; }
            set
            {
                if (ReferenceEquals(this._dragDropSource, value))
                {
                    return;
                }
                if (this._dragDropSource != null)
                {
                    this._dragDropSource.PreviewMouseLeftButtonDown -= this.PreviewMouseLeftButtonDown;
                    this._dragDropSource.PreviewMouseMove -= this.PreviewMouseMove;
                }
                this._dragDropSource = value;
                if (this._dragDropSource != null)
                {
                    this._dragDropSource.PreviewMouseLeftButtonDown += this.PreviewMouseLeftButtonDown;
                    this._dragDropSource.PreviewMouseMove += this.PreviewMouseMove;
                }
            }
        }

        public FrameworkElement DragDropTarget
        {
            get { return this._dragDropTarget; }
            set
            {
                if (ReferenceEquals(this._dragDropTarget, value))
                {
                    return;
                }
                if (this._dragDropTarget != null)
                {
                    this._dragDropTarget.PreviewDragEnter -= this.PreviewDrag;
                    this._dragDropTarget.DragEnter -= this.Drag;
                    this._dragDropTarget.PreviewDragOver -= this.PreviewDrag;
                    this._dragDropTarget.DragOver -= this.Drag;
                    this._dragDropTarget.Drop -= this.Drop;
                }
                this._dragDropTarget = value;
                if (this._dragDropTarget != null)
                {
                    this._dragDropTarget.PreviewDragEnter += this.PreviewDrag;
                    this._dragDropTarget.DragEnter += this.Drag;
                    this._dragDropTarget.PreviewDragOver += this.PreviewDrag;
                    this._dragDropTarget.DragOver += this.Drag;
                    this._dragDropTarget.Drop += this.Drop;
                }
            }
        }

        private static DragDropManager _instance;
        private FrameworkElement _dragDropSource;
        private FrameworkElement _dragDropTarget;
        private Point _startPoint;
        private bool _isDragDropStarted;
        private readonly string _dragDataName = "DragData";
        

        public static DragDropManager Instance
        {
            get { return _instance ?? (_instance = new DragDropManager()); }
        }

        private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._startPoint = e.GetPosition(null);
        }

        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = this._startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed
                && e.OriginalSource is FrameworkElement
                && !this._isDragDropStarted
                && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                )
            {
                var listView = sender as ListView;
                if (listView == null)
                {
                    return;
                }
                var listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listViewItem == null)
                {
                    return;
                }

                var file = (IDragable)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                if (!file.IsDragable)
                {
                    return;
                }

                DataObject dragData = new DataObject(this._dragDataName, file);
                this._isDragDropStarted = true;
                DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
                this._isDragDropStarted = false;
            }
        }

        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private static T FindViewModel<T>(DependencyObject current) where T : class 
        {
            do
            {
                if ((current as FrameworkElement)?.DataContext is T)
                {
                    return (current as FrameworkElement).DataContext as T;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private void PreviewDrag(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(this._dragDataName))
            {
                e.Handled = true;
            }
        }

        private void Drag(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(this._dragDataName))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(this._dragDataName))
            {
                return;
            }
            var filePropertiesViewModel = FindViewModel<IFileProperties>((DependencyObject)sender);
            if (filePropertiesViewModel != null)
            {
                filePropertiesViewModel.SelectedFile = (IDragable)e.Data.GetData(this._dragDataName);
            }
            e.Handled = true;
        }
    }
}