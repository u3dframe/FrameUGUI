using System.IO;
/// <summary>
/// 类名 : 路径工具
/// 作者 : Canyon
/// 日期 : 2016-12-23 15:20:00
/// 功能 : 
/// </summary>
namespace Toolkits
{
	public static class PathEx
	{
		// 取得文件分隔符"/"
		static public readonly char folderSeparator = Path.DirectorySeparatorChar;
        
		// 取得路径分隔符";"
		static public readonly char pathSeparator = Path.PathSeparator;

		// 文件的目录路径
		static public string getFolderPath (string fn)
		{
			return Path.GetDirectoryName (fn);
		}

		// 扩展名 Suffix(包含.号)
		static public string getSuffix (string fn)
		{
			return Path.GetExtension (fn);
		}

		static public string getSuffixToLower (string fn)
		{
			return getSuffix (fn).ToLower ();
		}

		// 文件名字(含有扩展名)
		static public string getFileName (string fn)
		{
			return Path.GetFileName (fn);
		}

        // 文件名字(不含扩展名)
        static public string getFileNameNoSuffix(string fn)
        {
            return Path.GetFileNameWithoutExtension(fn);
        }

		// 文件全路径
		static public string getFullPath (string fn)
		{
			return Path.GetFullPath (fn);
		}
	}
}
