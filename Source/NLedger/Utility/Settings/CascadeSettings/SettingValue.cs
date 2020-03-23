// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility.Settings.CascadeSettings.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Settings.CascadeSettings
{
    /// <summary>
    /// Represents an effective setting value
    /// </summary>
    /// <typeparam name="T">Type of setting</typeparam>
    public class SettingValue<T>
    {
        /// <summary>
        /// Instantiates a setting value obect
        /// </summary>
        /// <param name="settingsContainer">Settings container that returns and effective text value</param>
        /// <param name="definition">Value definition that specifies a value type and its default value</param>
        public SettingValue(CascadeSettingsContainer settingsContainer, BaseSettingDefinition<T> definition)
        {
            if (settingsContainer == null)
                throw new ArgumentNullException("settingsContainer");
            if (definition == null)
                throw new ArgumentNullException("definition");

            SettingsContainer = settingsContainer;
            Definition = definition;
        }

        public CascadeSettingsContainer SettingsContainer { get; private set; }
        public BaseSettingDefinition<T> Definition { get; private set; }

        /// <summary>
        /// Effective setting value
        /// </summary>
        /// <remarks>
        /// Raises InvalidOperationException if the effective value cannot be converted to the target type
        /// </remarks>
        public T Value
        {
            get
            {
                var effectiveValue = SettingsContainer.GetEffectiveValue(Definition.Name, Definition.Scope);
                try
                {
                    return Definition.ConvertFromString(effectiveValue);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(String.Format("Cannot get '{0}' setting value (effective value is '{1}'; expected type is '{2}')", Definition.Name, effectiveValue, typeof(T).Name), ex);
                }
            }
        }
    }
}
