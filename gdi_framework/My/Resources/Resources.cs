namespace gdi_framework.My.Resources
{
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [StandardModule, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated, HideModuleName]
    internal sealed class Resources
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => 
                resourceCulture;
            set
            {
                resourceCulture = value;
            }
        }

        internal static Icon icon =>
            ((Icon) RuntimeHelpers.GetObjectValue(ResourceManager.GetObject("icon", resourceCulture)));

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    resourceMan = new System.Resources.ResourceManager("gdi_framework.Resources", typeof(gdi_framework.My.Resources.Resources).Assembly);
                }
                return resourceMan;
            }
        }
    }
}

