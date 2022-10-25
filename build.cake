#tool nuget:?package=Cake.CoreCLR&version=1.1.0
#tool dotnet:?package=GitVersion.Tool&version=5.6.11
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
var mod_previous_version_path = "./ise-mod-previous";
var git_hash = "";
var asm_version = "";
var steam_folder = Argument<string>("steam_path");
var latest_game_folder = Argument<string>("game_current_version_path");
var previous_game_folder = Argument<string>("game_previous_version_path");

var current_version_string = "1.4";
var previous_version_string = "1.3";
var current_assembly_version = "1.4.8330.27538";
var previous_assembly_version = "1.3.8194.1375";


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


Task("CopySourceCodePreviousVersion")
.Does(() => {
  CreateDirectory(mod_previous_version_path);
  DeleteDirectory(mod_previous_version_path, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
  });

  CopyDirectory(mod_source_path, mod_previous_version_path);

  CreateDirectory(mod_previous_version_path + "/bin");
  CreateDirectory(mod_previous_version_path + "/obj");
  DeleteDirectory(mod_previous_version_path + "/bin", new DeleteDirectorySettings {
    Recursive = true,
    Force = true
  });
    DeleteDirectory(mod_previous_version_path + "/obj", new DeleteDirectorySettings {
    Recursive = true,
    Force = true
  });
  MoveFile(mod_previous_version_path + "/ise-mod.csproj", mod_previous_version_path + "/ise-mod-previous.csproj");
  CreateDirectory(mod_previous_version_path+"/Assemblies");
});

Task("UpdateReferences")
.IsDependentOn("CopySourceCodePreviousVersion")
.Does(() => {
  var changed = ReplaceRegexInFiles(
  $"{mod_source_path}/ise-mod.csproj", 
  @"<HintPath>.*(RimWorldLinux_Data[\\|\/]Managed)[\\|\/](.*)<\/HintPath>", 
  $"<HintPath>{latest_game_folder}/$1/$2</HintPath>"
  );
  ReplaceRegexInFiles(
  $"{mod_previous_version_path}/ise-mod-previous.csproj", 
  @"<HintPath>.*(RimWorldLinux_Data[\\|\/]Managed)[\\|\/](.*)<\/HintPath>", 
  $"<HintPath>{previous_game_folder}/$1/$2</HintPath>"
  );
  ReplaceTextInFiles(
    $"{mod_previous_version_path}/ise-mod-previous.csproj", 
    $"Version={current_assembly_version},", 
    $"Version={previous_assembly_version}"
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
  CreateDirectory(mod_path+$"/{current_version_string}/Assemblies");
  CreateDirectory(mod_path+$"/{previous_version_string}/Assemblies");
});

Task("CopyDLLs")
.IsDependentOn("Compile")
.Does(() => {

  // Common DLLs
 
  // CopyFile(mod_source_path +  $"/bin/{configuration}/RestSharp.dll", mod_path+"/Common/1RestSharp.dll");
  // CopyFile(mod_source_path +  $"/bin/{configuration}/LiteDB.dll", mod_path+"/Common/1LiteDB.dll");
  // CopyFile(mod_source_path +  $"/bin/{configuration}/Google.Protobuf.dll", mod_path+"/Common/2Google.Protobuf.dll");
  // CopyFile(mod_source_path +  $"/bin/{configuration}/MiniSentrySDK.dll", mod_path+"/Assemblies/2MiniSentrySDK.dll");
  
  // Current Version
  CopyFile(mod_source_path +  $"/bin/{configuration}/RestSharp.dll", mod_path+$"/{current_version_string}/Assemblies/1RestSharp.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/LiteDB.dll", mod_path+$"/{current_version_string}/Assemblies/1LiteDB.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/Google.Protobuf.dll", mod_path+$"/{current_version_string}/Assemblies/2Google.Protobuf.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/ise-core.dll", mod_path+$"/{current_version_string}/Assemblies/98ise-core.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/ise-mod.dll", mod_path+$"/{current_version_string}/Assemblies/99ise-mod.dll");
  if(configuration == "Debug")
  {
    CopyFile(mod_source_path +  $"/bin/{configuration}/ise-core.pdb", mod_path+$"/{current_version_string}/Assemblies/98ise-core.pdb");
    CopyFile(mod_source_path +  $"/bin/{configuration}/ise-mod.pdb", mod_path+$"/{current_version_string}/Assemblies/99ise-mod.pdb");
  };

  // Previous Version 
  CopyFile(mod_source_path +  $"/bin/{configuration}/RestSharp.dll", mod_path+$"/{previous_version_string}/Assemblies/1RestSharp.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/LiteDB.dll", mod_path+$"/{previous_version_string}/Assemblies/1LiteDB.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/Google.Protobuf.dll", mod_path+$"/{previous_version_string}/Assemblies/2Google.Protobuf.dll");
  CopyFile(mod_previous_version_path +  $"/bin/{configuration}/ise-core.dll", mod_path+$"/{previous_version_string}/Assemblies/98ise-core.dll");
  CopyFile(mod_previous_version_path +  $"/bin/{configuration}/ise-mod.dll", mod_path+$"/{previous_version_string}/Assemblies/99ise-mod.dll");
  if(configuration == "Debug")
  {
    CopyFile(mod_previous_version_path +  $"/bin/{configuration}/ise-core.pdb", mod_path+$"/{previous_version_string}/Assemblies/98ise-core.pdb");
    CopyFile(mod_previous_version_path +  $"/bin/{configuration}/ise-mod.pdb", mod_path+$"/{previous_version_string}/Assemblies/99ise-mod.pdb");
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

  // Current Version
  var game_mod_path = $"{latest_game_folder}/mods/{modname}";
	CreateDirectory(game_mod_path);
	DeleteDirectory(game_mod_path, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  game_mod_path = $"{latest_game_folder}/mods";
  ZipUncompress($"{modname}-Build_{git_hash}_{configuration}.zip", game_mod_path);
  Information($"ZIP Unpacked in: {game_mod_path}");

  // Previous Version
  game_mod_path = $"{previous_game_folder}/mods/{modname}";
	CreateDirectory(game_mod_path);
	DeleteDirectory(game_mod_path, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  game_mod_path = $"{previous_game_folder}/mods";
  ZipUncompress($"{modname}-Build_{git_hash}_{configuration}.zip", game_mod_path);
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
