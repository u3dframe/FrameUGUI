using UnityEngine;
using System.Collections;

namespace Kernel
{
	/// <summary>
	/// 类名 : 资源信息对象
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-07 14:35
	/// 功能 : 
	/// </summary>
	public class ResInfo  {
		// 资源名(一般是相对路径 xxx/xx/xx.ab,xx.lua,)
		public string m_resName = "";

		// 对比码
		public string m_compareCode = "";

		// 资源的包体(上面的相对路径还在该resPackage下面)
		public string m_resPackage = "";

		// 文件大小
		public int m_size = 0;

		// 文件位置(下载的时候用)
		public string m_filePath {
			get { 
				if (string.IsNullOrEmpty (this.m_resPackage)) {
					return this.m_resName;
				} else {
					return string.Format ("{0}/{1}", this.m_resPackage, this.m_resName);
				}
			}
		}

		public ResInfo(){
		}

		public ResInfo(string row){
			Init (row);
		}

		public ResInfo(string resName,string compareCode,string resPackage,int size){
			Init (resName,compareCode,resPackage,size);
		}

		int Str2Int(string str){
			int ret = 0;
			int.TryParse (str, out ret);
			return ret;
		}

		public void Init(string row){
			string[] _arrs = row.Split (",".ToCharArray (), System.StringSplitOptions.None);
			if (_arrs.Length < 3)
				return;
			string resPackage = "";
			if (_arrs.Length > 3)
				resPackage = _arrs [3];
			
			Init (_arrs [0], _arrs [1],resPackage, Str2Int(_arrs [2]));
		}

		public void Init(string resName,string compareCode,string resPackage,int size){
			this.m_resName = resName;
			this.m_compareCode = compareCode;
			this.m_resPackage = resPackage == null ? "" : resPackage;
			this.m_size = size;
		}

		public override string ToString ()
		{
			return string.Concat (m_resName,",",m_compareCode,",",m_size,",",m_resPackage);
		}

		public bool IsSame(ResInfo other){
			if (other == null)
				return false;
			int v = string.Compare(other.m_compareCode,m_compareCode,true);
			return v == 0;
		}

		public void CloneFromOther(ResInfo other){
			this.m_resName = other.m_resName;
			this.m_compareCode = other.m_compareCode;
			this.m_resPackage = other.m_resPackage;
			this.m_size = other.m_size;
		}
	}
}
