using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace chatz.client
{
  public class LocalTimeFromUtcConverter : IValueConverter
  {
    public Object Convert (Object       value
                          ,Type         targetType
                          ,Object       parameter
                          ,CultureInfo  culture)
    {
      if (value == null) return null;

      if (value is DateTime)
      {
        var dt = (DateTime) value;
        var lt = TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.Local);
        return DateTime.SpecifyKind(lt, DateTimeKind.Local);
      }
      
      return value.ToString();
    }

    public Object ConvertBack (Object       value
                              ,Type         targetType
                              ,Object       parameter
                              ,CultureInfo  culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }
}
