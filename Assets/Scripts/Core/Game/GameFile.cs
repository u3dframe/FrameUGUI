using UnityEngine;
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
		static public readonly string m_curPlatform = platformAndroid;
		#elif UNITY_IOS
		static public readonly string m_curPlatform = platformIOS;
		#else
		static public readonly string m_curPlatform = platformWindows;
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

		// zip 压缩文件
		static public readonly string m_fpZipCache = string.Format ("{0}resource.zip", m_dirResCache);
		static public readonly string m_fpZipCachePatch = string.Format ("{0}res_patch.zip", m_dirResCache);
		static public readonly string m_fpZip = string.Format ("{0}resource.zip", m_dirStreaming);

		// zip 压缩文件列表(将文件分包体大小来压缩,减小解压时所需内存)
		static public readonly string m_fpZipListCache = string.Concat (m_dirResCache,"ziplist.txt");
		static public readonly string m_fpZipList = string.Concat (m_dirStreaming,"ziplist.txt");
		static public readonly string m_fmtZipCache = string.Concat (m_dirResCache,"_zips/resource{0}.zip");
		static public readonly string m_fmtZip = string.Concat (m_dirStreaming,"resource{0}.zip");

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

		static public void DeleteFolder(string fp){
			string _fd = Path.GetDirectoryName (fp);
			if (Directory.Exists (_fd)) {
				Directory.Delete (_fd,true);
			}
		}

		// 取得路径
		static public string GetFilePath(string fn){
			return string.Concat (m_dirRes, fn);
		}

		static public string GetStreamingFilePath(string fn){
			return string.Concat (m_dirStreaming, fn);
		}

		static public string GetPath(string fn){
			string _fp = GetFilePath (fn);
			if (File.Exists (_fp)) {
				return _fp;
			}

			return GetStreamingFilePath (fn);
		}

		static public void DeleteFile(string fn){
			string _fp = GetFilePath (fn);
			if (File.Exists (_fp))
				File.Delete (_fp);
		}

		// 取得文本内容
		static public string GetText(string fn){
			string _fp = GetPath (fn);
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
			string _fp = GetPath (fn);
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

		static public string ReUrlEnd(string url){
			int _index = url.LastIndexOf("/");
			if (_index == url.Length - 1) {
				return url;
			}
			return string.Concat (url, "/");
		}

		static public string ReUrlTime(string url){
			return string.Concat (url, "?time=", System.DateTime.Now.Ticks);
		}

		static public string ReUrlTime(string url,string fn){
			url = ReUrlEnd (url);
			return string.Concat (url,fn,"?time=", System.DateTime.Now.Ticks);
		}

		static public string ReUrlTime(string url,string proj,string fn){
			if (!string.IsNullOrEmpty (proj)) {
				url = ReUrlEnd (url);
				url = string.Concat (url, proj);
			}
			return ReUrlTime (url, fn);
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

