#tool nuget:?package=Cake.CoreCLR&version=1.1.0
#tool dotnet:?package=GitVersion.Tool&version=5.7.0
#addin nuget:?package=SharpZipLib&version=1.3.1
#addin nuget:?package=Cake.Compression&version=0.2.6
#addin Cake.Incubator&version=6.0.0
#addin Cake.FileHelpers&version=4.0.1
///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Make");
var configuration = Argument("configuration", "Release");
var modname = "ISE";
var mod_source_path = "./ise-mod";
var version = "1.0";
var mod_base_path = "mod_package";
var mod_path = $"{mod_base_path}/{modname}";
var mod_common_path = $"{mod_path}/Common";
var v12_path = "./ise-mod-12";
var git_hash = "";
var asm_version = "";
var steam_folder = Argument<string>("steam_path");
var v13_game_folder = Argument<string>("v13_path");
var v12_game_folder = Argument<string>("v12_path");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////


Task("CopySourceCodeV12")
.Does(() => {
  CreateDirectory(v12_path);
  DeleteDirectory(v12_path, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
  });

  CopyDirectory(mod_source_path, v12_path);

  CreateDirectory(v12_path + "/bin");
  CreateDirectory(v12_path + "/obj");
  DeleteDirectory(v12_path + "/bin", new DeleteDirectorySettings {
    Recursive = true,
    Force = true
  });
    DeleteDirectory(v12_path + "/obj", new DeleteDirectorySettings {
    Recursive = true,
    Force = true
  });
  MoveFile(v12_path + "/ise-mod.csproj", v12_path + "/ise-mod-12.csproj");
  CreateDirectory(v12_path+"/Assemblies");
});

Task("UpdateReferences")
.IsDependentOn("CopySourceCodeV12")
.Does(() => {
  var changed = ReplaceRegexInFiles(
  $"{mod_source_path}/ise-mod.csproj", 
  @"<HintPath>.*(RimWorldWin64_Data[\\|\/]Managed)[\\|\/](.*)<\/HintPath>", 
  $"<HintPath>{v13_game_folder}/$1/$2</HintPath>"
  );
  ReplaceRegexInFiles(
  $"{v12_path}/ise-mod-12.csproj", 
  @"<HintPath>.*(RimWorldWin64_Data[\\|\/]Managed)[\\|\/](.*)<\/HintPath>", 
  $"<HintPath>{v12_game_folder}/$1/$2</HintPath>"
  );
  ReplaceTextInFiles(
    $"{v12_path}/ise-mod-12.csproj", 
    "Version=1.3.7892.27157,", 
    "Version=1.2.7528.19679"
  );
});

Task("CopyDataFolders")
.Does(() => {
  CreateDirectory(mod_path);
  DeleteDirectory(mod_path, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
  });
  CreateDirectory(mod_common_path);
  CopyDirectory(mod_source_path + "/About", mod_path+"/About");
  CopyDirectory(mod_source_path + "/Defs", mod_common_path+"/Defs");
  CopyDirectory(mod_source_path + "/Languages", mod_common_path+"/Languages");
  CopyDirectory(mod_source_path + "/Textures", mod_common_path+"/Textures");
  CreateDirectory(mod_path+"/1.3/Assemblies");
  CreateDirectory(mod_path+"/1.2/Assemblies");
});

Task("CopyDLLs")
.IsDependentOn("Compile")
.Does(() => {

  // Common DLLs
 
  // CopyFile(mod_source_path +  $"/bin/{configuration}/RestSharp.dll", mod_path+"/Common/1RestSharp.dll");
  // CopyFile(mod_source_path +  $"/bin/{configuration}/LiteDB.dll", mod_path+"/Common/1LiteDB.dll");
  // CopyFile(mod_source_path +  $"/bin/{configuration}/Google.Protobuf.dll", mod_path+"/Common/2Google.Protobuf.dll");
  // CopyFile(mod_source_path +  $"/bin/{configuration}/MiniSentrySDK.dll", mod_path+"/Assemblies/2MiniSentrySDK.dll");
  
  // Version 13
  CopyFile(mod_source_path +  $"/bin/{configuration}/RestSharp.dll", mod_path+"/1.3/Assemblies/1RestSharp.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/LiteDB.dll", mod_path+"/1.3/Assemblies/1LiteDB.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/Google.Protobuf.dll", mod_path+"/1.3/Assemblies/2Google.Protobuf.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/ise-core.dll", mod_path+"/1.3/Assemblies/98ise-core.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/ise-mod.dll", mod_path+"/1.3/Assemblies/99ise-mod.dll");
  if(configuration == "Debug")
  {
    CopyFile(mod_source_path +  $"/bin/{configuration}/ise-core.pdb", mod_path+"/1.3/Assemblies/98ise-core.pdb");
    CopyFile(mod_source_path +  $"/bin/{configuration}/ise-mod.pdb", mod_path+"/1.3/Assemblies/99ise-mod.pdb");
  };

  // Version 12
  CopyFile(mod_source_path +  $"/bin/{configuration}/RestSharp.dll", mod_path+"/1.2/Assemblies/1RestSharp.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/LiteDB.dll", mod_path+"/1.2/Assemblies/1LiteDB.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/Google.Protobuf.dll", mod_path+"/1.2/Assemblies/2Google.Protobuf.dll");
  CopyFile(v12_path +  $"/bin/{configuration}/ise-core.dll", mod_path+"/1.2/Assemblies/98ise-core.dll");
  CopyFile(v12_path +  $"/bin/{configuration}/ise-mod.dll", mod_path+"/1.2/Assemblies/99ise-mod.dll");
  if(configuration == "Debug")
  {
    CopyFile(v12_path +  $"/bin/{configuration}/ise-core.pdb", mod_path+"/1.2/Assemblies/98ise-core.pdb");
    CopyFile(v12_path +  $"/bin/{configuration}/ise-mod.pdb", mod_path+"/1.2/Assemblies/99ise-mod.pdb");
  };
});

