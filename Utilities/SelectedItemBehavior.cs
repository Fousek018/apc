using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace LABPOWER_APC.Utilities
{
    public class SelectedItemsBehavior : Behavior<ListView>
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(SelectedItemsBehavior), new PropertyMetadata(null));

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.SelectionChanged += OnSelectionChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItems != null)
            {
                SelectedItems.Clear();
                foreach (var item in AssociatedObject.SelectedItems)
                {
                    SelectedItems.Add(item);
                }
            }
        }
    }
}
