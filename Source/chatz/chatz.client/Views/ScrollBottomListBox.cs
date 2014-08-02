using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;

namespace chatz.client
{
  public class ScrollBottomListBox : ListBox
  {
    protected override void OnItemsChanged (NotifyCollectionChangedEventArgs e)
    {
      base.OnItemsChanged (e);

      if (e.NewItems != null && e.NewItems.Count > 0) 
      {
        UpdateLayout();
        Items.MoveCurrentToLast();
        ScrollIntoView(Items.CurrentItem);
        UpdateLayout();
      }
    }
  }
}
