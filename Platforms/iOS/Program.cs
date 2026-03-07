using ObjCRuntime;
using UIKit;

namespace WeeklyTimetable;

public class Program
{
	/// <summary>
	/// Application entry point for iOS; boots UIKit with the configured app delegate.
	/// </summary>
	/// <param name="args">Process command-line arguments.</param>
	/// <returns>None.</returns>
	static void Main(string[] args)
	{
		// if you want to use a different Application Delegate class from "AppDelegate"
		// you can specify it here.
		UIApplication.Main(args, null, typeof(AppDelegate));
	}
}
