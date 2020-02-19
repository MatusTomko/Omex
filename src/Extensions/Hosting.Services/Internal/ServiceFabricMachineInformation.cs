﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal class ServiceFabricMachineInformation : EmptyMachineInformation
	{

		public ServiceFabricMachineInformation(IHostEnvironment hostEnvironment, IStatelessServiceContextAccessor accessor)
			: this(hostEnvironment, GetActivationContext(accessor), GetNodeContext(accessor))
		{
		}


		private ServiceFabricMachineInformation(IHostEnvironment hostEnvironment, ICodePackageActivationContext activationContext, NodeContext nodeContext)
		{
			MachineName = GetMachineName();
			DeploymentSlice = DefaultEmptyValue;
			IsCanary = false;
			MachineCount = 1;

			ServiceName = hostEnvironment.ApplicationName;
			MachineRole = activationContext.ApplicationName ?? DefaultEmptyValue;
			BuildVersion = activationContext.CodePackageVersion;

			MachineId = FormattableString.Invariant($"{MachineName}_{nodeContext.NodeName}");
			MachineClusterIpAddress = IPAddress.TryParse(nodeContext.IPAddressOrFQDN, out IPAddress ipAddress)
				? ipAddress
				: GetIpAddress(MachineName);

			EnvironmentName = hostEnvironment.EnvironmentName ?? DefaultEmptyValue;
			IsPrivateDeployment = hostEnvironment.IsDevelopment();
			RegionName = GetRegionName() ?? DefaultEmptyValue;
			MachineCluster = GetClusterName()
				?? nodeContext.IPAddressOrFQDN
				?? MachineId;
		}


		private string? GetRegionName() =>
			Environment.GetEnvironmentVariable("REGION_NAME"); // should be defined by Azure


		private string? GetClusterName() =>
			Environment.GetEnvironmentVariable("CLUSTER_NAME"); // We should define it


		private static ICodePackageActivationContext GetActivationContext(IStatelessServiceContextAccessor accessor) =>
			accessor.ServiceContext?.CodePackageActivationContext ?? FabricRuntime.GetActivationContext();


		private static NodeContext GetNodeContext(IStatelessServiceContextAccessor accessor) =>
			accessor.ServiceContext?.NodeContext ?? FabricRuntime.GetNodeContext();
	}
}
