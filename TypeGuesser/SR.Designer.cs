﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TypeGuesser {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SR {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SR() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TypeGuesser.SR", typeof(SR).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not combine Types &apos;{0}&apos; and &apos;{1}&apos; because they were of differing Types and neither Type appeared in the PreferenceOrder.
        /// </summary>
        internal static string DatabaseTypeRequest_Max_Could_not_combine_Types___0___and___1___because_they_were_of_differing_Types_and_neither_Type_appeared_in_the_PreferenceOrder {
            get {
                return ResourceManager.GetString("DatabaseTypeRequest_Max_Could_not_combine_Types___0___and___1___because_they_were" +
                        "_of_differing_Types_and_neither_Type_appeared_in_the_PreferenceOrder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not parse &apos;{0}&apos; to a valid DateTime.
        /// </summary>
        internal static string DateTimeTypeDecider_ParseImpl_Could_not_parse___0___to_a_valid_DateTime {
            get {
                return ResourceManager.GetString("DateTimeTypeDecider_ParseImpl_Could_not_parse___0___to_a_valid_DateTime", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DecideTypesForStrings abstract base was not passed any typesSupported by implementing derived class.
        /// </summary>
        internal static string DecideTypesForStrings_DecideTypesForStrings_DecideTypesForStrings_abstract_base_was_not_passed_any_typesSupported_by_implementing_derived_class {
            get {
                return ResourceManager.GetString("DecideTypesForStrings_DecideTypesForStrings_DecideTypesForStrings_abstract_base_w" +
                        "as_not_passed_any_typesSupported_by_implementing_derived_class", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not parse string value &apos;{0}&apos; with Decider Type:{1}.
        /// </summary>
        internal static string DecideTypesForStrings_Parse_Could_not_parse_string_value___0___with_Decider_Type__1_ {
            get {
                return ResourceManager.GetString("DecideTypesForStrings_Parse_Could_not_parse_string_value___0___with_Decider_Type_" +
                        "_1_", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Guesser does not support being passed hard typed objects (e.g. int) mixed with untyped objects (e.g. string).  We were adjusting to compensate for object &apos;{0}&apos; which is of Type &apos;{1}&apos;, we were previously passed a &apos;{2}&apos; type.
        /// </summary>
        internal static string Guesser_AdjustToCompensateForValue_GuesserPassedMixedTypeValues {
            get {
                return ResourceManager.GetString("Guesser_AdjustToCompensateForValue_GuesserPassedMixedTypeValues", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No Type Decider exists for Type:{0}.
        /// </summary>
        internal static string Guesser_ThrowIfNotSupported_No_Type_Decider_exists_for_Type__0_ {
            get {
                return ResourceManager.GetString("Guesser_ThrowIfNotSupported_No_Type_Decider_exists_for_Type__0_", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DataType {0} does not have an associated IDecideTypesForStrings.
        /// </summary>
        internal static string TypeDeciderFactory_Create_DataType__0__does_not_have_an_associated_IDecideTypesForStrings {
            get {
                return ResourceManager.GetString("TypeDeciderFactory_Create_DataType__0__does_not_have_an_associated_IDecideTypesFo" +
                        "rStrings", resourceCulture);
            }
        }
    }
}
