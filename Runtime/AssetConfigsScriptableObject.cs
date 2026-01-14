using GameLovers.GameData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable once CheckNamespace

namespace GameLovers.AssetsImporter
{
	/// <summary>
	/// Abstract base class for asset configuration scriptable objects.
	/// Provides a basic contract for asset configurations with reference to weak link assets.
	/// </summary>
	public abstract class AssetConfigsScriptableObject : ScriptableObject
	{
		[SerializeField] private string _assetsFolderPath;

		/// <summary>
		/// Gets the type of asset that this scriptable object is configured for.
		/// </summary>
		public abstract Type AssetType { get; }

		/// <summary>
		/// Returns the folder path of the assets to be referenced in this container.
		/// </summary>
		public string AssetsFolderPath
		{
			get => _assetsFolderPath;
			set => _assetsFolderPath = value;
		}
	}
	/// <summary>
	/// Abstract base class for asset configuration scriptable objects.
	/// Provides a basic contract for asset configurations with reference to weak link assets.
	/// </summary>
	/// <typeparam name="TId">The type of the identifier, which must be a struct.</typeparam>
	/// <typeparam name="TAsset">The type of the asset.</typeparam>
	public abstract class AssetConfigsScriptableObjectBase<TId, TAsset> :
		AssetConfigsScriptableObject, IPairConfigsContainer<TId, TAsset>, ISerializationCallbackReceiver
	{
		[SerializeField] private List<Pair<TId, TAsset>> _configs = new();

		/// <inheritdoc />
		public List<Pair<TId, TAsset>> Configs
		{
			get => _configs;
			set => _configs = value;
		}

		/// <summary>
		/// Requests the assets configs as a read only dictionary
		/// </summary>
		public IReadOnlyDictionary<TId, TAsset> ConfigsDictionary { get; private set; }

		/// <inheritdoc />
		public void OnBeforeSerialize()
		{
			// Do Nothing
		}

		/// <inheritdoc />
		public virtual void OnAfterDeserialize()
		{
			var dictionary = new Dictionary<TId, TAsset>();

			foreach (var config in Configs)
			{
				dictionary.Add(config.Key, config.Value);
			}

			ConfigsDictionary = new ReadOnlyDictionary<TId, TAsset>(dictionary);
		}
	}

	/// <inheritdoc />
	public abstract class AssetConfigsScriptableObject<TId, TAsset> :
		AssetConfigsScriptableObjectBase<TId, AssetReference>
	{
		/// <inheritdoc />
		public override Type AssetType => typeof(TAsset);
	}
}