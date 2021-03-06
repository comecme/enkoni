﻿using System.Configuration;
using System.Xml;

using Enkoni.Framework.Validation.Properties;

namespace Enkoni.Framework.Validation.Validators.Configuration {
  /// <summary>Defines the configuration element that can be used to configure the <see cref="DutchPhoneNumberValidator"/>.</summary>
  public class DutchPhoneNumberValidatorConfigElement : ConfigurationElement {
    #region Constructor

    /// <summary>Initializes a new instance of the <see cref="DutchPhoneNumberValidatorConfigElement"/> class.</summary>
    public DutchPhoneNumberValidatorConfigElement() {
      string[] defaultAreaCodes = Resources.AreaCodes_NL.Split(';');
      foreach(string areaCode in defaultAreaCodes) {
        DutchPhoneNumberAreaCodeConfigElement areaElement = new DutchPhoneNumberAreaCodeConfigElement("0" + areaCode);
        this.AreaCodes.Add(areaElement);
      }
    }

    #endregion

    #region Properties

    /// <summary>Gets or sets the name of the validator.</summary>
    [ConfigurationProperty("name", IsKey = true, IsRequired = false, DefaultValue = DutchPhoneNumberValidator.DefaultName)]
    public string Name {
      get { return (string)this["name"]; }
      set { this["name"] = value; }
    }

    /// <summary>Gets or sets a value indicating whether country calling codes are allowed by the validator.</summary>
    [ConfigurationProperty("allowCountryCallingCode", IsKey = true, IsRequired = false, DefaultValue = true)]
    public bool AllowCountryCallingCode {
      get { return (bool)this["allowCountryCallingCode"]; }
      set { this["allowCountryCallingCode"] = value; }
    }

    /// <summary>Gets or sets a value indicating whether carrier preselect codes are allowed by the validator.</summary>
    [ConfigurationProperty("allowCarrierPreselect", IsKey = true, IsRequired = false, DefaultValue = false)]
    public bool AllowCarrierPreselect {
      get { return (bool)this["allowCarrierPreselect"]; }
      set { this["allowCarrierPreselect"] = value; }
    }

    /// <summary>Gets the collection of countries that must be used by the validator.</summary>
    [ConfigurationProperty("areaCodes", IsRequired = false, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(DutchPhoneNumberAreaCodeCollection), AddItemName = "add", RemoveItemName = "remove", ClearItemsName = "clear")]
    public DutchPhoneNumberAreaCodeCollection AreaCodes {
      get {
        DutchPhoneNumberAreaCodeCollection areaCodes = (DutchPhoneNumberAreaCodeCollection)this["areaCodes"];
        return areaCodes;
      }
    }

    #endregion

    #region Internal methods

    /// <summary>Reads XML from the configuration file.</summary>
    /// <param name="reader">The <see cref="XmlReader"/> that reads from the configuration file.</param>
    /// <param name="serializeCollectionKey"><see langword="true"/> to serialize only the collection key properties; otherwise,
    /// <see langword="false"/>.</param>
    internal void ReadFromConfig(XmlReader reader, bool serializeCollectionKey) {
      this.DeserializeElement(reader, serializeCollectionKey);
    }

    #endregion
  }
}