Task("GetGitVersion")
.Does(() => {
  var gvSettings = new GitVersionSettings {
    UpdateAssemblyInfo = true
  };
  var gv = GitVersion(gvSettings);

  git_hash = gv.Sha.Substring(0,7);
  asm_version = gv.AssemblySemFileVer;
  Information($"Git hash is: {git_hash}, File Version is: {asm_version}");
});

Task("UpdateXML")
.IsDependentOn("GetGitVersion")
.IsDependentOn("CopyDataFolders")
.Does(() => {
  ReplaceTextInFiles($"{mod_path}/About/About.xml", "!!GITCOMMIT!!", git_hash);
  ReplaceTextInFiles($"{mod_path}/About/About.xml", "!!VERSION!!", asm_version);

});

Task("Compile")
.Does(() => {
  var msBuildSettings = new MSBuildSettings {
    Verbosity = Verbosity.Normal,
    ToolVersion = MSBuildToolVersion.Default,
    Configuration = configuration,
    PlatformTarget = PlatformTarget.MSIL,
  };
  if(!IsRunningOnWindows())
	{
    msBuildSettings.ToolPath = new FilePath(
		  @"/usr/lib/mono/msbuild/15.0/bin/MSBuild.dll"
		  );
	}

	CreateDirectory(mod_source_path + "/Assemblies");
	DeleteDirectory(mod_source_path + "/Assemblies", new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  MSBuild("./iseworld.sln", msBuildSettings.WithTarget("Build"));
});

Task("Make")
.IsDependentOn("UpdateXML")
  .IsDependentOn("UpdateReferences")
  .IsDependentOn("CopyDataFolders")
  .IsDependentOn("CopyDLLs")
  .Does(() => {
});

Task("MakeZIP")
.IsDependentOn("Make")
.Does(() => {
  var zip_name = $"{modname}-Build_{git_hash}_{configuration}.zip";
  Zip(mod_base_path, zip_name);
  Information($"ZIP Name is: {zip_name}");
});

Task("CopyToSteam")
.Does(() => {
  var rimworld_mod_path = $"{steam_folder}/steamapps/common/RimWorld/Mods/";
  var dir_name = $"{rimworld_mod_path}{modname}";
	CreateDirectory(dir_name);
	DeleteDirectory(dir_name, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  ZipUncompress(
    $"{modname}-Build_{git_hash}_{configuration}.zip", 
    rimworld_mod_path
  );
  Information($"ZIP Unpacked in: {rimworld_mod_path}");
});

Task("CopyToLocal")
.Does(() => {

  // Version 13
  var game_mod_path = $"{v13_game_folder}/mods/{modname}";
	CreateDirectory(game_mod_path);
	DeleteDirectory(game_mod_path, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  game_mod_path = $"{v13_game_folder}/mods/";
  ZipUncompress($"{modname}-Build_{git_hash}_{configuration}.zip", v13_game_folder);
  Information($"ZIP Unpacked in: {game_mod_path}");

  // Version 12
  game_mod_path = $"{v12_game_folder}/mods/{modname}";
	CreateDirectory(game_mod_path);
	DeleteDirectory(game_mod_path, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  game_mod_path = $"{v12_game_folder}/mods/";
  ZipUncompress($"{modname}-Build_{git_hash}_{configuration}.zip", v12_game_folder);
  Information($"ZIP Unpacked in: {game_mod_path}");
});

Task("SteamPublish")
  .IsDependentOn("MakeZIP")
	.IsDependentOn("CopyToSteam")
  .Does(() => {});


Task("LocalPublish")
  .IsDependentOn("MakeZIP")
	.IsDependentOn("CopyToLocal")
  .Does(() => {});

Task("Publish")
  .IsDependentOn("LocalPublish")
  .IsDependentOn("SteamPublish")
  .Does(() => {});

RunTarget(target);
