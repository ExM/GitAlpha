using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using GitAlpha.Git;

namespace GitAlpha.Avalonia.Converters;

public class ObjectIdRenderer: IValueConverter
{
	private int _length = 4;

	public int Length
	{
		get => _length;
		set
		{
			if (value < 1)
				value = 1;
			if (value > ObjectId.Sha1CharCount)
				value = ObjectId.Sha1CharCount;
			
			_length = value;
		}
	}
	
	public string Render(ObjectId id) => id.ToShortString(_length);

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is ObjectId id &&
		    targetType.IsAssignableTo(typeof(string)))
		{
			return Render(id);
		}
		
		return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}

