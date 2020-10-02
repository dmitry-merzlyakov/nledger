// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Utility.Settings.CascadeSettings;
using NLedger.Utility.Settings.CascadeSettings.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.Settings
{
    /// <summary>
    /// Represents a collection of effective NLedger settings
    /// </summary>
    public class NLedgerConfiguration
    {
        public NLedgerConfiguration(IEnumerable<ISettingDefinition> externalDefinitions = null)
        {
            var isAttyDefinition = AddDefinition(new BoolSettingDefinition("IsAtty", IsAttyDescription, true));
            var isTimeZoneIdDefinition = AddDefinition(new TimeZoneSettingDefinition("TimeZoneId", TimeZoneDescription, TimeZoneInfo.Local));
            var outputEncodingDefinition = AddDefinition(new EncodingSettingDefinition("OutputEncoding", OutputEncodingDescription, Encoding.Default));
            var ansiTerminalEmulationDefinition = AddDefinition(new BoolSettingDefinition("AnsiTerminalEmulation", AnsiTerminalEmulationDescription, true));
            var defaultPagerDefinition = AddDefinition(new StringSettingDefinition("DefaultPager", DefaultPagerDescription));
            var disableUserSettingsDefinition = AddDefinition(new BoolSettingDefinition("DisableUserSettings", DisableUserSettingsDescription, false, SettingScopeEnum.Application));

            if (externalDefinitions != null && externalDefinitions.Any())
                Definitions = Definitions.Concat(externalDefinitions).ToList();

            SettingsContainer = new NLedgerSettingsContainer(Definitions);

            IsAtty = new SettingValue<bool>(SettingsContainer, isAttyDefinition);
            TimeZoneId = new SettingValue<TimeZoneInfo>(SettingsContainer, isTimeZoneIdDefinition);
            OutputEncoding = new SettingValue<Encoding>(SettingsContainer, outputEncodingDefinition);
            AnsiTerminalEmulation = new SettingValue<bool>(SettingsContainer, ansiTerminalEmulationDefinition);
            DefaultPager = new SettingValue<string>(SettingsContainer, defaultPagerDefinition);
            DisableUserSettings = new SettingValue<bool>(SettingsContainer, disableUserSettingsDefinition);

            SettingsContainer.EffectiveScope = DisableUserSettings.Value ? SettingScopeEnum.Application : SettingScopeEnum.User;
        }

        public NLedgerSettingsContainer SettingsContainer { get; private set; }
        public IList<ISettingDefinition> Definitions { get; private set; } = new List<ISettingDefinition>();

        public SettingValue<bool> IsAtty { get; private set; }
        public SettingValue<TimeZoneInfo> TimeZoneId { get; private set; }
        public SettingValue<Encoding> OutputEncoding { get; private set; }
        public SettingValue<bool> AnsiTerminalEmulation { get; private set; }
        public SettingValue<string> DefaultPager { get; private set; }
        public SettingValue<bool> DisableUserSettings { get; private set; }

        /// <summary>
        /// Popupates the main application context with effective settings for a console application
        /// </summary>
        public MainApplicationContext CreateConsoleApplicationContext()
        {
            Console.OutputEncoding = OutputEncoding.Value;
            if (AnsiTerminalEmulation.Value)
                AnsiTextWriter.Attach();

            var context = new MainApplicationContext()
            {
                IsAtty = IsAtty.Value,
                TimeZone = TimeZoneId.Value,
                DefaultPager = DefaultPager.Value
            };

            context.SetEnvironmentVariables(SettingsContainer.VarSettings.EnvironmentVariables);
            return context;
        }

        private T AddDefinition<T>(T definition) where T: ISettingDefinition
        {
            Definitions.Add(definition);
            return definition;
        }

        private static readonly string IsAttyDescription = "Specifies whether the output console supports extended ATTY/VT100 functions.";
        private static readonly string TimeZoneDescription = "Specifies the current time zone for date and time conversion. Default value reflects OS settings.";
        private static readonly string OutputEncodingDescription = "The name of the output encoding. By default, it uses your local console settings.";
        private static readonly string AnsiTerminalEmulationDescription = "Enables embedded managing of VT100 codes and colorizing the console output.";
        private static readonly string DefaultPagerDescription = "When this value is not empty and *IsAtty* is turned on, NLedger runs the pager and directs the output to its input stream.";
        private static readonly string DisableUserSettingsDescription = "Disables managing of NLedger settings by optional common and user configuration files.";
    }
}
