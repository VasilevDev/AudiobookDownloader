namespace AudiobookDownloader.Logging
{
	public interface ICustomLogger
	{
		void Log(string text);
		void Debug(string text);
		void Error(string text);
		void Success(string text);
		void Warning(string text);
		void Clear();
	}
}
