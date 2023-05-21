using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using GitAlpha.Git;

namespace MvvmDemo;

public class ObjectIdRenderer: IValueConverter
{
	public static readonly ObjectIdRenderer Instance = new();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		//return value?.ToString() + " " + parameter?.ToString();

		if (value is ObjectId id &&
		    parameter is string targetSize &&
		    targetType.IsAssignableTo(typeof(string)))
		{
			return id.ToShortString(int.Parse(targetSize));
		}
		
		return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}

