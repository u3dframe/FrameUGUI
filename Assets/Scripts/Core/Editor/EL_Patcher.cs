using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kernel.Core{
	
	/// <summary>
	/// 类名 : ab资源操作
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-13 11:40
	/// 功能 : 将所有资源拷贝到cache目录，然后生成filelist,如果要zip压缩就进行压缩,
	/// 最后Zip或者所以文件拷贝到Assets下面流文件
	/// </summary>
	public static class EL_Patcher {
		
		static public bool m_isBuilded{ get; private set;}
		static List<ResInfo> m_list = new List<ResInfo>();

		static string[] m_ignoreFiles = {
			"version.txt",
			"filelist.txt",
			".manifest",
			".meta",
		};

		static bool _IsIgnoreFile(string fp){
			for (int i = 0; i < m_ignoreFiles.Length; i++) {
				if(fp.Contains (m_ignoreFiles [i])){
					return true;
				}
			}
			return false;
		}

		static bool _IsLuaFile(string fp){
			return fp.EndsWith (".lua", System.StringComparison.OrdinalIgnoreCase);
		}

		static void _CopyFiles(string dirSource,string dirDest,bool isInfo, List<ResInfo> list){
			if (isInfo) {
				if (list == null)
					list = new List<ResInfo> ();
			}

			GameFile.CreateFolder (dirDest);

			EL_Path.Clear ();
			EL_Path.Init (dirSource);

			string _fpRelative = "";
			string _fpdest = "";

			int indexRelative = 0;
			int lensRelative = GameFile.m_curPlatform.Length;

			FileInfo _fInfo;
			ResInfo _info;
			int _fLens = 0;

			float nLens = EL_Path.files.Count;
			int nIndex = 0;
			bool isLuaFile = false;

			string _fmtFile = "正在Copy文件{0}/{1}";
			string _fmtLuaFile = "正在Copy Lua文件{0}/{1}";

			foreach (string fp in EL_Path.files) {
				nIndex++;

				if (isInfo && _IsIgnoreFile (fp))
					continue;

				isLuaFile = _IsLuaFile (fp);

				if (isLuaFile) {
					_fpRelative = fp.Replace (dirSource, "Lua");
				}else{
					indexRelative = fp.IndexOf (GameFile.m_curPlatform);
					if (indexRelative < 0)
						continue;
					
					_fpRelative = fp.Substring(indexRelative + lensRelative + 1);
				}

				EditorUtility.DisplayProgressBar (string.Format(isLuaFile ? _fmtLuaFile : _fmtFile,nIndex,nLens),_fpRelative, nIndex / nLens);

				_fpdest = string.Concat (dirDest, _fpRelative);

				GameFile.CreateFolder (_fpdest);

				_fInfo = new FileInfo (fp);
				_fInfo.CopyTo (_fpdest, true);

				if (isLuaFile)
					_EncodeLuaFile (_fpdest, _fpdest, ref _fInfo);

				if (isInfo) {
					_fLens = (int)_fInfo.Length;
					_info = new ResInfo (_fpRelative, ALG.CRC32Class.GetCRC32 (_fInfo),"",_fLens);
					list.Add (_info);
				}

			}

			EL_Path.Clear ();
		}

		static void _EncodeLuaFile(string srcFile, string outFile,ref FileInfo fInfo) {
		}

		static void _Copy2Cache(List<ResInfo> list){
			GameFile.DeleteFolder (GameFile.m_dirResCache);
			_CopyFiles (GameFile.m_dirRes, GameFile.m_dirResCache, true, list);
			// lua文件 可先byte，删除原来文件，最后在生成resInfo
			string _destLua = string.Concat(GameFile.m_dirResCache,"Lua/");
			_destLua = _destLua.Replace ("\\", "/");
			GameFile.DeleteFolder (_destLua);
			_destLua = GameFile.m_dirResCache;
		}

		static void _MakeNewFilelist(List<ResInfo> list){
			try {
				string _fp = string.Concat (GameFile.m_dirResCache, "filelist.txt");
				GameFile.CreateFolder (_fp);

				using (FileStream stream = new FileStream (_fp, FileMode.OpenOrCreate)) {
					using (StreamWriter writer = new StreamWriter (stream)) {
						for (int i = 0; i < list.Count; i++) {
							writer.WriteLine (list [i].ToString ());
						}
					}
				}
			} catch{
			}
		}

		static void _ZipFiles(List<ResInfo> list,bool isAll){
			string _destFile = isAll ? GameFile.m_fpZipCache : GameFile.m_fpZipCachePatch;
			GameFile.DeleteFile (_destFile);
			GameFile.CreateFolder (_destFile);
		}

		static void _ToSteamingAssets(bool isZip){
			GameFile.DeleteFolder (GameFile.m_dirStreaming);
			GameFile.CreateFolder (GameFile.m_dirStreaming);

			if (isZip) {
				if (File.Exists (GameFile.m_fpZipCache))
					File.Copy (GameFile.m_fpZipCache, GameFile.m_fpZip, true);
				return;
			}

			_CopyFiles (GameFile.m_dirResCache, GameFile.m_dirStreaming, false, null);
		}

		static void _CopyFilelistToRes(){
			string _srcfp = string.Concat (GameFile.m_dirResCache, "filelist.txt");
			string _destfp = string.Concat (GameFile.m_dirRes, "filelist.txt");
			if (File.Exists (_srcfp))
				File.Copy (_srcfp,_destfp, true);
		}

		static void _Build(bool isZip,bool isAll){
			ReInit ();
			EditorUtility.DisplayProgressBar ("Patachering","开始处理...", 0);
			_Copy2Cache (m_list);
			_MakeNewFilelist (m_list);
			if (isZip) {
				_ZipFiles (m_list,isAll);
			}
			if (isAll) {
				_ToSteamingAssets (isZip);
				_CopyFilelistToRes ();
			}
			m_isBuilded = true;
			EditorUtility.ClearProgressBar ();
		}

		static public void BuildAll(bool isZip){
			_Build (isZip, true);
		}

		static public void BuildPatch(){
			if(!m_isBuilded)
				_Build (false, false);

			EditorUtility.DisplayProgressBar ("BuildPatch","开始对比...", 0);

			CompareFiles comFile = new CompareFiles ();
			comFile.m_cfgOld.LoadDefault ();
			comFile.m_cfgNew.LoadFP(string.Concat(GameFile.m_dirResCache,"filelist.txt"));
			comFile.DoCompare ();

			var ups = comFile.m_updates;
			m_list.Clear ();
			foreach (var item in ups.Values) {
				m_list.Add (item);
			}
			_ZipFiles (m_list,false);

			_CopyFilelistToRes ();
			EditorUtility.ClearProgressBar ();
		}

		static public void ClearCache(){
			ReInit ();
			GameFile.DeleteFolder (GameFile.m_dirResCache);
		}

		static public void ReInit(){
			m_isBuilded = false;
			m_list.Clear ();
			EL_Path.Clear ();
		}
	}
}