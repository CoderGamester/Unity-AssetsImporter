using GameLovers.ConfigsProvider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable once CheckNamespace

namespace GameLovers.AssetsImporter
{
	/// <summary>
	/// Defines a basic contract for Asset config with reference to weak link assets
	/// </summary>
	public abstract class AssetConfigScriptableObject : ScriptableObject
	{
		[SerializeField] private string _assetsFolderPath;

		/// <summary>
		/// Returns the folder path of the assets to be referenced in this container
		/// </summary>
		public string AssetsFolderPath
		{
			get => _assetsFolderPath;
			set => _assetsFolderPath = value;
		}
	}

	/// <inheritdoc cref="AssetConfigsScriptableObject{TKey,TValue}"/>
	public abstract class AssetConfigsScriptableObject : AssetConfigScriptableObject
	{
		/// <summary>
		/// Returns the asset type of <see cref="AssetReference"/> that this container is holding
		/// </summary>
		public abstract Type AssetType { get; }
	}

	/// <inheritdoc cref="IPairConfigsContainer{TKey,TValue}"/>
	/// <remarks>
	/// Use this configs container to hold the configs data of assets of the given <typeparamref name="TAsset"/>
	/// mapped with the given <typeparamref name="TId"/>
	/// </remarks>
	public abstract class AssetConfigsScriptableObject<TId, TAsset> :
		AssetConfigsScriptableObject, IPairConfigsContainer<TId, AssetReference>, ISerializationCallbackReceiver
		where TId : struct
	{
		[SerializeField] private List<Pair<TId, AssetReference>> _configs = new();

		/// <inheritdoc />
		public override Type AssetType => typeof(TAsset);

		/// <inheritdoc />
		public List<Pair<TId, AssetReference>> Configs
		{
			get => _configs;
			set => _configs = value;
		}

		/// <summary>
		/// Requests the assets configs as a read only dictionary
		/// </summary>
		public IReadOnlyDictionary<TId, AssetReference> ConfigsDictionary { get; private set; }

		/// <inheritdoc />
		public void OnBeforeSerialize()
		{
			// Do Nothing
		}

		/// <inheritdoc />
		public virtual void OnAfterDeserialize()
		{
			var dictionary = new Dictionary<TId, AssetReference>();

			foreach (var config in Configs)
			{
				dictionary.Add(config.Key, config.Value);
			}

			ConfigsDictionary = new ReadOnlyDictionary<TId, AssetReference>(dictionary);
		}
	}

	/// <inheritdoc cref="IPairConfigsContainer{TKey,TValue}"/>
	/// <remarks>
	/// Use this configs container to hold the configs data of assets of the given <typeparamref name="TAsset"/>
	/// mapped with the given <typeparamref name="TId"/>
	/// </remarks>
	public abstract class AssetConfigsScriptableObjectSimple<TId, TAsset> :
		AssetConfigsScriptableObject, IPairConfigsContainer<TId, TAsset>, ISerializationCallbackReceiver
		where TId : struct
	{
		[SerializeField] private List<Pair<TId, TAsset>> _configs = new();

		/// <inheritdoc />
		public override Type AssetType => typeof(TAsset);

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
}