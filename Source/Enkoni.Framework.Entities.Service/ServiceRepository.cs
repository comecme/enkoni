﻿using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Enkoni.Framework.Entities {
  /// <summary>This abstract class extends the abstract <see cref="Repository{T}"/> class and implements some of the functionality using WCF
  /// data communication. This implementation can be used a base for any WCF-service repositories.</summary>
  /// <typeparam name="TEntity">The type of the entity that is handled by this repository.</typeparam>
  public abstract class ServiceRepository<TEntity> : Repository<TEntity>
    where TEntity : class, new() {
    #region Constructor

    /// <summary>Initializes a new instance of the <see cref="ServiceRepository{TEntity}"/> class using the specified <see cref="DataSourceInfo"/>.
    /// </summary>
    /// <param name="dataSourceInfo">The data source information that must be used to access the source file.</param>
    protected ServiceRepository(DataSourceInfo dataSourceInfo)
      : base(dataSourceInfo) {
      /* Determine if the supported properties have been specified */
      if(ServiceSourceInfo.IsEndpointConfigurationNameSpecified(dataSourceInfo)) {
        this.EndpointConfigurationName = ServiceSourceInfo.SelectEndpointConfigurationName(dataSourceInfo);
      }

      if(ServiceSourceInfo.IsRemoteAddressSpecified(dataSourceInfo)) {
        this.RemoteAddress = ServiceSourceInfo.SelectRemoteAddress(dataSourceInfo);
      }

      if(ServiceSourceInfo.IsBindingSpecified(dataSourceInfo)) {
        this.Binding = ServiceSourceInfo.SelectBinding(dataSourceInfo);
      }
    }

    #endregion

    #region Protected properties

    /// <summary>Gets the endpoint configuration name that references the used endpoint configuration.</summary>
    protected string EndpointConfigurationName { get; private set; }

    /// <summary>Gets the address of the remote service.</summary>
    protected EndpointAddress RemoteAddress { get; private set; }

    /// <summary>Gets the binding that is used to communicate with the remote service.</summary>
    protected Binding Binding { get; private set; }

    #endregion

    #region Repository<T> overrides

    /// <summary>Creates a new entity of type <typeparamref name="TEntity"/>. This is done by calling the default constructor of
    /// <typeparamref name="TEntity"/>.</summary>
    /// <param name="dataSourceInfo">Information about the data source that may not have been set at an earlier stage.</param>
    /// <returns>The created entity.</returns>
    protected override TEntity CreateEntityCore(DataSourceInfo dataSourceInfo) {
      TEntity entity = new TEntity();
      return entity;
    }

    #endregion
  }
}
