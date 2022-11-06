namespace GitAlpha.Extensions;

public static class CollectionExtensions
{
	public static void InsertOrAdd<T>(this List<T> list, int index, T item)
	{
		if (index < list.Count)
			list.Insert(index, item);
		else
		{
			list.Add(item);
		}
	}
}
