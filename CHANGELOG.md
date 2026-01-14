# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.5.2] - 2026-01-14

**Changed**:
- Updated dependency `com.gamelovers.dataextensions` and `com.gamelovers.configsprovider` to `com.gamelovers.gamedata`
- Updated assembly definitions to reference `GameLovers.GameData`

## [0.5.1] - 2024-11-19

**Changed**:
- Removed unecessary *UniTaskExtension* as it doesn't work properly
- Renamed all load and unload methods from *AssetResolverService* with async prefix as they all are executed asynchronously

**Fixed**:
- Fixed the *AssetResolverService.UnloadScene* method that was crashing when sending with a callback

## [0.5.0] - 2024-11-13

**New**:
- Added new *AssetsConfigsImporterBase* to allow to more costumizated asset configs needed to be imported in more complex projects
- Added new *AssetsConfigsGeneratorImporter* & *IAssetConfigsGeneratorImporter* to allow the code generation of *TId* enum and *TScriptableObject* on importing the configs. This makes the development flow of asset configs simpler for lazy devs
- Added cysharp *Unitask* package to the project
- Added *AssetResolverService* & *IAssetResolverService* to the package to help manage addressable assets by it's Id and Asset Type. It also allows to add assets from *ScriptableObject* configs

**Changed**:
- Removed *IScriptableObjectImporter*. Now the *IAssetConfigsImporter* is a complete interface for all asset configs needed to be imported
- Changed the *TId* constrain for the *AssetsConfigsImporter* to be always an Enum
- Improved the performance and Inspector visuals of asset importer
- Changed all asset loading from *AddressableAssetLoader*, *IAssetLoader* & *ISceneLoader* to *UniTask* to allow the code to run on WebGL builds

**Fixed**:
- Fixed the issue preventing this package to be used in WebGL builds

## [0.4.0] - 2024-11-08

**New**:
- Added new "Toggle Auto Import" button in the "Tools/Addressables/" Menu item to allow pausing processing all asset importers when code recompiles. Allowing the editor to be faster reloading on script changes

**Changed**:
- Changed *IAssetLoader* to execute a callback when operation is completed. This allows for non-Task operations to execute logic in it's own scope

**Fixed**:
- Prevented the generation of *AddressableId* when it failed to properly process an asset type. This way avoids future compilation errors while properly reporting the Asset Type failed to load.

## [0.3.0] - 2024-04-27

**New**:
- Added new AddressableId Generator Settings scriptable object to control some of the settings being generated

**Changed**:
- Moved the Unity Editor commands to the Tools/AddressableId Generator path

## [0.2.1] - 2023-09-04

**Changed**:
- Changed AddressableConfig from a struct to a class. This change enhances the flexibility and efficiency of memory usage in our codebase, as AddressableConfig instances can now be shared or null, reducing potential redundancy. Please note that this may alter how AddressableConfig is used in some contexts due to the shift from value type to reference type.

## [0.2.0] - 2023-08-27

**New**:
- Introduced AssetsImporter tool for importing assets data in Unity, accessible from the Unity Editor's Tools menu.
- Added AssetReferenceScene class to validate scene assets.

**Changed**:
- Updated AddressableIdsGenerator to include new namespaces, enums, and lookup methods. Improved asset filtering based on labels.
- Changed parameter type of LoadAssetAsync and InstantiateAsync methods from string to object in AddressablesAssetLoader.
- Renamed namespace from "GameLovers.AddressablesExtensions" to "GameLovers.AssetsImporter" across multiple files.

**Fixed**:
- Prevented destruction of GameObjects in UnloadAsset method of AddressablesAssetLoader.

## [0.1.1] - 2020-08-31

**Fixed**:
- Fixed the namespace set of the generated file from the *AddressablesIdGenerator*

## [0.1.0] - 2020-08-31

- Initial submission for package distribution
