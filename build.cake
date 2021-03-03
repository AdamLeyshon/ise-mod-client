#addin nuget:?package=SharpZipLib&version=1.3.1
#addin nuget:?package=Cake.Compression&version=0.2.4
#addin Cake.Incubator&version=5.1.0
#tool nuget:?package=GitVersion.CommandLine&version=5.3.7
#addin Cake.FileHelpers&version=3.3.0
///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Make");
var configuration = Argument("configuration", "Release");
var modname = "ISE";
var mod_source_path = "./ise-mod";
var version = "1.0";
var mod_base_path = "./mod_package";
var mod_path = $"./{mod_base_path}/{modname} [{version}]";
var git_hash = "";
var asm_version = "";
var steam_folder = @"E:\Games\Steam\steamapps\common\RimWorld\Mods";
var local_game_folder = @"F:\Games\RimWorld12\Mods";

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


Task("CopyDataFolders")
.Does(() => {
  CreateDirectory(mod_path);
  DeleteDirectory(mod_path, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
  });
  CopyDirectory(mod_source_path + "/About", mod_path+"/About");
  CopyDirectory(mod_source_path + "/Defs", mod_path+"/Defs");
  CopyDirectory(mod_source_path + "/Languages", mod_path+"/Languages");
  CopyDirectory(mod_source_path + "/Textures", mod_path+"/Textures");
  CreateDirectory(mod_path+"/Assemblies");
});

Task("CopyDLLs")
.IsDependentOn("Compile")
.Does(() => {
  //CopyFile(mod_source_path + $"/bin/{configuration}/0Harmony.dll", mod_path+"/Assemblies/0Harmony.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/RestSharp.dll", mod_path+"/Assemblies/1RestSharp.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/LiteDB.dll", mod_path+"/Assemblies/1LiteDB.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/Google.Protobuf.dll", mod_path+"/Assemblies/2Google.Protobuf.dll");
  //CopyFile(mod_source_path +  $"/bin/{configuration}/MiniSentrySDK.dll", mod_path+"/Assemblies/2MiniSentrySDK.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/ise-core.dll", mod_path+"/Assemblies/98ise-core.dll");
  CopyFile(mod_source_path +  $"/bin/{configuration}/ise-mod.dll", mod_path+"/Assemblies/99ise-mod.dll");
  if(configuration == "Debug")
  {
    CopyFile(mod_source_path +  $"/bin/{configuration}/ise-core.pdb", mod_path+"/Assemblies/98ise-core.pdb");
    CopyFile(mod_source_path +  $"/bin/{configuration}/ise-mod.pdb", mod_path+"/Assemblies/99ise-mod.pdb");
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
.Does(() => {
  ReplaceRegexInFiles($"{mod_path}/About/About.xml",
                      "@SHAHASH@",
                      git_hash);
  ReplaceRegexInFiles($"{mod_path}/About/Version.xml",
                      "@Version@",
                      asm_version);
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
  .IsDependentOn("CopyDataFolders")
  .IsDependentOn("CopyDLLs")
  .IsDependentOn("UpdateXML")
  .Does(() => {
});

Task("MakeZIP")
.Does(() => {
  var zip_name = $"{modname}_[{version}]-Build_{git_hash}_{configuration}.zip";
  Zip(mod_base_path, zip_name);
  Information($"ZIP Name is: {zip_name}");
});

Task("CopyToSteam")
.Does(() => {
  var dir_name = $"{steam_folder}/{modname} [{version}]";
	CreateDirectory(dir_name);
	DeleteDirectory(dir_name, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  ZipUncompress($"{modname}_[{version}]-Build_{git_hash}_{configuration}.zip", steam_folder);
  Information($"ZIP Unpacked in: {dir_name}");
});

Task("CopyToLocal")
.Does(() => {
  var dir_name = $"{local_game_folder}/{modname} [{version}]";
	CreateDirectory(dir_name);
	DeleteDirectory(dir_name, new DeleteDirectorySettings {
    Recursive = true,
    Force = true
	});
  ZipUncompress($"{modname}_[{version}]-Build_{git_hash}_{configuration}.zip", local_game_folder);
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
