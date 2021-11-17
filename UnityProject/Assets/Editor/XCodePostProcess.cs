using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;

public class XCodePostProcess
{
	[PostProcessBuild]
	static void OnPostprocessBuild(BuildTarget target, string path)
	{
		if (target == BuildTarget.iOS)
		{
			ModifyProj(path);

			//SetPlist(path);
		}
	}

    // 修改项目设置
	public static void ModifyProj(string path)
	{
		string projPath = PBXProject.GetPBXProjectPath(path);
		PBXProject pbxProj = new PBXProject();
		pbxProj.ReadFromString(File.ReadAllText(projPath));

		// 配置TARGETS
		string targetGuid = pbxProj.TargetGuidByName("Unity-iPhone");

        // 关闭bitcode
        //pbxProj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "false");

        // 添加.framework
        //pbxProj.AddFrameworkToProject(targetGuid, "Security.framework", false);
        //pbxProj.AddFrameworksBuildPhase(targetGuid);

        // 添加.tbd
        //pbxProj.AddFileToBuild(targetGuid, pbxProj.AddFile("usr/lib/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));

        // 设置teamID
        //pbxProj.SetBuildProperty(targetGuid, "DEVELOPMENT_TEAM", "AUF3355GWB"); //填的是组织单位，而不是用户ID

        // Other Linker Flags
        pbxProj.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");

        File.WriteAllText(projPath, pbxProj.WriteToString());
	}

    // 修改plist
	static void SetPlist(string path)
	{
		string plistPath = path + "/Info.plist";
		PlistDocument plist = new PlistDocument();
		plist.ReadFromString(File.ReadAllText(plistPath));

		// Information Property List
		PlistElementDict plistDict = plist.root;

		// CoreLocation
		plistDict.SetString("NSLocationAlwaysUsageDescription", "I need Location"); //始终访问地理位置
		plistDict.SetString("NSLocationWhenInUseUsageDescription", ""); //在使用期间访问地理位置
		plistDict.SetString("NSCameraUsageDescription", ""); //摄像头权限

		// 设置Array类型
		var array = plistDict.CreateArray("UIBackgroundModes");
		array.AddString("fetch");
		array.AddString("location");

		// 设置Alipay回调app名称
		var urltypes = plistDict.CreateArray("CFBundleURLTypes");
		var item0 = urltypes.AddDict();
		var urlschemes = item0.CreateArray("URL Schemes");
		urlschemes.AddString("mymirror");

		// AMap
		plistDict.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", "");

		File.WriteAllText(plistPath, plist.WriteToString());
	}
}
#endif
