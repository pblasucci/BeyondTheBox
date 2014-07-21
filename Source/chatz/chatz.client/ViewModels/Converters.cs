using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace chatz.client
{
  // A helper to localize the timestamp of received messages
  public class LocalTimeFromUtcConverter : IValueConverter
  {
    // Attempts to convert UTC DateTime to a localize value;
    // Falls back to the string representation of the given value
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

    // Not implemented
    public Object ConvertBack (Object       value
                              ,Type         targetType
                              ,Object       parameter
                              ,CultureInfo  culture)
    {
      return DependencyProperty.UnsetValue;
    }
  }
}
