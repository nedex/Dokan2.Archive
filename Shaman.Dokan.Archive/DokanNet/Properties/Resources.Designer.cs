﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Shaman.Dokan.Archive.DokanNet.Properties {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Shaman.Dokan.Archive.DokanNet.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 Can&apos;t assign a drive letter or mount point 的本地化字符串。
        /// </summary>
        public static string ErrorAssignDriveLetter {
            get {
                return ResourceManager.GetString("ErrorAssignDriveLetter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Bad drive letter 的本地化字符串。
        /// </summary>
        public static string ErrorBadDriveLetter {
            get {
                return ResourceManager.GetString("ErrorBadDriveLetter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Dokan error 的本地化字符串。
        /// </summary>
        public static string ErrorDokan {
            get {
                return ResourceManager.GetString("ErrorDokan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Can&apos;t install the Dokan driver 的本地化字符串。
        /// </summary>
        public static string ErrorDriverInstall {
            get {
                return ResourceManager.GetString("ErrorDriverInstall", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Mount point is invalid 的本地化字符串。
        /// </summary>
        public static string ErrorMountPointInvalid {
            get {
                return ResourceManager.GetString("ErrorMountPointInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Something&apos;s wrong with the Dokan driver 的本地化字符串。
        /// </summary>
        public static string ErrorStart {
            get {
                return ResourceManager.GetString("ErrorStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Unknown error 的本地化字符串。
        /// </summary>
        public static string ErrorUnknown {
            get {
                return ResourceManager.GetString("ErrorUnknown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Version error 的本地化字符串。
        /// </summary>
        public static string ErrorVersion {
            get {
                return ResourceManager.GetString("ErrorVersion", resourceCulture);
            }
        }
    }
}