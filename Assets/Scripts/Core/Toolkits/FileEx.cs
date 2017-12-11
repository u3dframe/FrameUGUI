using UnityEngine;
using System.Collections;
using System.IO;
using System;

/// <summary>
/// 类名 : 文件读写工具
/// 作者 : Canyon
/// 日期 : 2016-12-23 15:20:00
/// 功能 :
/// </summary>
namespace Toolkits
{
	public static class FileEx
	{
		//  是否存在 目录
		static public bool isFolder (string fn)
		{
			return Directory.Exists (fn);
		}

		//  是否存在 文件
		static public bool isFile (string fn)
		{
			return File.Exists (fn);
		}

		// 拷贝文件
		static public void copy (string srcFile, string destFile)
		{
			File.Copy (srcFile, destFile, true);
		}

		// 删除文件
		static public void delFile (string fn)
		{
			File.Delete (fn);
		}

		// 删除文件夹
		static public void delFolder (string fn)
		{
			Directory.Delete (fn);
		}

		// 创建文件夹
		static public bool createFolder (string fn)
		{
			string folder = PathEx.getFolderPath (fn);
			if (isFolder (folder)) {
				return true;
			}

			DirectoryInfo dicInfo = Directory.CreateDirectory (folder);
			return dicInfo.Exists;
		}

		// 创建文件
		static public bool createFile (string fn, byte[] buffs)
		{
			File.WriteAllBytes (fn, buffs);
			return true;
		}

		static public bool createFile (string fn, string contents)
		{
			File.WriteAllText (fn, contents);
			return true;
		}

		// 取得文件字节
		static public byte[] getFileBytesSync (string fn)
		{
			return File.ReadAllBytes (fn);
		}

		// 取得文件内容
		static public string getFileContentSync (string fn)
		{
			return File.ReadAllText (fn);
		}

		// 取得目录下面的文件夹
		static public string[] getFns4Folders (string fn)
		{
			try {
				return Directory.GetDirectories (fn);
			} catch (Exception ex) {
				Debug.LogError (ex);
			}
			return new string[0];
		}

		// 取得目录下面的文件(只取得当前文件夹下面的文件)
		static public string[] getFns4Files (string fn)
		{
			try {
				return Directory.GetFiles (fn);
			} catch (Exception ex) {
				Debug.LogError (ex);
			}
			return new string[0];
		}

		// 取得文件如果是文件夹，则返回文件夹路径
		static public string getFnPath (string fn, bool isFolder = true)
		{
			if (string.IsNullOrEmpty (fn)) {
				fn = "";
			}
			int lastIndex = fn.LastIndexOf (PathEx.folderSeparator);
			int lens = fn.Length;
			bool isEndSeparator = lens == (lastIndex + 1);
			if (isFolder && !isEndSeparator && lens > 0) {
				fn = fn + PathEx.folderSeparator;
			}
			return fn;
		}
	}
}
