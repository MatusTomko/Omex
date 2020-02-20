﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	/// <summary>
	/// Creates ServiceInstanceListener with all of Omex dependencies initialized
	/// </summary>
	internal sealed class KestrelListenerBuilder<TStartup, TServiceContext> : IListenerBuilder<TServiceContext>
		where TStartup : class
		where TServiceContext : ServiceContext
	{
		public string Name { get; }


		public ICommunicationListener Build(TServiceContext context) =>
			new KestrelCommunicationListener(context, Name, (url, listener) => BuildWebHost(context, url, listener));


		internal KestrelListenerBuilder(
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder> builderExtension)
		{
			Name = name;
			m_options = options;
			m_builderExtension = builderExtension;
		}

		private readonly ServiceFabricIntegrationOptions m_options;
		private readonly Action<IWebHostBuilder> m_builderExtension;


		private void ConfigureServices(TServiceContext context, IServiceCollection services)
		{
			services.AddSingleton<IServiceContextAccessor<TServiceContext>>(new ServiceContextAccessor(context));
			services.AddSingleton<ServiceContext>(context);
			services.AddSingleton(context);
		}


		// Method made internal instead of private to check type registration in service collection
		internal IWebHost BuildWebHost(TServiceContext context, string url, AspNetCoreCommunicationListener listener)
		{
			IWebHostBuilder hostBuilder = new WebHostBuilder()
				.UseKestrel()
				.ConfigureServices(collection => ConfigureServices(context, collection))
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<TStartup>()
				.UseServiceFabricIntegration(listener, m_options)
				.UseUrls(url)
				.UseDefaultServiceProvider(config =>
				{
					config.ValidateOnBuild = true;
					config.ValidateScopes = true;
				});

			m_builderExtension(hostBuilder);

			return hostBuilder.Build();
		}

		private class ServiceContextAccessor : IServiceContextAccessor<TServiceContext>
		{
			public TServiceContext? ServiceContext { get; }

			public ServiceContextAccessor(TServiceContext serviceContext) => ServiceContext = serviceContext;
		}
	}
}
