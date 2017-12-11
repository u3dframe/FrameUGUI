﻿using UnityEngine;
using System.Collections;
using System.IO;

namespace Kernel
{
	/// <summary>
	/// 类名 : 游戏 路径
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-07 10:05
	/// 功能 : 目前不考虑直接加载 Assets 下面的源文件(只处理下载，解压文件或resources下面的文件)
	/// </summary>
	public static class GameFile
	{
		// 平台
		static public readonly string platformAndroid = "Android";
		static public readonly string platformIOS = "IOS";
		static public readonly string platformWindows = "Windows";

		#if UNITY_ANDROID
		static readonly string m_curPlatform = platformAndroid;
		#elif UNITY_IOS
		static readonly string m_curPlatform = platformIOS;
		#else
		static readonly string m_curPlatform = platformWindows;
		#endif

		// 流文件夹
		static public readonly string m_dirStreaming = string.Format ("{0}/", Application.streamingAssetsPath);

		// 解压文件夹(下载文件夹)
		#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
		static public readonly string m_dirRes = string.Format ("{0}/",Application.persistentDataPath);
		#else
		static public readonly string m_dirRes = string.Format ("{0}/../../GameRes/{1}/", Application.dataPath, m_curPlatform);
		#endif

		// 临时存放所有生成的文件(ab,txt等，可以进行Zip压缩，最后将生成zip文件或copy的所以文件再拷贝到流文件夹下)
		static public readonly string m_dirResCache = string.Format ("{0}/../../GameCache/{1}/", Application.dataPath, m_curPlatform);

		// 统一分割符号
		static public string ReDirSeparator(string fp){
			return fp.Replace ('\\', '/');
		}

		// 创建文件夹
		static public void CreateFolder(string fp){
			string _fd = Path.GetDirectoryName (fp);
			if (!Directory.Exists (_fd)) {
				Directory.CreateDirectory (_fd);
			}
		}

		// 取得路径
		static public string GetFilePath(string fn){
			return string.Format ("{0}{1}", m_dirRes, fn);
		}

		static public string GetStreamingFilePath(string fn){
			return string.Format ("{0}{1}", m_dirStreaming, fn);
		}

		static public void DeleteFile(string fn){
			string _fp = GetFilePath (fn);
			if (File.Exists (_fp))
				File.Delete (_fp);
		}

		// 取得文本内容
		static public string GetText(string fn){
			string _fp = GetFilePath (fn);
			if (File.Exists (_fp)) {
				return File.ReadAllText (_fp);
			}

			string _suffix = Path.GetExtension (fn);
			string _fnNoSuffix = fn.Substring(0, fn.LastIndexOf(_suffix));
			TextAsset txtAsset = Resources.Load<TextAsset> (_fnNoSuffix); // 可以不用考虑释放txtAsset
			if (txtAsset)
				return txtAsset.text;
			return "";
		}

		// 取得文件流
		static public byte[] GetFileBytes(string fn){
			string _fp = GetFilePath (fn);
			if (File.Exists (_fp)) {
				return File.ReadAllBytes (_fp);
			}

			string _suffix = Path.GetExtension (fn);
			string _fnNoSuffix = fn.Substring(0, fn.LastIndexOf(_suffix));
			TextAsset txtAsset = Resources.Load<TextAsset> (_fnNoSuffix); // 可以不用考虑释放txtAsset
			if (txtAsset)
				return txtAsset.bytes;
			return null;
		}

		static public AssetBundleManifest GetAssetManifest(){
			byte[] _bts = GetFileBytes (m_curPlatform);
			AssetBundleManifest ret = null;
			if (_bts != null) {
				AssetBundle ab = AssetBundle.LoadFromMemory (_bts);
				ret = ab.LoadAsset<AssetBundleManifest> ("AssetBundleManifest");
				ab.Unload (false);
			}
			return ret;
		}
	}
}

