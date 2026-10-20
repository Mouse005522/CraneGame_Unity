namespace Kun.Tool
{
    /// <summary>
    /// 有name當作key的物件
    /// </summary>
    public interface IKeyable
	{
		string ItemKey{ get; set;}
	}

	public interface ITableable<T>
	{
		string Key { get;}

		T Item { get; }
	}
}