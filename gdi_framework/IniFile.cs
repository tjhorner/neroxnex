namespace gdi_framework
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    public class IniFile
    {
        private Hashtable m_sections = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        public IniSection AddSection(string sSection)
        {
            IniSection section = null;
            sSection = sSection.Trim();
            if (this.m_sections.ContainsKey(sSection))
            {
                return (IniSection) this.m_sections[sSection];
            }
            section = new IniSection(this, sSection);
            this.m_sections[sSection] = section;
            return section;
        }

        public string GetKeyValue(string sSection, string sKey)
        {
            IniSection section = this.GetSection(sSection);
            if (section != null)
            {
                IniSection.IniKey key = section.GetKey(sKey);
                if (key != null)
                {
                    return key.Value;
                }
            }
            return string.Empty;
        }

        public IniSection GetSection(string sSection)
        {
            sSection = sSection.Trim();
            if (this.m_sections.ContainsKey(sSection))
            {
                return (IniSection) this.m_sections[sSection];
            }
            return null;
        }

        public void Load(string sFileName, bool bMerge = false)
        {
            if (!bMerge)
            {
                this.RemoveAllSections();
            }
            IniSection section = null;
            StreamReader reader = new StreamReader(sFileName);
            Regex regex = new Regex(@"^([\s]*#.*)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            Regex regex2 = new Regex(@"^[\s]*\[[\s]*([^\[\s].*[^\s\]])[\s]*\][\s]*$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            Regex regex3 = new Regex(@"^\s*([^=]*[^\s=])\s*=(.*)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            while (!reader.EndOfStream)
            {
                string input = reader.ReadLine();
                if (input != string.Empty)
                {
                    Match match = null;
                    if (regex.Match(input).Success)
                    {
                        match = regex.Match(input);
                        Trace.WriteLine($"Skipping Comment: {match.Groups[0].Value}");
                    }
                    else
                    {
                        if (regex2.Match(input).Success)
                        {
                            match = regex2.Match(input);
                            Trace.WriteLine($"Adding section [{match.Groups[1].Value}]");
                            section = this.AddSection(match.Groups[1].Value);
                            continue;
                        }
                        if (regex3.Match(input).Success && (section != null))
                        {
                            match = regex3.Match(input);
                            Trace.WriteLine($"Adding Key [{match.Groups[1].Value}]=[{match.Groups[2].Value}]");
                            section.AddKey(match.Groups[1].Value).Value = match.Groups[2].Value;
                            continue;
                        }
                        if (section != null)
                        {
                            Trace.WriteLine($"Adding Key [{input}]");
                            section.AddKey(input);
                            continue;
                        }
                        Trace.WriteLine($"Skipping unknown type of data: {input}");
                    }
                }
            }
            reader.Close();
        }

        public bool RemoveAllSections()
        {
            this.m_sections.Clear();
            return (this.m_sections.Count == 0);
        }

        public bool RemoveKey(string sSection, string sKey)
        {
            IniSection section = this.GetSection(sSection);
            return ((section != null) && section.RemoveKey(sKey));
        }

        public bool RemoveSection(IniSection Section)
        {
            if (Section != null)
            {
                try
                {
                    this.m_sections.Remove(Section.Name);
                    return true;
                }
                catch (Exception exception1)
                {
                    ProjectData.SetProjectError(exception1);
                    Exception exception = exception1;
                    Trace.WriteLine(exception.Message);
                    ProjectData.ClearProjectError();
                }
            }
            return false;
        }

        public bool RemoveSection(string sSection)
        {
            sSection = sSection.Trim();
            return this.RemoveSection(this.GetSection(sSection));
        }

        public bool RenameKey(string sSection, string sKey, string sNewKey)
        {
            IniSection section = this.GetSection(sSection);
            if (section != null)
            {
                IniSection.IniKey key = section.GetKey(sKey);
                if (key != null)
                {
                    return key.SetName(sNewKey);
                }
            }
            return false;
        }

        public bool RenameSection(string sSection, string sNewSection)
        {
            bool flag = false;
            IniSection section = this.GetSection(sSection);
            if (section != null)
            {
                flag = section.SetName(sNewSection);
            }
            return flag;
        }

        public void Save(string sFileName)
        {
            IEnumerator enumerator;
            StreamWriter writer = new StreamWriter(sFileName, false);
            try
            {
                enumerator = this.Sections.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    IEnumerator enumerator2;
                    IniSection current = (IniSection) enumerator.Current;
                    Trace.WriteLine($"Writing Section: [{current.Name}]");
                    writer.WriteLine($"[{current.Name}]");
                    try
                    {
                        enumerator2 = current.Keys.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            IniSection.IniKey key = (IniSection.IniKey) enumerator2.Current;
                            if (key.Value != string.Empty)
                            {
                                Trace.WriteLine($"Writing Key: {key.Name}={key.Value}");
                                writer.WriteLine($"{key.Name}={key.Value}");
                            }
                            else
                            {
                                Trace.WriteLine($"Writing Key: {key.Name}");
                                writer.WriteLine($"{key.Name}");
                            }
                        }
                        continue;
                    }
                    finally
                    {
                        if (enumerator2 is IDisposable)
                        {
                            (enumerator2 as IDisposable).Dispose();
                        }
                    }
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
            writer.Close();
        }

        public bool SetKeyValue(string sSection, string sKey, string sValue)
        {
            IniSection section = this.AddSection(sSection);
            if (section != null)
            {
                IniSection.IniKey key = section.AddKey(sKey);
                if (key != null)
                {
                    key.Value = sValue;
                    return true;
                }
            }
            return false;
        }

        public ICollection Sections =>
            this.m_sections.Values;

        public class IniSection
        {
            private Hashtable m_keys;
            private IniFile m_pIniFile;
            private string m_sSection;

            protected internal IniSection(IniFile parent, string sSection)
            {
                this.m_pIniFile = parent;
                this.m_sSection = sSection;
                this.m_keys = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            }

            public IniKey AddKey(string sKey)
            {
                sKey = sKey.Trim();
                IniKey key = null;
                if (sKey.Length != 0)
                {
                    if (this.m_keys.ContainsKey(sKey))
                    {
                        return (IniKey) this.m_keys[sKey];
                    }
                    key = new IniKey(this, sKey);
                    this.m_keys[sKey] = key;
                }
                return key;
            }

            public IniKey GetKey(string sKey)
            {
                sKey = sKey.Trim();
                if (this.m_keys.ContainsKey(sKey))
                {
                    return (IniKey) this.m_keys[sKey];
                }
                return null;
            }

            public string GetName() => 
                this.m_sSection;

            public bool RemoveAllKeys()
            {
                this.m_keys.Clear();
                return (this.m_keys.Count == 0);
            }

            public bool RemoveKey(IniKey Key)
            {
                if (Key != null)
                {
                    try
                    {
                        this.m_keys.Remove(Key.Name);
                        return true;
                    }
                    catch (Exception exception1)
                    {
                        ProjectData.SetProjectError(exception1);
                        Exception exception = exception1;
                        Trace.WriteLine(exception.Message);
                        ProjectData.ClearProjectError();
                    }
                }
                return false;
            }

            public bool RemoveKey(string sKey) => 
                this.RemoveKey(this.GetKey(sKey));

            public bool SetName(string sSection)
            {
                sSection = sSection.Trim();
                if (sSection.Length != 0)
                {
                    IniFile.IniSection section = this.m_pIniFile.GetSection(sSection);
                    if ((section != this) && (section != null))
                    {
                        return false;
                    }
                    try
                    {
                        this.m_pIniFile.m_sections.Remove(this.m_sSection);
                        this.m_pIniFile.m_sections[sSection] = this;
                        this.m_sSection = sSection;
                        return true;
                    }
                    catch (Exception exception1)
                    {
                        ProjectData.SetProjectError(exception1);
                        Exception exception = exception1;
                        Trace.WriteLine(exception.Message);
                        ProjectData.ClearProjectError();
                    }
                }
                return false;
            }

            public ICollection Keys =>
                this.m_keys.Values;

            public string Name =>
                this.m_sSection;

            public class IniKey
            {
                private IniFile.IniSection m_section;
                private string m_sKey;
                private string m_sValue;

                protected internal IniKey(IniFile.IniSection parent, string sKey)
                {
                    this.m_section = parent;
                    this.m_sKey = sKey;
                }

                public string GetName() => 
                    this.m_sKey;

                public string GetValue() => 
                    this.m_sValue;

                public bool SetName(string sKey)
                {
                    sKey = sKey.Trim();
                    if (sKey.Length != 0)
                    {
                        IniFile.IniSection.IniKey key = this.m_section.GetKey(sKey);
                        if ((key != this) && (key != null))
                        {
                            return false;
                        }
                        try
                        {
                            this.m_section.m_keys.Remove(this.m_sKey);
                            this.m_section.m_keys[sKey] = this;
                            this.m_sKey = sKey;
                            return true;
                        }
                        catch (Exception exception1)
                        {
                            ProjectData.SetProjectError(exception1);
                            Exception exception = exception1;
                            Trace.WriteLine(exception.Message);
                            ProjectData.ClearProjectError();
                        }
                    }
                    return false;
                }

                public void SetValue(string sValue)
                {
                    this.m_sValue = sValue;
                }

                public string Name =>
                    this.m_sKey;

                public string Value
                {
                    get => 
                        this.m_sValue;
                    set
                    {
                        this.m_sValue = value;
                    }
                }
            }
        }
    }
}

