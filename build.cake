#addin nuget:?package=SharpZipLib&version=1.3.1
#addin nuget:?package=Cake.Compression&version=0.2.6
#addin Cake.Incubator&version=6.0.0
#tool nuget:?package=GitVersion.CommandLine&version=5.3.7
#addin Cake.FileHelpers&version=4.0.1
#tool nuget:?package=Cake.CoreCLR&version=1.1.0
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
var steam_folder = @"F:\Games\Steam\steamapps\common\RimWorld\Mods";
var v13_game_folder = @"G:\Games\RimWorld13\Mods";
var v12_game_folder = @"G:\Games\RimWorld12\Mods";

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

Task("UpdateV12References")
.IsDependentOn("CopySourceCodeV12")
.Does(() => {
  ReplaceTextInFiles(
  $"{mod_path}/ise-mod-12.csproj", 
  @"..\..\..\..\..\Games\RimWorld13\RimWorldWin64_Data\Managed\", 
  @"..\..\..\..\..\Games\RimWorld12\RimWorldWin64_Data\Managed\"
  );
  ReplaceTextInFiles(
    $"{mod_path}/ise-mod-12.csproj", 
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
  git_hash = GitVersion().Sha.Substring(0,8);
  Information($"Git hash is: {git_hash}");
});

Task("GetAsmVersion")
.Does(() => {
  var assemblyInfo = ParseAssemblyInfo(mod_source_path + "/Properties/AssemblyInfo.cs");
  asm_version = $"{assemblyInfo.AssemblyFileVersion}";
  Information($"Assembly version is: {asm_version}");
});


Task("UpdateXML")
.IsDependentOn("GetGitVersion")
.IsDependentOn("GetAsmVersion")
.IsDependentOn("CopyDataFolders")
.Does(() => {
  ReplaceTextInFiles($"{mod_path}/About/About.xml", "!!GITCOMMIT!!", git_hash);

  ReplaceTextInFiles($"{mod_path}/About/Version.xml", "!!VERSION!!", asm_version);

});

Task("Compile")
.Does(() => {
	CreateDirectory(mod_source_path + "/Assemblies");
	DeleteDirectory(mod_source_path + "/Assemblies", new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  MSBuild("./iseworld.sln", new MSBuildSettings {
    Verbosity = Verbosity.Normal,
    ToolVersion = MSBuildToolVersion.Default,
    Configuration = configuration,
    PlatformTarget = PlatformTarget.MSIL,
  }.WithTarget("Build"));
});

Task("Make")
  .IsDependentOn("UpdateV12References")
  .IsDependentOn("CopyDataFolders")
  .IsDependentOn("CopyDLLs")
  .IsDependentOn("UpdateXML")
  .Does(() => {
});

Task("MakeZIP")
.Does(() => {
  var zip_name = $"{modname}-Build_{git_hash}_{configuration}.zip";
  Zip(mod_base_path, zip_name);
  Information($"ZIP Name is: {zip_name}");
});

Task("CopyToSteam")
.Does(() => {
  var dir_name = $"{steam_folder}/{modname}";
	CreateDirectory(dir_name);
	DeleteDirectory(dir_name, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  ZipUncompress($"{modname}-Build_{git_hash}_{configuration}.zip", steam_folder);
  Information($"ZIP Unpacked in: {dir_name}");
});

Task("CopyToLocal")
.Does(() => {

  // Version 13
  var dir_name = $"{v13_game_folder}/{modname}";
	CreateDirectory(dir_name);
	DeleteDirectory(dir_name, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  ZipUncompress($"{modname}-Build_{git_hash}_{configuration}.zip", v13_game_folder);
  Information($"ZIP Unpacked in: {dir_name}");

  // Version 12
  dir_name = $"{v12_game_folder}/{modname}";
	CreateDirectory(dir_name);
	DeleteDirectory(dir_name, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  ZipUncompress($"{modname}-Build_{git_hash}_{configuration}.zip", v12_game_folder);
  Information($"ZIP Unpacked in: {dir_name}");
});


Task("SteamPublish")
  .IsDependentOn("Make")
  .IsDependentOn("MakeZIP")
	.IsDependentOn("CopyToSteam")
  .Does(() => {});


Task("LocalPublish")
  .IsDependentOn("Make")
  .IsDependentOn("MakeZIP")
	.IsDependentOn("CopyToLocal")
  .Does(() => {});


Task("Publish")
  .IsDependentOn("LocalPublish")
  .IsDependentOn("SteamPublish")
  .Does(() => {});

RunTarget(target);
