using System.Collections.Generic;
using System;
using GameLovers.GameData;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace GameLovers.AssetsImporter
{
	/// <summary>
	/// Service to extend the behaviour to load assets into the project based on it's own custom needs
	/// </summary>
	public interface IAssetResolverService : IAssetLoader, ISceneLoader
	{
		/// <summary>
		/// Requests the given <typeparamref name="TAsset"/> of the given <paramref name="id"/>.
		/// If <paramref name="loadAsynchronously"/> is true then will load asynchronously.
		/// It will also return the result in the provided <paramref name="onLoadCallback"/> when the loading is complete
		/// and will instantiate the asset if the given <paramref name="instantiate"/> is true
		/// </summary>
		UniTask<TAsset> RequestAsset<TId, TAsset>(TId id, bool loadAsynchronously = true, bool instantiate = true,
												Action<TId, TAsset, bool> onLoadCallback = null)
			where TAsset : Object;

		/// <inheritdoc cref="RequestAsset{TId,TAsset}"/>
		/// <remarks>
		/// Enhances the behaviour to pass the given <paramref name="data"/> to the <paramref name="onLoadCallback"/>
		/// </remarks>
		UniTask<TAsset> RequestAsset<TId, TAsset, TData>(TId id, TData data, bool loadAsynchronously = true,
														bool instantiate = true,
														Action<TId, TAsset, TData, bool> onLoadCallback = null)
			where TAsset : Object;

		/// <summary>
		/// Loads asynchronously a <see cref="Scene"/> mapped with the given <paramref name="id"/> and the given info.
		/// It will also return the result in the provided <paramref name="onLoadCallback"/> when the loading is complete
		/// </summary>
		UniTask<SceneInstance> LoadSceneAsync<TId>(TId id, LoadSceneMode loadMode = LoadSceneMode.Single,
											bool activateOnLoad = true,
											bool setActive = true, Action<TId, SceneInstance> onLoadCallback = null);


		/// <summary>
		/// Loads all assets previously added from <seealso cref="IAssetAdderService.AddConfigs{TId,TAsset}(AssetConfigsScriptableObject{TId,TAsset})"/>
		/// Has the option from the params to <paramref name="loadAsynchronously"/> and have a <paramref name="onLoadCallback"/>
		/// for each asset being loaded.
		/// Returns the full list of assets loaded into memory.
		/// </summary>
		/// <remarks>
		/// Will require to call <see cref="UnloadAssets{TId,TAsset}(bool,AssetConfigsScriptableObject{TId,TAsset})"/>
		/// to clean the assets from memory again
		/// </remarks>
		UniTask<List<Pair<TId, TAsset>>> LoadAllAssets<TId, TAsset>(bool loadAsynchronously = true,
																	Action<TId, TAsset> onLoadCallback = null);

		/// <summary>
		/// Unloads asynchronously a <see cref="Scene"/> mapped with the given <paramref name="id"/>.
		/// It will also return the result in the provided <paramref name="onUnloadCallback"/> when the loading is complete
		/// </summary>
		UniTask UnloadSceneAsync<TId>(TId id, Action<TId> onUnloadCallback = null);

		/// <summary>
		/// Unloads all the asset reference of the given <typeparamref name="TId"/> type.
		/// If the given <paramref name="clearReferences"/> is true then will also removes any reference to the assets
		/// </summary>
		void UnloadAssets<TId, TAsset>(bool clearReferences);

		/// <summary>
		/// Unloads the asset reference of the given <typeparamref name="TId"/> from the given <paramref name="assetConfigs"/>.
		/// If the given <paramref name="clearReferences"/> is true then will also removes any reference to the assets
		/// </summary>
		void UnloadAssets<TId, TAsset>(bool clearReferences, AssetConfigsScriptableObject<TId, TAsset> assetConfigs);

		/// <summary>
		/// Unloads the asset reference of the given <typeparamref name="TId"/> type of the given <paramref name="ids"/>.
		/// If the given <paramref name="clearReferences"/> is true then will also removes any reference to the assets
		/// </summary>
		void UnloadAssets<TId, TAsset>(bool clearReferences, params TId[] ids);
	}

	/// <inheritdoc />
	/// <remarks>
	/// It allows to add new asset references to the service in order to separate the behaviour of the service between getters and setters
	/// </remarks>
	public interface IAssetAdderService : IAssetResolverService
	{
		/// <summary>
		/// Adds the given <paramref name="configs"/> to the asset reference list with <typeparamref name="TId"/> as
		/// the identifier type and <typeparamref name="TAsset"/> as asset type
		/// </summary>
		void AddConfigs<TId, TAsset>(AssetConfigsScriptableObject<TId, TAsset> configs) => AddAssets(configs.AssetType, configs.Configs);

		/// <summary>
		/// Adds the given <paramref name="assets"/> to the asset reference list with <typeparamref name="TId"/> as
		/// the identifier type and <paramref name="assetType"/> as asset type
		/// </summary>
		/// <typeparam name="TId">The identifier type</typeparam>
		/// <param name="assetType">The asset type</param>
		/// <param name="assets">The assets to add</param>
		void AddAssets<TId>(Type assetType, List<Pair<TId, AssetReference>> assets);

		/// <summary>
		/// Adds a single asset reference to the asset reference list with <typeparamref name="TId"/> as
		/// the identifier type and <paramref name="assetType"/> as asset type
		/// </summary>
		/// <typeparam name="TId">The identifier type</typeparam>
		/// <param name="assetType">The asset type</param>
		/// <param name="id">The identifier of the asset</param>
		/// <param name="assetReference">The asset reference to add</param>
		void AddAsset<TId>(Type assetType, TId id, AssetReference assetReference);

		/// <summary>
		/// Adds the debug assets when errors occur
		/// </summary>
		void AddDebugConfigs(Sprite errorSprite = null, GameObject errorCube = null, Material errorMaterial = null, 
			AudioClip errorClip = null);
	}

	/// <inheritdoc cref="IAssetResolverService" />
	public class AssetResolverService : AddressablesAssetLoader, IAssetAdderService
	{
		private readonly IDictionary<Type, IDictionary<Type, IDictionary>> _assetMap =
			new Dictionary<Type, IDictionary<Type, IDictionary>>();

		private Sprite _errorSprite;
		private GameObject _errorCube;
		private Material _errorMaterial;
		private AudioClip _errorClip;

		/// <inheritdoc />
		public async UniTask<SceneInstance> LoadSceneAsync<TId>(TId id, LoadSceneMode loadMode = LoadSceneMode.Single,
														bool activateOnLoad = true, bool setActive = true,
														Action<TId, SceneInstance> onLoadCallback = null)
		{
			if (!TryGetAssetReference<TId, Scene>(id, out var assetReference))
			{
				throw new MissingMemberException($"The {nameof(AssetResolverService)} does not have the scene config " +
												 $"to load with the given {id} id of {typeof(TId)} type. " +
												 $"Use the {nameof(AddAssets)} method to add it.");
			}

			if (!assetReference.OperationHandle.IsValid())
			{
				_ = assetReference.LoadSceneAsync(loadMode, activateOnLoad);
			}

			if (!assetReference.IsDone)
			{
				await assetReference.OperationHandle.ToUniTask();
			}

			var sceneOperation = assetReference.OperationHandle.Convert<SceneInstance>();

			if (setActive)
			{
				SceneManager.SetActiveScene(sceneOperation.Result.Scene);
			}

			onLoadCallback?.Invoke(id, sceneOperation.Result);

			return sceneOperation.Result;
		}

		/// <inheritdoc />
		public async UniTask<List<Pair<TId, TAsset>>> LoadAllAssets<TId, TAsset>(bool loadAsynchronously = true,
																			  Action<TId, TAsset> onLoadCallback = null)
		{
			var list = new List<Pair<TId, TAsset>>();
			var tasks = new List<UniTask>();

			if (!TryGetDictionary<TId, TAsset>(out var dictionary))
			{
				throw new MissingMemberException($"The {nameof(AssetResolverService)} does not have the '{typeof(TAsset)}' " +
												 $"asset list for the '{typeof(TId)}' type to load. " +
												 $"Use the {nameof(AddAssets)} method to add it.");
			}

			foreach (var pair in dictionary)
			{
				var operation = pair.Value.LoadAssetAsync<TAsset>();
				var id = pair.Key;
				var task = operation.ToUniTask().ContinueWith(result =>
				{
					list.Add(new Pair<TId, TAsset>(id, result));
					onLoadCallback?.Invoke(id, result);
				});

				tasks.Add(task);

				if (!loadAsynchronously)
				{
					operation.WaitForCompletion();
				}
			}

			await UniTask.WhenAll(tasks);

			/* Keep the code in case some asset types don't work with the code above
			foreach (var pair in dictionary)
			{
				list.Add(new Pair<TId, TAsset>(pair.Key, pair.Value.OperationHandle.Convert<TAsset>().Result));
			}*/

			return list;
		}

		/// <inheritdoc />
		public async UniTask<TAsset> RequestAsset<TId, TAsset>(TId id, bool loadAsynchronously = true,
															bool instantiate = true,
															Action<TId, TAsset, bool> onLoadCallback = null)
			where TAsset : Object
		{
			return await RequestAsset<TId, TAsset, int>(id, 0, loadAsynchronously, instantiate,
														(arg1, arg2, _, arg4) => onLoadCallback?.Invoke(arg1, arg2, arg4));
		}

		/// <inheritdoc />
		public async UniTask<TAsset> RequestAsset<TId, TAsset, TData>(TId id, TData data, bool loadAsynchronously = true,
																   bool instantiate = true,
																   Action<TId, TAsset, TData, bool> onLoadCallback = null)
			where TAsset : Object
		{
			if (!TryGetAssetReference<TId, TAsset>(id, out var assetReference))
			{
				throw new MissingMemberException($"The {nameof(AssetResolverService)} does not have the " +
												 $"{nameof(AssetReference)} config to load the necessary asset for the " +
												 $"given {typeof(TAsset)} type with the given {id} id of {typeof(TId)} type");
			}

			if (!assetReference.OperationHandle.IsValid())
			{
				_ = assetReference.LoadAssetAsync<TAsset>();
			}

			if (!loadAsynchronously)
			{
				assetReference.OperationHandle.WaitForCompletion();
			}

			if (!assetReference.IsDone)
			{
				await assetReference.OperationHandle.ToUniTask();
			}

			var asset = Convert<TAsset>(assetReference, instantiate);

			if (asset == null || assetReference.Asset == null)
			{
				Debug.LogWarning($"The given id '{id.ToString()}' is loading an empty asset '{typeof(TAsset)}' type");
			}

			onLoadCallback?.Invoke(id, asset, data, instantiate);

			return asset;
		}

		/// <inheritdoc />
		public async UniTask UnloadSceneAsync<TId>(TId id, Action<TId> onUnloadCallback = null)
		{
			if (!TryGetAssetReference<TId, Scene>(id, out var assetReference))
			{
				Debug.LogWarning($"The {nameof(AssetResolverService)} does not have the config to unload the scene with the given {id} " +
					$"id of {typeof(TId)} type. Use the {nameof(AddAssets)} method to add it.");
				return;
			}

			await assetReference.UnLoadScene().ToUniTask();

			onUnloadCallback?.Invoke(id);
		}

		/// <inheritdoc />
		public void UnloadAssets<TId, TAsset>(bool clearReferences)
		{
			if (!TryGetDictionary<TId, TAsset>(out var dictionary))
			{
				Debug.LogWarning($"The {nameof(AssetResolverService)} does not have the '{typeof(TAsset)}' " +
					$"asset list for the '{typeof(TId)}' type. Use the {nameof(AddAssets)} method to add it.");
				return;
			}

			foreach (var asset in dictionary)
			{
				if (asset.Value.IsValid())
				{
					asset.Value.ReleaseAsset();
				}
			}

			if (clearReferences)
			{
				_assetMap[typeof(TAsset)].Remove(typeof(TId));
			}
		}

		/// <inheritdoc />
		public void UnloadAssets<TId, TAsset>(bool clearReferences,
											  AssetConfigsScriptableObject<TId, TAsset> assetConfigs)
		{
			if (!TryGetDictionary<TId, TAsset>(out var dictionary))
			{
				Debug.LogWarning($"The {nameof(AssetResolverService)} does not have the '{typeof(TAsset)}' " +
					$"asset list for the '{typeof(TId)}' type. Use the {nameof(AddAssets)} method to add it.");
				return;
			}

			foreach (var pair in assetConfigs.Configs)
			{
				if (!dictionary.TryGetValue(pair.Key, out var asset))
				{
					continue;
				}

				if (asset.IsValid())
				{
					asset.ReleaseAsset();
				}

				if (clearReferences)
				{
					dictionary.Remove(pair.Key);
				}
			}
		}

		/// <inheritdoc />
		public void UnloadAssets<TId, TAsset>(bool clearReferences, params TId[] ids)
		{
			if(!TryGetDictionary<TId, TAsset>(out var dictionary))
			{
				Debug.LogWarning($"The {nameof(AssetResolverService)} does not have the '{typeof(TAsset)}' " +
					$"asset list for the '{typeof(TId)}' type. Use the {nameof(AddAssets)} method to add it.");
				return;
			}

			foreach (var id in ids)
			{
				if (!dictionary.TryGetValue(id, out var asset))
				{
					continue;
				}

				if (asset.IsValid())
				{
					asset.ReleaseAsset();
				}

				if (clearReferences)
				{
					dictionary.Remove(id);
				}
			}
		}

		/// <inheritdoc />
		public void AddAssets<TId>(Type assetType, List<Pair<TId, AssetReference>> assets)
		{
			var idType = typeof(TId);
			var assetReferences = new Dictionary<TId, AssetReference>(assets.Count);

			for (var i = 0; i < assets.Count; i++)
			{
				assetReferences.Add(assets[i].Key, assets[i].Value);
			}

			if (!_assetMap.TryGetValue(assetType, out var map))
			{
				map = new Dictionary<Type, IDictionary>();

				_assetMap.Add(assetType, map);
			}

			if (map.TryGetValue(idType, out var assetMap))
			{
				var dictionary = assetMap as Dictionary<TId, AssetReference>;

				// ReSharper disable once PossibleNullReferenceException
				foreach (var asset in dictionary)
				{
					assetReferences.Add(asset.Key, asset.Value);
				}

				map[idType] = assetReferences;
			}
			else
			{
				map.Add(idType, assetReferences);
			}
		}

		/// <inheritdoc />
		public void AddAsset<TId>(Type assetType, TId id, AssetReference assetReference)
		{
			AddAssets(assetType, new List<Pair<TId, AssetReference>> { new Pair<TId, AssetReference>(id, assetReference) });
		}

		/// <inheritdoc />
		public void AddDebugConfigs(Sprite errorSprite = null, GameObject errorCube = null, Material errorMaterial = null, 
									AudioClip errorClip = null)
		{
			_errorSprite = errorSprite;
			_errorCube = errorCube;
			_errorMaterial = errorMaterial;
			_errorClip = errorClip;
		}

		private TAsset Convert<TAsset>(AssetReference assetReference, bool instantiate)
			where TAsset : Object
		{
			var type = typeof(TAsset);

			/* AssetReference types
			
				GameObject
				ScriptableObject
				Texture
				Texture3D
				Texture2D
				RenderTexture
				CustomRenderTexture
				CubeMap
				Material
				PhysicMaterial
				PhysicMaterial2D
				Sprite
				SpriteAtlas
				VideoClip
				AudioClip
				AudioMixer
				Avatar
				AnimatorController
				AnimatorOverrideController
				TextAsset
				Mesh
				Shader
				ComputeShader
				Flare
				NavMeshData
				TerrainData
				TerrainLayer
				Font
				Scene
				GUISkin
			 * */

			if (assetReference.Asset == null)
			{
				return null;
			}

			if (type == typeof(GameObject))
			{
				var asset = !assetReference.IsDone ? _errorCube : assetReference.Asset as GameObject;

				return instantiate ? Object.Instantiate(asset) as TAsset : asset as TAsset;
			}

			if (type == typeof(Sprite))
			{
				return !assetReference.IsDone ? _errorSprite as TAsset : assetReference.Asset as TAsset;
			}

			if (type == typeof(Material))
			{
				var asset = !assetReference.IsDone ? _errorMaterial : assetReference.Asset as Material;

				return instantiate ? new Material(asset) as TAsset : asset as TAsset;
			}

			if (type == typeof(AudioClip))
			{
				return !assetReference.IsDone ? _errorClip as TAsset : assetReference.Asset as TAsset;
			}

			return assetReference.Asset as TAsset;
		}

		private bool TryGetDictionary<TId, TAsset>(out Dictionary<TId, AssetReference> dictionary)
		{
			dictionary = null;

			if (!_assetMap.TryGetValue(typeof(TAsset), out var idMap))
			{
				return false;
			}

			if (!idMap.TryGetValue(typeof(TId), out var assetMap))
			{
				return false;
			}

			dictionary = assetMap as Dictionary<TId, AssetReference>;

			return true;
		}

		private bool TryGetAssetReference<TId, TAsset>(TId id, out AssetReference assetReference)
		{
			if (!TryGetDictionary<TId, TAsset>(out var dictionary))
			{
				assetReference = default;

				return false;
			}

			return dictionary.TryGetValue(id, out assetReference);
		}
	}
}