﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Xml.Serialization {   
 
    /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class XmlSerializerVersionAttribute : System.Attribute {
        string mvid;
        string serializerVersion;
        string ns;
        Type type;
        
        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.XmlSerializerVersionAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSerializerVersionAttribute() {
        }
        
        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.XmlSerializerAssemblyAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSerializerVersionAttribute(Type type) {
            this.type = type;
        }
        
        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.ParentAssemblyId"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ParentAssemblyId {
            get { return mvid; }
            set { mvid = value; }
        }
 
        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.ParentAssemblyId"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Version {
            get { return serializerVersion; }
            set { serializerVersion = value; }
        }
 
 
        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns; }
            set { ns = value; }
        }
 
        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.TypeName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Type Type {
            get { return type; }
            set { type = value; }
        }
    }
}
