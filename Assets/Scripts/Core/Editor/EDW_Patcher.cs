using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// 类名 : 生成资源文件放到流文件夹下面
/// 作者 : Canyon
/// 日期 : 2017-12-11 09:50
/// 功能 : 
/// </summary>
public class EDW_Patcher : EditorWindow
{
    static bool isOpenWindowView = false;

    static protected EDW_Patcher vwWindow = null;

    // 窗体宽高
	static public float width = 550;
    static public float height = 300;

	[MenuItem("Tools/Patcher",false,10)]
    static void AddWindow()
    {
        if (isOpenWindowView || vwWindow != null)
            return;

        try
        {
            isOpenWindowView = true;

            vwWindow = GetWindow<EDW_Patcher>("Patcher");

			Vector2 pos = new Vector2(100,100);
			Vector2 size = new Vector2(width,height);
			Rect rect = new Rect(pos,size);
            vwWindow.position = rect;
			vwWindow.minSize = size;
        }
        catch
        {
            ClearWindow();
            throw;
        }
    }

    static void ClearWindow()
    {
        isOpenWindowView = false;
        vwWindow = null;
    }

//    #region  == Member Attribute ===
//    #endregion

    #region  == EditorWindow Func ===

    void Awake()
    {
        Init();
    }

    void OnGUI()
    {
		_Draw();
    }

    // 在给定检视面板每秒10帧更新
    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnDestroy()
    {
        ClearWindow();
        Clear();
    }

    #endregion

    #region  == Self Func ===
    void Init()
    {
    }

    void OnUpdate()
    {
    }

    void Clear()
    {
    }
    #endregion

	#region  == GUI Draw Func ===
	/// <summary>
	/// 创建当前行对象的位置
	/// </summary>
	static public Rect CreateRect(ref int nX,int nY,int nWidth,int nHeight = 20){
		Rect rect = new Rect (nX, nY, nWidth, nHeight);
		nX += nWidth + 5;
		return rect;
	}

	/// <summary>
	/// 设置下一行的开始位置
	/// </summary>
	static public void NextLine(ref int nX,ref int nY,int addHeight = 30,int resetX = 10){
		nX = resetX;
		nY += addHeight;
	}
	#endregion

	void _Draw(){
		Kernel.CfgVersion _cfgVer = Kernel.CfgVersion.instance;
		int curX = 10;
		int curY = 10;
		int height = (int)(position.height - curY - 10);
		int _width = (int)(this.position.width - 20);

		NextLine (ref curX, ref curY, 5);
		GUI.Label (CreateRect (ref curX, curY,32, 25), "平台:");
		_cfgVer.m_platformType = EditorGUI.TextField (CreateRect (ref curX, curY, 50), _cfgVer.m_platformType);

		GUI.Label (CreateRect (ref curX, curY,32, 25), "语言:");
		_cfgVer.m_language = EditorGUI.TextField (CreateRect (ref curX, curY, 50), _cfgVer.m_language);

		GUI.Label (CreateRect (ref curX, curY,60, 25), "游戏版本:");
		_cfgVer.m_gameVerCode = EditorGUI.TextField (CreateRect (ref curX, curY, 80), _cfgVer.m_gameVerCode);

		GUI.Label (CreateRect (ref curX, curY,70, 25), "Svn版本号:");
		_cfgVer.m_svnVerCode = EditorGUI.TextField (CreateRect (ref curX, curY, 100), _cfgVer.m_svnVerCode);

		NextLine (ref curX, ref curY, 30);
		GUI.Label (CreateRect (ref curX, curY,76, 25), "上次资源版本:");
		EditorGUI.LabelField (CreateRect (ref curX, curY, 100), _cfgVer.m_lastResVerCode);
		
		GUI.Label (CreateRect (ref curX, curY,76, 25), "新资源版本号:");
		EditorGUI.LabelField (CreateRect (ref curX, curY, 100), _cfgVer.m_resVerCode);
		
		if (GUI.Button (CreateRect (ref curX, curY, 110), "刷新新版本号")) {
			_cfgVer.RefreshResVerCode ();
		}

		NextLine (ref curX, ref curY, 30);
		GUI.Label (CreateRect (ref curX, curY,80, 25), "version地址:");
		_cfgVer.m_urlVersion = EditorGUI.TextField (CreateRect (ref curX, curY, _width - 90), _cfgVer.m_urlVersion);

		NextLine (ref curX, ref curY, 30);
		GUI.Label (CreateRect (ref curX, curY,80, 25), "补丁下载地址:");
		_cfgVer.m_urlRes = EditorGUI.TextField (CreateRect (ref curX, curY, _width - 90), _cfgVer.m_urlRes);

		NextLine (ref curX, ref curY, 30);

		int botY = (int)(height - 30);
		if (botY > curY) {
			botY = curY;
		}

		if (GUI.Button (CreateRect (ref curX, botY, 100, 30), "生成Filelist")) {
		}

		if (GUI.Button (CreateRect (ref curX, botY, 100,30), "保存版本信息")) {
		}

		if (GUI.Button (CreateRect (ref curX, botY, 100,30), "Zip(全部)")) {
		}

		if (GUI.Button (CreateRect (ref curX, botY, 100,30), "Zip(补丁文件)")) {
		}

		// if (GUI.Button (CreateRect (ref curX, botY, 100,30), "Zip压缩(Mini)")) {
		// }
	}

	void _CopyToCahce(){
		// Kernel.GameFile.m_dirResCache;
	}

	void _MakeFilelist(){
	}

	void _SaveVersion(){
	}
}