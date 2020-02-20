﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Omex.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>Create wrapper from Service Fabric context to provide log information</summary>
	internal sealed class OmexServiceFabricContext : IServiceContext
	{
		/// <summary> Create OmexServiceFabricContext from StatelessServiceContextAccessor</summary>
		public OmexServiceFabricContext(IServiceContextAccessor<StatelessServiceContext> accessor)
			=> m_accessor = accessor;


		/// <summary> Create OmexServiceFabricContext from StatefulServiceContext</summary>
		public OmexServiceFabricContext(IServiceContextAccessor<StatefulServiceContext> accessor)
			=> m_accessor = accessor;


		/// <inheritdoc/>
		public Guid GetPartitionId() => GetValue(ref m_partitionId, m_accessor, s_partitionIdProvider);


		/// <inheritdoc/>
		public long GetReplicaOrInstanceId() => GetValue(ref m_replicaOrInstanceId, m_accessor, s_replicaProvider);


		private TResult GetValue<TSource, TResult>(ref TResult? storageField, TSource source, Func<TSource, TResult?> provider)
			where TResult: struct
		{
			if (storageField.HasValue)
			{
				return storageField.Value;
			}

			storageField = provider(source);
			return storageField.GetValueOrDefault();
		}


		private readonly IServiceContextAccessor<ServiceContext> m_accessor;
		private Guid? m_partitionId;
		private long? m_replicaOrInstanceId;

		private static Func<IServiceContextAccessor<ServiceContext>, Guid?> s_partitionIdProvider =>
			a => a.ServiceContext?.PartitionId;

		private static Func<IServiceContextAccessor<ServiceContext>, long?> s_replicaProvider =>
			a => a.ServiceContext?.ReplicaOrInstanceId;
	}
}
