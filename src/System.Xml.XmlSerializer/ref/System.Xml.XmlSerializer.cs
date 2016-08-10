// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ------------------------------------------------------------------------------
// Changes to this file must follow the http://aka.ms/api-review process.
// ------------------------------------------------------------------------------


namespace System.Xml.Serialization
{
    [System.AttributeUsageAttribute((System.AttributeTargets)(10624), AllowMultiple = false)]
    public partial class XmlAnyAttributeAttribute : System.Attribute
    {
        public XmlAnyAttributeAttribute() { }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(10624), AllowMultiple = true)]
    public partial class XmlAnyElementAttribute : System.Attribute
    {
        public XmlAnyElementAttribute() { }
        public XmlAnyElementAttribute(string name) { }
        public XmlAnyElementAttribute(string name, string ns) { }
        public string Name { get { return default(string); } set { } }
        public string Namespace { get { return default(string); } set { } }
        public int Order { get { return default(int); } set { } }
    }
    public partial class XmlAnyElementAttributes
    {
        public XmlAnyElementAttributes() { }
        public System.Xml.Serialization.XmlAnyElementAttribute this[int index] { get { return default(System.Xml.Serialization.XmlAnyElementAttribute); } set { } }
        public int Add(System.Xml.Serialization.XmlAnyElementAttribute attribute) { return default(int); }
        public bool Contains(System.Xml.Serialization.XmlAnyElementAttribute attribute) { return default(bool); }
        public void CopyTo(System.Xml.Serialization.XmlAnyElementAttribute[] array, int index) { }
        public int IndexOf(System.Xml.Serialization.XmlAnyElementAttribute attribute) { return default(int); }
        public void Insert(int index, System.Xml.Serialization.XmlAnyElementAttribute attribute) { }
        public void Remove(System.Xml.Serialization.XmlAnyElementAttribute attribute) { }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(10624), AllowMultiple = false)]
    public partial class XmlArrayAttribute : System.Attribute
    {
        public XmlArrayAttribute() { }
        public XmlArrayAttribute(string elementName) { }
        public string ElementName { get { return default(string); } set { } }
        public System.Xml.Schema.XmlSchemaForm Form { get { return default(System.Xml.Schema.XmlSchemaForm); } set { } }
        public bool IsNullable { get { return default(bool); } set { } }
        public string Namespace { get { return default(string); } set { } }
        public int Order { get { return default(int); } set { } }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(10624), AllowMultiple = true)]
    public partial class XmlArrayItemAttribute : System.Attribute
    {
        public XmlArrayItemAttribute() { }
        public XmlArrayItemAttribute(string elementName) { }
        public XmlArrayItemAttribute(string elementName, System.Type type) { }
        public XmlArrayItemAttribute(System.Type type) { }
        public string DataType { get { return default(string); } set { } }
        public string ElementName { get { return default(string); } set { } }
        public System.Xml.Schema.XmlSchemaForm Form { get { return default(System.Xml.Schema.XmlSchemaForm); } set { } }
        public bool IsNullable { get { return default(bool); } set { } }
        public string Namespace { get { return default(string); } set { } }
        public int NestingLevel { get { return default(int); } set { } }
        public System.Type Type { get { return default(System.Type); } set { } }
    }
    public partial class XmlArrayItemAttributes
    {
        public XmlArrayItemAttributes() { }
        public System.Xml.Serialization.XmlArrayItemAttribute this[int index] { get { return default(System.Xml.Serialization.XmlArrayItemAttribute); } set { } }
        public int Add(System.Xml.Serialization.XmlArrayItemAttribute attribute) { return default(int); }
        public bool Contains(System.Xml.Serialization.XmlArrayItemAttribute attribute) { return default(bool); }
        public void CopyTo(System.Xml.Serialization.XmlArrayItemAttribute[] array, int index) { }
        public int IndexOf(System.Xml.Serialization.XmlArrayItemAttribute attribute) { return default(int); }
        public void Insert(int index, System.Xml.Serialization.XmlArrayItemAttribute attribute) { }
        public void Remove(System.Xml.Serialization.XmlArrayItemAttribute attribute) { }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(10624))]
    public partial class XmlAttributeAttribute : System.Attribute
    {
        public XmlAttributeAttribute() { }
        public XmlAttributeAttribute(string attributeName) { }
        public XmlAttributeAttribute(string attributeName, System.Type type) { }
        public XmlAttributeAttribute(System.Type type) { }
        public string AttributeName { get { return default(string); } set { } }
        public string DataType { get { return default(string); } set { } }
        public System.Xml.Schema.XmlSchemaForm Form { get { return default(System.Xml.Schema.XmlSchemaForm); } set { } }
        public string Namespace { get { return default(string); } set { } }
        public System.Type Type { get { return default(System.Type); } set { } }
    }
    public partial class XmlAttributeOverrides
    {
        public XmlAttributeOverrides() { }
        public System.Xml.Serialization.XmlAttributes this[System.Type type] { get { return default(System.Xml.Serialization.XmlAttributes); } }
        public System.Xml.Serialization.XmlAttributes this[System.Type type, string member] { get { return default(System.Xml.Serialization.XmlAttributes); } }
        public void Add(System.Type type, string member, System.Xml.Serialization.XmlAttributes attributes) { }
        public void Add(System.Type type, System.Xml.Serialization.XmlAttributes attributes) { }
    }
    public partial class XmlAttributes
    {
        public XmlAttributes() { }
        public System.Xml.Serialization.XmlAnyAttributeAttribute XmlAnyAttribute { get { return default(System.Xml.Serialization.XmlAnyAttributeAttribute); } set { } }
        public System.Xml.Serialization.XmlAnyElementAttributes XmlAnyElements { get { return default(System.Xml.Serialization.XmlAnyElementAttributes); } }
        public System.Xml.Serialization.XmlArrayAttribute XmlArray { get { return default(System.Xml.Serialization.XmlArrayAttribute); } set { } }
        public System.Xml.Serialization.XmlArrayItemAttributes XmlArrayItems { get { return default(System.Xml.Serialization.XmlArrayItemAttributes); } }
        public System.Xml.Serialization.XmlAttributeAttribute XmlAttribute { get { return default(System.Xml.Serialization.XmlAttributeAttribute); } set { } }
        public System.Xml.Serialization.XmlChoiceIdentifierAttribute XmlChoiceIdentifier { get { return default(System.Xml.Serialization.XmlChoiceIdentifierAttribute); } }
        public object XmlDefaultValue { get { return default(object); } set { } }
        public System.Xml.Serialization.XmlElementAttributes XmlElements { get { return default(System.Xml.Serialization.XmlElementAttributes); } }
        public System.Xml.Serialization.XmlEnumAttribute XmlEnum { get { return default(System.Xml.Serialization.XmlEnumAttribute); } set { } }
        public bool XmlIgnore { get { return default(bool); } set { } }
        public bool Xmlns { get { return default(bool); } set { } }
        public System.Xml.Serialization.XmlRootAttribute XmlRoot { get { return default(System.Xml.Serialization.XmlRootAttribute); } set { } }
        public System.Xml.Serialization.XmlTextAttribute XmlText { get { return default(System.Xml.Serialization.XmlTextAttribute); } set { } }
        public System.Xml.Serialization.XmlTypeAttribute XmlType { get { return default(System.Xml.Serialization.XmlTypeAttribute); } set { } }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(10624), AllowMultiple = false)]
    public partial class XmlChoiceIdentifierAttribute : System.Attribute
    {
        public XmlChoiceIdentifierAttribute() { }
        public XmlChoiceIdentifierAttribute(string name) { }
        public string MemberName { get { return default(string); } set { } }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(10624), AllowMultiple = true)]
    public partial class XmlElementAttribute : System.Attribute
    {
        public XmlElementAttribute() { }
        public XmlElementAttribute(string elementName) { }
        public XmlElementAttribute(string elementName, System.Type type) { }
        public XmlElementAttribute(System.Type type) { }
        public string DataType { get { return default(string); } set { } }
        public string ElementName { get { return default(string); } set { } }
        public System.Xml.Schema.XmlSchemaForm Form { get { return default(System.Xml.Schema.XmlSchemaForm); } set { } }
        public bool IsNullable { get { return default(bool); } set { } }
        public string Namespace { get { return default(string); } set { } }
        public int Order { get { return default(int); } set { } }
        public System.Type Type { get { return default(System.Type); } set { } }
    }
    public partial class XmlElementAttributes
    {
        public XmlElementAttributes() { }
        public System.Xml.Serialization.XmlElementAttribute this[int index] { get { return default(System.Xml.Serialization.XmlElementAttribute); } set { } }
        public int Add(System.Xml.Serialization.XmlElementAttribute attribute) { return default(int); }
        public bool Contains(System.Xml.Serialization.XmlElementAttribute attribute) { return default(bool); }
        public void CopyTo(System.Xml.Serialization.XmlElementAttribute[] array, int index) { }
        public int IndexOf(System.Xml.Serialization.XmlElementAttribute attribute) { return default(int); }
        public void Insert(int index, System.Xml.Serialization.XmlElementAttribute attribute) { }
        public void Remove(System.Xml.Serialization.XmlElementAttribute attribute) { }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(256))]
    public partial class XmlEnumAttribute : System.Attribute
    {
        public XmlEnumAttribute() { }
        public XmlEnumAttribute(string name) { }
        public string Name { get { return default(string); } set { } }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(10624))]
    public partial class XmlIgnoreAttribute : System.Attribute
    {
        public XmlIgnoreAttribute() { }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(1100), AllowMultiple = true)]
    public partial class XmlIncludeAttribute : System.Attribute
    {
        public XmlIncludeAttribute(System.Type type) { }
        public System.Type Type { get { return default(System.Type); } set { } }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(10624), AllowMultiple = false)]
    public partial class XmlNamespaceDeclarationsAttribute : System.Attribute
    {
        public XmlNamespaceDeclarationsAttribute() { }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(9244))]
    public partial class XmlRootAttribute : System.Attribute
    {
        public XmlRootAttribute() { }
        public XmlRootAttribute(string elementName) { }
        public string DataType { get { return default(string); } set { } }
        public string ElementName { get { return default(string); } set { } }
        public bool IsNullable { get { return default(bool); } set { } }
        public string Namespace { get { return default(string); } set { } }
    }
    public partial class XmlSerializer
    {
        protected XmlSerializer() { }
        public XmlSerializer(System.Type type) { }
        public XmlSerializer(System.Type type, string defaultNamespace) { }
        public XmlSerializer(System.Type type, System.Type[] extraTypes) { }
        public XmlSerializer(System.Type type, System.Xml.Serialization.XmlAttributeOverrides overrides) { }
        public XmlSerializer(System.Type type, System.Xml.Serialization.XmlAttributeOverrides overrides, System.Type[] extraTypes, System.Xml.Serialization.XmlRootAttribute root, string defaultNamespace) { }
        public XmlSerializer(System.Type type, System.Xml.Serialization.XmlRootAttribute root) { }
        public virtual bool CanDeserialize(System.Xml.XmlReader xmlReader) { return default(bool); }
        public object Deserialize(System.IO.Stream stream) { return default(object); }
        public object Deserialize(System.IO.TextReader textReader) { return default(object); }
        public object Deserialize(System.Xml.XmlReader xmlReader) { return default(object); }
        public static System.Xml.Serialization.XmlSerializer[] FromTypes(System.Type[] types) { return default(System.Xml.Serialization.XmlSerializer[]); }
        public void Serialize(System.IO.Stream stream, object o) { }
        public void Serialize(System.IO.Stream stream, object o, System.Xml.Serialization.XmlSerializerNamespaces namespaces) { }
        public void Serialize(System.IO.TextWriter textWriter, object o) { }
        public void Serialize(System.IO.TextWriter textWriter, object o, System.Xml.Serialization.XmlSerializerNamespaces namespaces) { }
        public void Serialize(System.Xml.XmlWriter xmlWriter, object o) { }
        public void Serialize(System.Xml.XmlWriter xmlWriter, object o, System.Xml.Serialization.XmlSerializerNamespaces namespaces) { }
    }
    public partial class XmlSerializerNamespaces
    {
        public XmlSerializerNamespaces() { }
        public XmlSerializerNamespaces(System.Xml.Serialization.XmlSerializerNamespaces namespaces) { }
        public XmlSerializerNamespaces(System.Xml.XmlQualifiedName[] namespaces) { }
        public int Count { get { return default(int); } }
        public void Add(string prefix, string ns) { }
        public System.Xml.XmlQualifiedName[] ToArray() { return default(System.Xml.XmlQualifiedName[]); }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(10624))]
    public partial class XmlTextAttribute : System.Attribute
    {
        public XmlTextAttribute() { }
        public XmlTextAttribute(System.Type type) { }
        public string DataType { get { return default(string); } set { } }
        public System.Type Type { get { return default(System.Type); } set { } }
    }
    [System.AttributeUsageAttribute((System.AttributeTargets)(1052))]
    public partial class XmlTypeAttribute : System.Attribute
    {
        public XmlTypeAttribute() { }
        public XmlTypeAttribute(string typeName) { }
        public bool AnonymousType { get { return default(bool); } set { } }
        public bool IncludeInSchema { get { return default(bool); } set { } }
        public string Namespace { get { return default(string); } set { } }
        public string TypeName { get { return default(string); } set { } }
    }
    public partial class XmlMemberMapping
    {
        internal XmlMemberMapping() { }
        public bool Any { get { return default(bool); } }
        public bool CheckSpecified { get { return default(bool); } }
        public string ElementName { get { return default(string); } }
        public string MemberName { get { return default(string); } }
        public string Namespace { get { return default(string); } }
        public string TypeFullName { get { return default(string); } }
        public string TypeName { get { return default(string); } }
        public string TypeNamespace { get { return default(string); } }
        public string XsdElementName { get { return default(string); } }
    }
    public partial class XmlMembersMapping : XmlMapping
    {
        internal XmlMembersMapping() { }
        public int Count { get { return default(int); } }
        public XmlMemberMapping this[int index] { get { return default(XmlMemberMapping); } }
        public string TypeName { get { return default(string); } }
        public string TypeNamespace { get { return default(string); } }
    }
    public abstract partial class XmlMapping
    {
        internal XmlMapping() { }
        public string ElementName { get { return default(string); } }
        public string Namespace { get { return default(string); } }
        public string XsdElementName { get { return default(string); } }
        public void SetKey(string key) { }
    }
    public partial class XmlReflectionImporter
    {
        public XmlReflectionImporter() { }
        public XmlReflectionImporter(string defaultNamespace) { }
        public XmlReflectionImporter(XmlAttributeOverrides attributeOverrides) { }
        public XmlReflectionImporter(XmlAttributeOverrides attributeOverrides, string defaultNamespace) { }
        public XmlTypeMapping ImportTypeMapping(Type type) { return default(XmlTypeMapping); }
        public XmlTypeMapping ImportTypeMapping(Type type, string defaultNamespace) { return default(XmlTypeMapping); }
        public XmlTypeMapping ImportTypeMapping(Type type, XmlRootAttribute root) { return default(XmlTypeMapping); }
        public XmlTypeMapping ImportTypeMapping(Type type, XmlRootAttribute root, string defaultNamespace) { return default(XmlTypeMapping); }
        public void IncludeTypes(System.Reflection.MemberInfo memberInfo) { }
        public void IncludeType(Type type) { }
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement) { return default(XmlMembersMapping); }
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc) { return default(XmlMembersMapping); }
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc, bool openModel) { return default(XmlMembersMapping); }
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool rpc, bool openModel, XmlMappingAccess access) { return default(XmlMembersMapping); }
    }
    [Flags]
    public enum XmlMappingAccess
    {
        None = 0x00,
        Read = 0x01,
        Write = 0x02,
    }
    public partial class XmlReflectionMember
    {
        public XmlReflectionMember() { }
        public bool IsReturnValue { get { return default(bool); } set { } }
        public string MemberName { get { return default(string); } set { } }
        public Type MemberType { get { return default(Type); } set { } }
        public bool OverrideIsNullable { get { return default(bool); } set { } }
        public SoapAttributes SoapAttributes { get { return default(SoapAttributes); } set { } }
        public XmlAttributes XmlAttributes { get { return default(XmlAttributes); } set { } }
    }
    public partial class XmlTypeMapping : XmlMapping
    {
        internal XmlTypeMapping() { }
        public string TypeName { get { return default(string); } }
        public string TypeFullName { get { return default(string); } }
        public string XsdTypeName { get { return default(string); } }
        public string XsdTypeNamespace { get { return default(string); } }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public partial class SoapAttributeAttribute : System.Attribute
    {
        public SoapAttributeAttribute() { }
        public SoapAttributeAttribute(string attributeName) { }
        public string AttributeName { get { return default(string); } set { } }
        public string Namespace { get { return default(string); } set { } }
        public string DataType { get { return default(string); } set { } }
    }
    public partial class SoapAttributeOverrides
    {
        public void Add(Type type, SoapAttributes attributes) { }
        public void Add(Type type, string member, SoapAttributes attributes) { }
        public SoapAttributes this[Type type] { get { return default(SoapAttributes); } }
        public SoapAttributes this[Type type, string member] { get { return default(SoapAttributes); } }
    }
    public partial class SoapAttributes
    {
        public SoapAttributes() { }
        public SoapAttributes(System.Reflection.MemberInfo provider) { }
        public SoapTypeAttribute SoapType { get { return default(SoapTypeAttribute); } set { } }
        public SoapEnumAttribute SoapEnum { get { return default(SoapEnumAttribute); } set { } }
        public bool SoapIgnore { get { return default(bool); } set { } }
        public SoapElementAttribute SoapElement { get { return default(SoapElementAttribute); } set { } }
        public SoapAttributeAttribute SoapAttribute { get { return default(SoapAttributeAttribute); } set { } }
        public object SoapDefaultValue { get { return default(object); } set { } }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public partial class SoapElementAttribute : System.Attribute
    {
        public SoapElementAttribute() { }
        public SoapElementAttribute(string elementName) { }
        public string ElementName { get { return default(string); } set { } }
        public string DataType { get { return default(string); } set { } }
        public bool IsNullable { get { return default(bool); } set { } }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public partial class SoapEnumAttribute : System.Attribute
    {
        public SoapEnumAttribute() { }
        public SoapEnumAttribute(string name) { }
        public string Name { get { return default(string); } set { } }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public partial class SoapIgnoreAttribute : System.Attribute
    {
        public SoapIgnoreAttribute() { }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public partial class SoapIncludeAttribute : System.Attribute
    {
        public SoapIncludeAttribute(Type type) { }
        public Type Type { get { return default(Type); } set { } }
    }
    public partial class SoapReflectionImporter
    {
        public SoapReflectionImporter() { }
        public SoapReflectionImporter(string defaultNamespace) { }
        public SoapReflectionImporter(SoapAttributeOverrides attributeOverrides) { }
        public SoapReflectionImporter(SoapAttributeOverrides attributeOverrides, string defaultNamespace) { }
        public void IncludeTypes(System.Reflection.MemberInfo memberInfo) { }
        public void IncludeType(Type type) { }
        public XmlTypeMapping ImportTypeMapping(Type type)
        {
            return default(XmlTypeMapping);
        }
        public XmlTypeMapping ImportTypeMapping(Type type, string defaultNamespace)
        {
            return default(XmlTypeMapping);
        }
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members)
        {
            return default(XmlMembersMapping);
        }
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool writeAccessors)
        {
            return default(XmlMembersMapping);
        }
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool writeAccessors, bool validate)
        {
            return default(XmlMembersMapping);
        }
        public XmlMembersMapping ImportMembersMapping(string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool writeAccessors, bool validate, XmlMappingAccess access)
        {
            return default(XmlMembersMapping);
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct)]
    public partial class SoapTypeAttribute : System.Attribute
    {
        public SoapTypeAttribute() { }
        public SoapTypeAttribute(string typeName) { }
        public SoapTypeAttribute(string typeName, string ns) { }
        public bool IncludeInSchema { get { return default(bool); } set { } }
        public string TypeName { get { return default(string); } set { } }
        public string Namespace { get { return default(string); } set { } }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple=false)]
    public sealed partial class XmlSerializerAssemblyAttribute : System.Attribute {
        public XmlSerializerAssemblyAttribute() {}
        public XmlSerializerAssemblyAttribute(string assemblyName) {}
        public XmlSerializerAssemblyAttribute(string assemblyName, string codeBase) { }
        public string CodeBase { get { return default(string); } set { } }
        public string AssemblyName { get { return default(string); } set { } }
    }
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed partial class XmlSerializerVersionAttribute : System.Attribute
    {
        public XmlSerializerVersionAttribute() { }
        public XmlSerializerVersionAttribute(Type type) { }
        public string ParentAssemblyId { get { return default(string); } set { } }
        public string Version { get { return default(string); } set { } }
        public string Namespace { get { return default(string); } set { } }
        public Type Type { get { return default(Type); } set { } }
    }
}
