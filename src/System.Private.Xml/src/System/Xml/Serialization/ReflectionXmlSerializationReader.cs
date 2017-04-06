﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Extensions;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;

#if XMLSERIALIZERGENERATOR
namespace Microsoft.XmlSerializer.Generator
#else
namespace System.Xml.Serialization
#endif
{
    internal delegate void UnknownNodeAction(object o);

    internal class ReflectionXmlSerializationReader : XmlSerializationReader
    {
        private static TypeDesc StringTypeDesc { get; set; } = (new TypeScope()).GetTypeDesc(typeof(string));
        private static TypeDesc QnameTypeDesc { get; set; } = (new TypeScope()).GetTypeDesc(typeof(XmlQualifiedName));

        private XmlMapping _mapping;

        public ReflectionXmlSerializationReader(XmlMapping mapping, XmlReader xmlReader, XmlDeserializationEvents events, string encodingStyle)
        {
            Init(xmlReader, events, encodingStyle, tempAssembly: null);
            _mapping = mapping;
        }

        protected override void InitCallbacks()
        {
            TypeScope scope = _mapping.Scope;
            foreach (TypeMapping mapping in scope.TypeMappings)
            {
                if (mapping.IsSoap &&
                        (mapping is StructMapping || mapping is EnumMapping || mapping is ArrayMapping || mapping is NullableMapping) &&
                        !mapping.TypeDesc.IsRoot)
                {
                    AddReadCallback(
                        mapping.TypeName,
                        mapping.Namespace,
                        mapping.TypeDesc.Type,
                        CreateXmlSerializationReadCallback(mapping));
                }
            }
        }

        protected override void InitIDs()
        {
        }

        public object ReadObject()
        {
            var xmlMapping = _mapping;
            if (!xmlMapping.IsReadable)
                return null;
            if (!xmlMapping.GenerateSerializer)
                throw new ArgumentException(SR.Format(SR.XmlInternalError, "xmlMapping"));

            if (xmlMapping is XmlTypeMapping)
                return GenerateTypeElement((XmlTypeMapping)xmlMapping);
            else if (xmlMapping is XmlMembersMapping)
                return GenerateMembersElement((XmlMembersMapping)xmlMapping);
            else
                throw new ArgumentException(SR.Format(SR.XmlInternalError, "xmlMapping"));
        }

        private object GenerateMembersElement(XmlMembersMapping xmlMembersMapping)
        {
            if (xmlMembersMapping.Accessor.IsSoap)
                return GenerateEncodedMembersElement(xmlMembersMapping);
            else
                return GenerateLiteralMembersElement(xmlMembersMapping);
        }

        private object GenerateLiteralMembersElement(XmlMembersMapping xmlMembersMapping)
        {
            ElementAccessor element = xmlMembersMapping.Accessor;
            MemberMapping[] mappings = ((MembersMapping)element.Mapping).Members;
            bool hasWrapperElement = ((MembersMapping)element.Mapping).HasWrapperElement;
            Reader.MoveToContent();

            object[] p = new object[mappings.Length];

            InitializeValueTypes(p, mappings);

            if (hasWrapperElement)
            {
                string elementName = element.Name;
                string elementNs = element.Form == XmlSchemaForm.Qualified ? element.Namespace : "";
                Reader.MoveToContent();
                while (Reader.NodeType != XmlNodeType.EndElement && Reader.NodeType != XmlNodeType.None)
                {
                    if (Reader.IsStartElement(element.Name, elementNs))
                    {
                        if (!GenerateLiteralMembersElementInternal(mappings, hasWrapperElement, p))
                        {
                            continue;
                        }

                        ReadEndElement();
                    }
                    else
                    {

                        UnknownNode(null, $"{elementNs}:{elementName}");
                    }

                    Reader.MoveToContent();
                }
            }
            else
            {
                GenerateLiteralMembersElementInternal(mappings, hasWrapperElement, p);
            }

            return p;
        }

        private void DummySource(object o) { }

        private bool GenerateLiteralMembersElementInternal(MemberMapping[] mappings, bool hasWrapperElement, object[] p)
        {
            Member anyText = null;
            Member anyElement = null;
            Member anyAttribute = null;

            ArrayList membersList = new ArrayList();
            ArrayList textOrArrayMembersList = new ArrayList();
            var attributeMembersList = new List<Member>();

            int pLength = p.Length;
            for (int i = 0; i < mappings.Length; i++)
            {
                int index = i;
                MemberMapping mapping = mappings[index];
                Action<object> source = (o) => p[index] = o;

                //string choiceSource = GetChoiceIdentifierSource(mappings, mapping);
                //Member member = new Member(this, source, arraySource, "a", i, mapping, choiceSource);
                //Member anyMember = new Member(this, source, null, "a", i, mapping, choiceSource);
                Member member = new Member(mapping);
                Member anyMember = new Member(mapping);

                if (mapping.Xmlns != null)
                {
                    var xmlns = new XmlSerializerNamespaces();
                    p[index] = xmlns;
                    member.XmlnsSource = (ns, name) => xmlns.Add(ns, name);
                }

                member.Source = source;
                anyMember.Source = source;

                if (mapping.CheckSpecified == SpecifiedAccessor.ReadWrite)
                {
                    string nameSpecified = mapping.Name + "Specified";
                    for (int j = 0; j < mappings.Length; j++)
                    {
                        if (mappings[j].Name == nameSpecified)
                        {
                            int indexJ = j;
                            member.CheckSpecifiedSource = (o) => p[indexJ] = o;
                        }
                    }
                }

                bool foundAnyElement = false;
                if (mapping.Text != null)
                {
                    anyText = anyMember;
                }

                if (mapping.Attribute != null && mapping.Attribute.Any)
                {
                    anyMember.Collection = new CollectionMember();
                    anyMember.ArraySource = anyMember.Source;
                    anyMember.Source = (item) =>
                    {
                        anyMember.Collection.Add(item);
                    };

                    anyAttribute = anyMember;
                }

                if (mapping.Attribute != null || mapping.Xmlns != null)
                    attributeMembersList.Add(member);
                else if (mapping.Text != null)
                    textOrArrayMembersList.Add(member);

                if (!mapping.IsSequence)
                {
                    for (int j = 0; j < mapping.Elements.Length; j++)
                    {
                        if (mapping.Elements[j].Any && mapping.Elements[j].Name.Length == 0)
                        {
                            anyElement = anyMember;
                            if (mapping.Attribute == null && mapping.Text == null)
                            {
                                throw new NotImplementedException("mapping.Attribute == null && mapping.Text == null");
                                //textOrArrayMembersList.Add(anyMember);
                            }

                            foundAnyElement = true;
                            break;
                        }
                    }
                }

                if (mapping.Attribute != null || mapping.Text != null || foundAnyElement)
                {
                    membersList.Add(anyMember);
                }
                else if (mapping.TypeDesc.IsArrayLike && !(mapping.Elements.Length == 1 && mapping.Elements[0].Mapping is ArrayMapping))
                {
                    anyMember.Collection = new CollectionMember();
                    anyMember.ArraySource = (item) =>
                    {
                        anyMember.Collection.Add(item);
                    };

                    membersList.Add(anyMember);
                    textOrArrayMembersList.Add(anyMember);
                }
                else
                {
                    //if (mapping.TypeDesc.IsArrayLike && !mapping.TypeDesc.IsArray)
                    //    member.ParamsReadSource = null; // collection
                    membersList.Add(member);
                }
            }

            Member[] members = (Member[])membersList.ToArray(typeof(Member));
            Member[] textOrArrayMembers = (Member[])textOrArrayMembersList.ToArray(typeof(Member));

            if (members.Length > 0 && members[0].Mapping.IsReturnValue)
                IsReturnValue = true;

            if (attributeMembersList.Count > 0)
            {
                var attributeMembers = attributeMembersList.ToArray();
                object tempObject = null;
                WriteAttributes(attributeMembers, anyAttribute, UnknownNode, ref tempObject);
                Reader.MoveToElement();
            }

            if (hasWrapperElement)
            {
                if (Reader.IsEmptyElement)
                {
                    Reader.Skip();
                    Reader.MoveToContent();
                    return false;
                }

                Reader.ReadStartElement();
            }
            if (IsSequence(members))
            {
                throw new NotImplementedException();
                //Writer.WriteLine("int state = 0;");
            }

            Reader.MoveToContent();
            while (Reader.NodeType != XmlNodeType.EndElement && Reader.NodeType != XmlNodeType.None)
            {
                WriteMemberElements(members, UnknownNode, UnknownNode, anyElement, anyText, null);
                Reader.MoveToContent();
            }

            foreach (var member in textOrArrayMembers)
            {
                object value = null;
                SetCollectionObjectWithCollectionMember(ref value, member.Collection, member.Mapping.TypeDesc.Type);
                member.Source(value);
            }

            if (anyAttribute != null)
            {
                object value = null;
                SetCollectionObjectWithCollectionMember(ref value, anyAttribute.Collection, anyAttribute.Mapping.TypeDesc.Type);
                anyAttribute.ArraySource(value);
            }

            return true;
        }

        private string ExpectedElements(Member[] members)
        {
            if (IsSequence(members))
                return "null";
            string qnames = string.Empty;
            bool firstElement = true;
            for (int i = 0; i < members.Length; i++)
            {
                Member member = (Member)members[i];
                if (member.Mapping.Xmlns != null)
                    continue;
                if (member.Mapping.Ignore)
                    continue;
                if (member.Mapping.IsText || member.Mapping.IsAttribute)
                    continue;

                ElementAccessor[] elements = member.Mapping.Elements;

                for (int j = 0; j < elements.Length; j++)
                {
                    ElementAccessor e = elements[j];
                    string ns = e.Form == XmlSchemaForm.Qualified ? e.Namespace : "";
                    if (e.Any && (e.Name == null || e.Name.Length == 0)) continue;

                    if (!firstElement)
                        qnames += ", ";
                    qnames += ns + ":" + e.Name;
                    firstElement = false;
                }
            }

            var writer = new StringWriter(CultureInfo.InvariantCulture);
            ReflectionAwareCodeGen.WriteQuotedCSharpString(new IndentedWriter(writer, true), qnames);
            return writer.ToString();
        }

        private void InitializeValueTypes(object[] p, MemberMapping[] mappings)
        {
            for (int i = 0; i < mappings.Length; i++)
            {
                if (!mappings[i].TypeDesc.IsValueType)
                    continue;


                if (mappings[i].TypeDesc.IsOptionalValue && mappings[i].TypeDesc.BaseTypeDesc.UseReflection)
                {
                    p[i] = null;
                }
                else
                {
                    p[i] = ReflectionCreateObject(mappings[i].TypeDesc.Type);
                }
            }
        }

        private object GenerateEncodedMembersElement(XmlMembersMapping xmlMembersMapping)
        {
            ElementAccessor element = xmlMembersMapping.Accessor;
            var membersMapping = (MembersMapping)element.Mapping;
            MemberMapping[] mappings = membersMapping.Members;
            bool hasWrapperElement = membersMapping.HasWrapperElement;
            bool writeAccessors = membersMapping.WriteAccessors;

            Reader.MoveToContent();

            object[] p = new object[mappings.Length];
            InitializeValueTypes(p, mappings);

            bool isEmptyWrapper = true;
            if (hasWrapperElement)
            {
                Reader.MoveToContent();
                while (Reader.NodeType == XmlNodeType.Element)
                {
                    string root = Reader.GetAttribute("root", Soap.Encoding);
                    if (root == null || XmlConvert.ToBoolean(root))
                        break;

                    ReadReferencedElement();
                    Reader.MoveToContent();
                }

                if (membersMapping.ValidateRpcWrapperElement)
                {
                    string name = element.Name;
                    string ns = element.Form == XmlSchemaForm.Qualified ? element.Namespace : "";
                    if (!XmlNodeEqual(Reader, name, ns))
                    {
                        throw CreateUnknownNodeException();
                    }
                }

                isEmptyWrapper = Reader.IsEmptyElement;
                Reader.ReadStartElement();
            }

            var members = new Member[mappings.Length];
            for (int i = 0; i < mappings.Length; i++)
            {
                int index = i;
                MemberMapping mapping = mappings[index];
                var member = new Member(mapping);
                member.Source = (value) => p[index] = value;

                members[index] = member;
                if (mapping.CheckSpecified == SpecifiedAccessor.ReadWrite)
                {
                    string nameSpecified = mapping.Name + "Specified";
                    for (int j = 0; j < mappings.Length; j++)
                    {
                        if (mappings[j].Name == nameSpecified)
                        {
                            int indexOfSpecifiedMember = j;
                            member.CheckSpecifiedSource = (value) => p[indexOfSpecifiedMember] = value;
                            break;
                        }
                    }
                }

            }

            Fixup fixup = WriteMemberFixupBegin(members, p);
            if (members.Length > 0 && members[0].Mapping.IsReturnValue)
            {
                IsReturnValue = true;
            }

            List<CheckTypeSource> checkTypeHrefSource = null;
            if (!hasWrapperElement && !writeAccessors)
            {
                checkTypeHrefSource = new List<CheckTypeSource>();
            }

            Reader.MoveToContent();
            while (Reader.NodeType != XmlNodeType.EndElement && Reader.NodeType != XmlNodeType.None)
            {
                UnknownNodeAction unrecognizedElementSource;
                if (checkTypeHrefSource == null)
                {
                    unrecognizedElementSource = (_) => UnknownNode(p);
                }
                else
                {
                    unrecognizedElementSource = (_) =>
                      {
                          if (Reader.GetAttribute("id", null) != null)
                          {
                              ReadReferencedElement();
                          }
                          else
                          {
                              UnknownNode(p);
                          };
                      };
                }

                WriteMemberElements(members, unrecognizedElementSource, (_) => UnknownNode(p), null, null, fixup: fixup, checkTypeHrefsSource: checkTypeHrefSource);
                Reader.MoveToContent();
            }

            if (!isEmptyWrapper)
            {
                ReadEndElement();
            }

            if (checkTypeHrefSource != null)
            {
                foreach (CheckTypeSource currentySource in checkTypeHrefSource)
                {
                    bool isReferenced = true;
                    bool isObject = currentySource.IsObject;
                    object refObj = isObject ? currentySource.RefObject : GetTarget((string)currentySource.RefObject);
                    if (refObj == null)
                    {
                        continue;
                    }

                    CheckTypeSource checkTypeSource = new CheckTypeSource();
                    checkTypeSource.RefObject = refObj;
                    Type refObjType = refObj.GetType();
                    checkTypeSource.Type = refObjType;
                    checkTypeSource.Id = null;
                    WriteMemberElementsIf(members, null, (_) => isReferenced = false, fixup, checkTypeSource);

                    if (isObject && isReferenced)
                    {
                        Referenced(refObj);
                    }
                }
            }

            ReadReferencedElements();
            return p;
        }

        private object GenerateTypeElement(XmlTypeMapping xmlTypeMapping)
        {
            ElementAccessor element = xmlTypeMapping.Accessor;
            TypeMapping mapping = element.Mapping;

            Reader.MoveToContent();

            var memberMapping = new MemberMapping();
            memberMapping.TypeDesc = mapping.TypeDesc;
            memberMapping.Elements = new ElementAccessor[] { element };

            UnknownNodeAction elementElseAction = CreateUnknownNodeException;
            UnknownNodeAction elseAction = UnknownNode;

            object o = null;
            var holder = new ObjectHolder();
            var member = new Member(memberMapping);
            member.Source = (value) => holder.Object = value;
            WriteMemberElements(new Member[] { member }, elementElseAction, elseAction, element.Any ? member : null, null);
            o = holder.Object;

            if (element.IsSoap)
            {
                Referenced(o);
                ReadReferencedElements();
            }

            return o;
        }

        private void WriteMemberElements(Member[] expectedMembers, UnknownNodeAction elementElseAction, UnknownNodeAction elseAction, Member anyElement, Member anyText, Fixup fixup = null, List<CheckTypeSource> checkTypeHrefsSource = null)
        {
            bool checkType = checkTypeHrefsSource != null;
            if (Reader.NodeType == XmlNodeType.Element)
            {
                if (checkType)
                {
                    // WriteIfNotSoapRoot(elementElseString + " continue;");
                    if (Reader.GetAttribute("root", Soap.Encoding) == "0")
                    {
                        elementElseAction(null);
                        return;
                    }

                    WriteMemberElementsCheckType(checkTypeHrefsSource);
                }
                else
                {
                    WriteMemberElementsIf(expectedMembers, anyElement, elementElseAction, fixup: fixup);
                }
            }
            else if (anyText != null && anyText.Mapping != null && WriteMemberText(anyText))
            {
            }
            else
            {
                ProcessUnknownNode(elseAction);
            }
        }

        private void WriteMemberElementsCheckType(List<CheckTypeSource> checkTypeHrefsSource)
        {
            string refElemId = null;
            object RefElememnt = ReadReferencingElement(null, null, true, out refElemId);
            var source = new CheckTypeSource();
            if (refElemId != null)
            {
                source.RefObject = refElemId;
                source.IsObject = false;
                checkTypeHrefsSource.Add(source);
            }
            else if (RefElememnt != null)
            {
                source.RefObject = RefElememnt;
                source.IsObject = true;
                checkTypeHrefsSource.Add(source);
            }
        }

        private void ProcessUnknownNode(UnknownNodeAction action)
        {
            action?.Invoke(null);
        }

        private void WriteMembers(ref object o, Member[] members, UnknownNodeAction elementElseAction, UnknownNodeAction elseAction, Member anyElement, Member anyText)
        {
            Reader.MoveToContent();

            while (Reader.NodeType != XmlNodeType.EndElement && Reader.NodeType != XmlNodeType.None)
            {
                WriteMemberElements(members, elementElseAction, elseAction, anyElement, anyText);
                Reader.MoveToContent();
            }
        }

        private void SetCollectionObjectWithCollectionMember(ref object collection, CollectionMember collectionMember, Type collectionType)
        {
            if (collectionType.IsArray)
            {
                Array a;
                Type elementType = collectionType.GetElementType();
                var currentArray = collection as Array;
                if (currentArray != null && currentArray.Length == collectionMember.Count)
                {
                    a = currentArray;
                }
                else
                {
                    a = Array.CreateInstance(elementType, collectionMember.Count);
                }

                for (int i = 0; i < collectionMember.Count; i++)
                {
                    a.SetValue(collectionMember[i], i);
                }

                collection = a;
            }
            else
            {
                if (collection == null)
                {
                    collection = ReflectionCreateObject(collectionType);
                }

                AddObjectsIntoTargetCollection(collection, collectionMember, collectionType);
            }
        }

        private static void AddObjectsIntoTargetCollection(object targetCollection, List<object> sourceCollection, Type targetCollectionType)
        {
            var targetList = targetCollection as IList;
            if (targetList != null)
            {
                foreach (object item in sourceCollection)
                {
                    targetList.Add(item);
                }
            }
            else
            {
                MethodInfo addMethod = targetCollectionType.GetMethod("Add");
                if (addMethod == null)
                {
                    throw new InvalidOperationException("addMethod == null");
                }

                var arguments = new object[1];
                foreach (object item in sourceCollection)
                {
                    arguments[0] = item;
                    addMethod.Invoke(targetCollection, arguments);
                }
            }
        }

        private static void SetMemberValue(object o, object value, string memberName)
        {
            MemberInfo[] memberInfos = o.GetType().GetMember(memberName);
            var memberInfo = memberInfos[0];
            SetMemberValue(o, value, memberInfo);
        }

        private static void SetMemberValue(object o, object value, MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
            {
                var propInfo = (PropertyInfo)memberInfo;
                propInfo.SetValue(o, value);
            }
            else if (memberInfo is FieldInfo)
            {
                var fieldInfo = (FieldInfo)memberInfo;
                fieldInfo.SetValue(o, value);
            }
            else
            {
                throw new InvalidOperationException("InvalidMember");
            }
        }

        private object GetMemberValue(object o, MemberInfo memberInfo)
        {
            var memberProperty = memberInfo as PropertyInfo;
            if (memberProperty != null)
            {
                return memberProperty.GetValue(o);
            }

            var memberField = memberInfo as FieldInfo;
            if (memberField != null)
            {
                return memberField.GetValue(o);
            }

            throw new InvalidOperationException("unknown member type");
        }

        private bool WriteMemberText(Member anyText)
        {
            object value;
            MemberMapping anyTextMapping = anyText.Mapping;
            if ((Reader.NodeType == XmlNodeType.Text ||
                        Reader.NodeType == XmlNodeType.CDATA ||
                        Reader.NodeType == XmlNodeType.Whitespace ||
                        Reader.NodeType == XmlNodeType.SignificantWhitespace))
            {
                TextAccessor text = anyTextMapping.Text;

                if (text.Mapping is SpecialMapping)
                {
                    SpecialMapping special = (SpecialMapping)text.Mapping;
                    if (special.TypeDesc.Kind == TypeKind.Node)
                    {
                        value = Document.CreateTextNode(ReadString());
                    }
                    else
                    {
                        throw new InvalidOperationException(SR.Format(SR.XmlInternalError));
                    }
                }
                else
                {
                    if (anyTextMapping.TypeDesc.IsArrayLike)
                    {
                        if (text.Mapping.TypeDesc.CollapseWhitespace)
                        {
                            value = CollapseWhitespace(ReadString());
                        }
                        else
                        {
                            value = ReadString();
                        }
                    }
                    else
                    {
                        if (text.Mapping.TypeDesc == StringTypeDesc || text.Mapping.TypeDesc.FormatterName == "String")
                        {
                            value = ReadString(null, text.Mapping.TypeDesc.CollapseWhitespace);
                        }
                        else
                        {
                            value = WritePrimitive(text.Mapping, () => ReadString());
                        }
                    }
                }

                anyText.Source.Invoke(value);
                return true;
            }

            return false;
        }

        bool IsSequence(Member[] members)
        {
            // #10586: Currently the reflection based method treat this kind of type as normal types. 
            // But potentially we can do some optimization for types that have ordered properties. 

            //for (int i = 0; i < members.Length; i++)
            //{
            //    if (members[i].Mapping.IsParticle && members[i].Mapping.IsSequence)
            //        return true;
            //}
            return false;
        }

        private void WriteMemberElementsIf(Member[] expectedMembers, Member anyElementMember, UnknownNodeAction elementElseAction, Fixup fixup = null, CheckTypeSource checkTypeSource = null)
        {
            bool checkType = checkTypeSource != null;
            bool isSequence = IsSequence(expectedMembers);
            if (isSequence)
            {
                // #10586: Currently the reflection based method treat this kind of type as normal types. 
                // But potentially we can do some optimization for types that have ordered properties.
            }

            ElementAccessor e = null;
            Member member = null;
            bool foundElement = false;
            int elementIndex = -1;
            foreach (var m in expectedMembers)
            {
                if (m.Mapping.Xmlns != null)
                    continue;
                if (m.Mapping.Ignore)
                    continue;
                if (isSequence && (m.Mapping.IsText || m.Mapping.IsAttribute))
                    continue;

                for (int i = 0; i < m.Mapping.Elements.Length; i++)
                {
                    var ele = m.Mapping.Elements[i];
                    string ns = ele.Form == XmlSchemaForm.Qualified ? ele.Namespace : "";
                    if (checkType)
                    {
                        Type elementType;
                        if (ele.Mapping is NullableMapping)
                        {
                            TypeDesc td = ((NullableMapping)ele.Mapping).BaseMapping.TypeDesc;
                            elementType = td.Type;
                        }
                        else
                        {
                            elementType = ele.Mapping.TypeDesc.Type;
                        }

                        if (elementType.IsAssignableFrom(checkTypeSource.Type))
                        {
                            foundElement = true;
                        }
                    }
                    else if (ele.Name == Reader.LocalName && ns == Reader.NamespaceURI)
                    {
                        foundElement = true;
                    }

                    if (foundElement)
                    {
                        e = ele;
                        member = m;
                        elementIndex = i;
                        break;
                    }
                }

                if (foundElement)
                    break;
            }

            if (foundElement)
            {
                if (checkType)
                {
                    member.Source(checkTypeSource.RefObject);

                    if (member.FixupIndex >= 0)
                    {
                        fixup.Ids[member.FixupIndex] = checkTypeSource.Id;
                    }
                }
                else
                {
                    string ns = e.Form == XmlSchemaForm.Qualified ? e.Namespace : "";
                    bool isList = member.Mapping.TypeDesc.IsArrayLike && !member.Mapping.TypeDesc.IsArray;
                    WriteElement(e, member.Mapping.CheckSpecified == SpecifiedAccessor.ReadWrite, isList && member.Mapping.TypeDesc.IsNullable, member.Mapping.ReadOnly, ns, member.FixupIndex, elementIndex, fixup, member);
                }
            }
            else
            {
                if (anyElementMember != null && anyElementMember.Mapping != null)
                {
                    var anyElement = anyElementMember.Mapping;

                    member = anyElementMember;
                    ElementAccessor[] elements = anyElement.Elements;
                    for (int i = 0; i < elements.Length; i++)
                    {
                        ElementAccessor element = elements[i];
                        if (element.Any && element.Name.Length == 0)
                        {
                            string ns = element.Form == XmlSchemaForm.Qualified ? element.Namespace : "";
                            WriteElement(element, anyElement.CheckSpecified == SpecifiedAccessor.ReadWrite, false, false, ns, fixup: fixup, member: member);
                            break;
                        }
                    }
                }
                else
                {
                    member = null;
                    ProcessUnknownNode(elementElseAction);
                }
            }
        }

        private object WriteElement(ElementAccessor element, bool checkSpecified, bool checkForNull, bool readOnly, string defaultNamespace, int fixupIndex = -1, int elementIndex = -1, Fixup fixup = null, Member member = null)
        {
            object value = null;
            if (element.Mapping is ArrayMapping)
            {
                value = WriteArray((ArrayMapping)element.Mapping, readOnly, element.IsNullable, defaultNamespace, fixupIndex, fixup, member);
            }
            else if (element.Mapping is NullableMapping)
            {
                value = WriteNullableMethod((NullableMapping)element.Mapping, true, defaultNamespace);
            }
            else if (!element.Mapping.IsSoap && (element.Mapping is PrimitiveMapping))
            {
                if (element.IsNullable && ReadNull())
                {
                    if (element.Mapping.TypeDesc.IsValueType)
                    {
                        value = ReflectionCreateObject(element.Mapping.TypeDesc.Type);
                    }
                    else
                    {
                        value = null;
                    }
                }
                else if ((element.Default != null && !Globals.IsDBNullValue(element.Default) && element.Mapping.TypeDesc.IsValueType)
                         && (Reader.IsEmptyElement))
                {
                    Reader.Skip();
                }
                else
                {
                    if (element.Mapping.TypeDesc == QnameTypeDesc)
                        value = ReadElementQualifiedName();
                    else
                    {

                        if (element.Mapping.TypeDesc.FormatterName == "ByteArrayBase64")
                        {
                            value = ToByteArrayBase64(false);
                        }
                        else if (element.Mapping.TypeDesc.FormatterName == "ByteArrayHex")
                        {
                            value = ToByteArrayHex(false);
                        }
                        else
                        {
                            Func<string> readFunc = () => Reader.ReadElementContentAsString();

                            value = WritePrimitive(element.Mapping, readFunc);
                        }
                    }
                }
            }
            else if (element.Mapping is StructMapping || (element.Mapping.IsSoap && element.Mapping is PrimitiveMapping))
            {
                TypeMapping mapping = element.Mapping;
                if (mapping.IsSoap)
                {
                    object rre = fixupIndex >= 0 ?
                          ReadReferencingElement(mapping.TypeName, mapping.Namespace, out fixup.Ids[fixupIndex])
                        : ReadReferencedElement(mapping.TypeName, mapping.Namespace);

                    if (!mapping.TypeDesc.IsValueType || rre != null)
                    {
                        value = rre;
                        Referenced(value);
                    }

                    if (fixupIndex >= 0)
                    {
                        if (member == null)
                        {
                            throw new InvalidOperationException("member == null");
                        }

                        member.Source(value);
                        return value;
                    }
                }
                else
                {
                    if (checkForNull && (member.Source == null && member.ArraySource == null))
                    {
                        Reader.Skip();
                    }
                    else
                    {
                        value = WriteStructMethod(
                                mapping: (StructMapping)mapping,
                                isNullable: mapping.TypeDesc.IsNullable && element.IsNullable,
                                checkType: true,
                                defaultNamespace: defaultNamespace
                                );
                    }
                }
            }
            else if (element.Mapping is SpecialMapping)
            {
                SpecialMapping special = (SpecialMapping)element.Mapping;
                switch (special.TypeDesc.Kind)
                {
                    case TypeKind.Node:
                        bool isDoc = special.TypeDesc.FullName == typeof(XmlDocument).FullName;
                        if (isDoc)
                        {
                            value = ReadXmlDocument(!element.Any);
                        }
                        else
                        {
                            value = ReadXmlNode(!element.Any);
                        }

                        break;
                    case TypeKind.Serializable:
                        SerializableMapping sm = (SerializableMapping)element.Mapping;
                        // check to see if we need to do the derivation
                        bool flag = true;
                        if (sm.DerivedMappings != null)
                        {
                            XmlQualifiedName tser = GetXsiType();
                            if (tser == null || QNameEqual(tser, sm.XsiType.Name, sm.XsiType.Namespace, defaultNamespace))
                            {

                            }
                            else
                            {
                                flag = false;
                            }
                        }

                        if (flag)
                        {
                            bool isWrappedAny = !element.Any && IsWildcard(sm);
                            value = ReadSerializable((IXmlSerializable)ReflectionCreateObject(sm.TypeDesc.Type), isWrappedAny);
                        }

                        if (sm.DerivedMappings != null)
                        {
                            // #10587: To Support SpecialMapping Types Having DerivedMappings  
                            throw new NotImplementedException("sm.DerivedMappings != null");
                            //WriteDerivedSerializable(sm, sm, source, isWrappedAny);
                            //WriteUnknownNode("UnknownNode", "null", null, true);
                        }
                        break;
                    default:
                        throw new InvalidOperationException(SR.Format(SR.XmlInternalError));
                }
            }
            else
            {
                throw new InvalidOperationException(SR.Format(SR.XmlInternalError));
            }

            member?.ChoiceSource?.Invoke(element.Name);

            if (member?.ArraySource != null)
            {
                member?.ArraySource(value);
            }
            else
            {
                member?.Source?.Invoke(value);
                member?.CheckSpecifiedSource?.Invoke(true);
            }

            return value;
        }

        private XmlSerializationReadCallback CreateXmlSerializationReadCallback(TypeMapping mapping)
        {
            if (mapping is StructMapping)
            {
                return () => WriteStructMethod((StructMapping)mapping, mapping.TypeDesc.IsNullable, true, defaultNamespace: null);
            }
            else if (mapping is EnumMapping)
            {
                return () => WriteEnumMethodSoap((EnumMapping)mapping);
            }
            else if (mapping is ArrayMapping)
            {
                return DummyReadArrayMethod;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private object DummyReadArrayMethod()
        {
            UnknownNode(null);
            return null;
        }

        private static Type GetMemberType(MemberInfo memberInfo)
        {
            Type memberType;

            if (memberInfo is FieldInfo)
            {
                memberType = ((FieldInfo)memberInfo).FieldType;
            }
            else if (memberInfo is PropertyInfo)
            {
                memberType = ((PropertyInfo)memberInfo).PropertyType;
            }
            else
            {
                throw new InvalidOperationException("unknown member type");
            }

            return memberType;
        }

        private static bool IsWildcard(SpecialMapping mapping)
        {
            if (mapping is SerializableMapping)
                return ((SerializableMapping)mapping).IsAny;
            return mapping.TypeDesc.CanBeElementValue;
        }

        private object WriteArray(ArrayMapping arrayMapping, bool readOnly, bool isNullable, string defaultNamespace, int fixupIndex = -1, Fixup fixup = null, Member member = null)
        {
            object o = null;
            if (arrayMapping.IsSoap)
            {
                object rre;

                if (fixupIndex >= 0)
                {
                    rre = ReadReferencingElement(arrayMapping.TypeName, arrayMapping.Namespace, out fixup.Ids[fixupIndex]);
                }
                else
                {
                    rre = ReadReferencedElement(arrayMapping.TypeName, arrayMapping.Namespace);
                }

                TypeDesc td = arrayMapping.TypeDesc;
                if (td.IsEnumerable || td.IsCollection)
                {
                    if (rre != null)
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    if (member == null)
                    {
                        throw new InvalidOperationException("member == null");
                    }

                    member.Source(rre);
                }

                o = rre;
            }
            else
            {
                if (!ReadNull())
                {
                    MemberMapping memberMapping = new MemberMapping();
                    memberMapping.Elements = arrayMapping.Elements;
                    memberMapping.TypeDesc = arrayMapping.TypeDesc;
                    memberMapping.ReadOnly = readOnly;

                    Type collectionType = memberMapping.TypeDesc.Type;
                    o = ReflectionCreateObject(memberMapping.TypeDesc.Type);

                    if (memberMapping.ChoiceIdentifier != null)
                    {
                        // #10588: To Support ArrayMapping Types Having ChoiceIdentifier
                        throw new NotImplementedException("memberMapping.ChoiceIdentifier != null");
                    }

                    var arrayMember = new Member(memberMapping);
                    arrayMember.Collection = new CollectionMember();
                    arrayMember.ArraySource = (item) =>
                    {
                        arrayMember.Collection.Add(item);
                    };

                    if ((readOnly && o == null) || Reader.IsEmptyElement)
                    {
                        Reader.Skip();
                    }
                    else
                    {
                        Reader.ReadStartElement();
                        Reader.MoveToContent();
                        while (Reader.NodeType != XmlNodeType.EndElement && Reader.NodeType != XmlNodeType.None)
                        {
                            WriteMemberElements(new Member[] { arrayMember }, UnknownNode, UnknownNode, null, null);
                            Reader.MoveToContent();
                        }
                        ReadEndElement();
                    }

                    SetCollectionObjectWithCollectionMember(ref o, arrayMember.Collection, collectionType);
                }
            }

            return o;
        }

        private object WritePrimitive(TypeMapping mapping, Func<string> readFunc)
        {
            if (mapping is EnumMapping)
            {
                if (mapping.IsSoap)
                {
                    // #10676: As all SOAP relates APIs are not in CoreCLR yet, the reflection based method
                    // currently throws PlatformNotSupportedException when types require SOAP serialization.
                    throw new PlatformNotSupportedException();
                }

                return WriteEnumMethod((EnumMapping)mapping, readFunc);
            }
            else if (mapping.TypeDesc == StringTypeDesc)
            {
                return readFunc();
            }
            else if (mapping.TypeDesc.FormatterName == "String")
            {
                if (mapping.TypeDesc.CollapseWhitespace)
                {
                    return CollapseWhitespace(readFunc());
                }
                else
                {
                    return readFunc();
                }
            }
            else
            {
                if (!mapping.TypeDesc.HasCustomFormatter)
                {
                    string value = readFunc();
                    object retObj;
                    switch (mapping.TypeDesc.FormatterName)
                    {
                        case "Boolean":
                            retObj = XmlConvert.ToBoolean(value);
                            break;
                        case "Int32":
                            retObj = XmlConvert.ToInt32(value);
                            break;
                        case "Int16":
                            retObj = XmlConvert.ToInt16(value);
                            break;
                        case "Int64":
                            retObj = XmlConvert.ToInt64(value);
                            break;
                        case "Single":
                            retObj = XmlConvert.ToSingle(value);
                            break;
                        case "Double":
                            retObj = XmlConvert.ToDouble(value);
                            break;
                        case "Decimal":
                            retObj = XmlConvert.ToDecimal(value);
                            break;
                        case "Byte":
                            retObj = XmlConvert.ToByte(value);
                            break;
                        case "SByte":
                            retObj = XmlConvert.ToSByte(value);
                            break;
                        case "UInt16":
                            retObj = XmlConvert.ToUInt16(value);
                            break;
                        case "UInt32":
                            retObj = XmlConvert.ToUInt32(value);
                            break;
                        case "UInt64":
                            retObj = XmlConvert.ToUInt64(value);
                            break;
                        case "Guid":
                            retObj = XmlConvert.ToGuid(value);
                            break;
                        case "Char":
                            retObj = XmlConvert.ToChar(value);
                            break;
                        case "TimeSpan":
                            retObj = XmlConvert.ToTimeSpan(value);
                            break;
                        default:
                            throw new InvalidOperationException(SR.Format(SR.XmlInternalErrorDetails, $"unknown FormatterName: {mapping.TypeDesc.FormatterName}"));
                    }

                    return retObj;
                }
                else
                {
                    string methodName = "To" + mapping.TypeDesc.FormatterName;
                    MethodInfo method = typeof(XmlSerializationReader).GetMethod(methodName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, new Type[] { typeof(string) });
                    if (method == null)
                    {
                        throw new InvalidOperationException(SR.Format(SR.XmlInternalErrorDetails, $"unknown FormatterName: {mapping.TypeDesc.FormatterName}"));
                    }

                    return method.Invoke(this, new object[] { readFunc() });
                }
            }
        }

        object WriteStructMethod(StructMapping mapping, bool isNullable, bool checkType, string defaultNamespace)
        {
            if (mapping.IsSoap)
                return WriteEncodedStructMethod(mapping);
            else
                return WriteLiteralStructMethod(mapping, isNullable, checkType, defaultNamespace);
        }

        object WriteNullableMethod(NullableMapping nullableMapping, bool checkType, string defaultNamespace)
        {
            object o = Activator.CreateInstance(nullableMapping.TypeDesc.Type);
            if (!ReadNull())
            {
                ElementAccessor element = new ElementAccessor();
                element.Mapping = nullableMapping.BaseMapping;
                element.Any = false;
                element.IsNullable = nullableMapping.BaseMapping.TypeDesc.IsNullable;

                o = WriteElement(element, false, false, false, defaultNamespace);
            }

            return o;
        }

        private object WriteEnumMethod(EnumMapping mapping, Func<string> readFunc)
        {
            Debug.Assert(!mapping.IsSoap, "mapping.IsSoap was true. Use WriteEnumMethodSoap for reading SOAP encoded enum value.");
            string source = readFunc();
            return WriteEnumMethod(mapping, source);
        }

        private object WriteEnumMethodSoap(EnumMapping mapping)
        {
            string source = Reader.ReadElementString();
            return WriteEnumMethod(mapping, source);
        }

        private object WriteEnumMethod(EnumMapping mapping, string source)
        {
            if (mapping.IsFlags)
            {
                Hashtable table = WriteHashtable(mapping, mapping.TypeDesc.Name);
                return Enum.ToObject(mapping.TypeDesc.Type, ToEnum(source, table, mapping.TypeDesc.Name));
            }
            else
            {
                foreach (var c in mapping.Constants)
                {
                    if (string.Equals(c.XmlName, source))
                    {
                        return Enum.Parse(mapping.TypeDesc.Type, c.Name);
                    }
                }

                throw CreateUnknownConstantException(source, mapping.TypeDesc.Type);
            }
        }

        private Hashtable WriteHashtable(EnumMapping mapping, string name)
        {
            var h = new Hashtable();

            ConstantMapping[] constants = mapping.Constants;

            for (int i = 0; i < constants.Length; i++)
            {
                h.Add(constants[i].XmlName, constants[i].Value);
            }

            return h;
        }

        private object ReflectionCreateObject(Type type)
        {
            object obj = null;

            if (type.IsArray)
            {
                obj = Activator.CreateInstance(type, 32);
            }
            else
            {
                ConstructorInfo ci = GetDefaultConstructor(type);
                if (ci != null)
                {
                    obj = ci.Invoke(Array.Empty<object>());
                }
                else
                {
                    obj = Activator.CreateInstance(type);
                }
            }

            return obj;
        }

        private ConstructorInfo GetDefaultConstructor(Type type)
        {
            if (type.IsValueType)
                return null;

            ConstructorInfo ctor = FindDefaultConstructor(type.GetTypeInfo());
            return ctor;
        }

        private static ConstructorInfo FindDefaultConstructor(TypeInfo ti)
        {
            foreach (var ci in ti.DeclaredConstructors)
            {
                if (!ci.IsStatic && ci.GetParameters().Length == 0)
                {
                    return ci;
                }
            }
            return null;
        }

        private object WriteEncodedStructMethod(StructMapping structMapping)
        {
            if (structMapping.TypeDesc.IsRoot)
                return null;

            Member[] members = null;

            if (structMapping.TypeDesc.IsAbstract)
            {
                throw CreateAbstractTypeException(structMapping.TypeName, structMapping.Namespace);
            }
            else
            {
                object o = ReflectionCreateObject(structMapping.TypeDesc.Type);

                MemberMapping[] mappings = TypeScope.GetSettableMembers(structMapping);
                members = new Member[mappings.Length];
                for (int i = 0; i < mappings.Length; i++)
                {
                    MemberMapping mapping = mappings[i];
                    // Member member = new Member(this, source, source, "a", i, mapping, GetChoiceIdentifierSource(mapping, "o", structMapping.TypeDesc));
                    var member = new Member(mapping);
                    member.Source = (value) =>
                    {
                        SetMemberValue(o, value, member.Mapping.MemberInfo);
                    };

                    members[i] = member;
                }

                Fixup fixup = WriteMemberFixupBegin(members, o);

                Action<object> unknownNodeAction = (n) => UnknownNode(n);
                WriteAttributes(members, null, unknownNodeAction, ref o);
                Reader.MoveToElement();
                if (Reader.IsEmptyElement)
                {
                    Reader.Skip();
                    return o;
                }

                Reader.ReadStartElement();

                Reader.MoveToContent();
                while (Reader.NodeType != XmlNodeType.EndElement && Reader.NodeType != XmlNodeType.None)
                {
                    WriteMemberElements(members, UnknownNode, UnknownNode, null, null, fixup: fixup);
                    Reader.MoveToContent();
                }

                ReadEndElement();
                return o;
            }
        }

        private Fixup WriteMemberFixupBegin(Member[] members, object o)
        {
            int fixupCount = 0;
            foreach (var member in members)
            {
                if (member.Mapping.Elements.Length == 0)
                    continue;

                TypeMapping mapping = member.Mapping.Elements[0].Mapping;
                if (mapping is StructMapping || mapping is ArrayMapping || mapping is PrimitiveMapping || mapping is NullableMapping)
                {
                    member.MultiRef = true;
                    member.FixupIndex = fixupCount++;
                }
            }

            Fixup fixup;
            if (fixupCount > 0)
            {
                fixup = new Fixup(o, CreateWriteFixupMethod(members), fixupCount);
                AddFixup(fixup);
            }
            else
            {
                fixup = null;
            }

            return fixup;
        }

        private XmlSerializationFixupCallback CreateWriteFixupMethod(Member[] members)
        {
            return (fixupObject) =>
            {
                var fixup = (Fixup)fixupObject;
                object o = fixup.Source;
                string[] ids = fixup.Ids;
                foreach (var member in members)
                {
                    if (member.MultiRef)
                    {
                        int fixupIndex = member.FixupIndex;
                        if (ids[fixupIndex] != null)
                        {
                            var memberValue = GetTarget(ids[fixupIndex]);
                            TypeDesc td = member.Mapping.TypeDesc;
                            if (td.IsCollection || td.IsEnumerable)
                            {
                                WriteAddCollectionFixup(o, member, memberValue);
                            }
                            else
                            {
                                SetMemberValue(o, memberValue, member.Mapping.Name);
                            }
                        }
                    }
                }
            };
        }

        private void WriteAddCollectionFixup(object o, Member member, object memberValue)
        {
            TypeDesc typeDesc = member.Mapping.TypeDesc;
            bool readOnly = member.Mapping.ReadOnly;
            object memberSource = GetMemberValue(o, member.Mapping.MemberInfo);
            if (memberSource == null)
            {
                if (readOnly)
                {
                    throw CreateReadOnlyCollectionException(typeDesc.CSharpName);
                }

                memberSource = ReflectionCreateObject(typeDesc.Type);
                SetMemberValue(o, memberSource, member.Mapping.MemberInfo);
            }

            var collectionFixup = new CollectionFixup(
                memberSource,
                new XmlSerializationCollectionFixupCallback(GetCreateCollectionOfObjectsCallback(typeDesc.Type)),
                memberValue);

            AddFixup(collectionFixup);
        }

        private XmlSerializationCollectionFixupCallback GetCreateCollectionOfObjectsCallback(Type collectionType)
        {
            return (collection, collectionItems) =>
            {
                if (collectionItems == null)
                    return;

                if (collection == null)
                    return;

                var listOfItems = new List<object>();
                var enumerableItems = collectionItems as IEnumerable;
                if (enumerableItems != null)
                {
                    foreach (var item in enumerableItems)
                    {
                        listOfItems.Add(item);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

                AddObjectsIntoTargetCollection(collection, listOfItems, collectionType);
            };
        }

        private object WriteLiteralStructMethod(StructMapping structMapping, bool isNullable, bool checkType, string defaultNamespace)
        {
            XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable)
            {
                isNull = ReadNull();
            }

            if (checkType)
            {
                if (structMapping.TypeDesc.IsRoot && isNull)
                {
                    if (xsiType != null)
                        return ReadTypedNull(xsiType);

                    else
                    {
                        if (structMapping.TypeDesc.IsValueType)
                        {
                            return ReflectionCreateObject(structMapping.TypeDesc.Type);

                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                object o = null;
                if (xsiType == null || (!structMapping.TypeDesc.IsRoot && QNameEqual(xsiType, structMapping.TypeName, structMapping.Namespace, defaultNamespace)))
                {
                    if (structMapping.TypeDesc.IsRoot)
                    {
                        return ReadTypedPrimitive(new XmlQualifiedName(Soap.UrType, XmlReservedNs.NsXs));
                    }
                }
                else if (WriteDerivedTypes(out o, structMapping, xsiType, defaultNamespace, checkType, isNullable))
                {
                    return o;
                }
                else if (structMapping.TypeDesc.IsRoot && WriteEnumAndArrayTypes(out o, structMapping, xsiType, defaultNamespace))
                {
                    return o;
                }
                else
                {
                    if (structMapping.TypeDesc.IsRoot)
                        return ReadTypedPrimitive(xsiType);
                    else
                        throw CreateUnknownTypeException(xsiType);
                }
            }

            if (structMapping.TypeDesc.IsNullable && isNull)
            {
                return null;
            }
            else if (structMapping.TypeDesc.IsAbstract)
            {
                throw CreateAbstractTypeException(structMapping.TypeName, structMapping.Namespace);
            }
            else
            {
                if (structMapping.TypeDesc.Type != null && typeof(XmlSchemaObject).IsAssignableFrom(structMapping.TypeDesc.Type))
                {
                    // #10589: To Support Serializing XmlSchemaObject
                    throw new NotImplementedException("typeof(XmlSchemaObject)");
                }

                object o = ReflectionCreateObject(structMapping.TypeDesc.Type);

                MemberMapping[] mappings = TypeScope.GetSettableMembers(structMapping);
                MemberMapping anyText = null;
                MemberMapping anyElement = null;
                Member anyAttribute = null;
                Member anyElementMember = null;
                Member anyTextMember = null;

                bool isSequence = structMapping.HasExplicitSequence();

                if (isSequence)
                {
                    // #10586: Currently the reflection based method treat this kind of type as normal types. 
                    // But potentially we can do some optimization for types that have ordered properties.
                }

                var allMembersList = new List<Member>(mappings.Length);
                var allMemberMappingList = new List<MemberMapping>(mappings.Length);

                for (int i = 0; i < mappings.Length; i++)
                {
                    MemberMapping mapping = mappings[i];
                    var member = new Member(mapping);

                    if (mapping.Text != null)
                    {
                        anyText = mapping;
                    }

                    if (mapping.Attribute != null)
                    {
                        member.Source = (value) => SetOrAddValueToMember(o, value, member.Mapping.MemberInfo);
                        if (mapping.Attribute.Any)
                        {
                            anyAttribute = member;
                        }
                    }

                    if (!isSequence)
                    {
                        // find anyElement if present.
                        for (int j = 0; j < mapping.Elements.Length; j++)
                        {
                            if (mapping.Elements[j].Any && (mapping.Elements[j].Name == null || mapping.Elements[j].Name.Length == 0))
                            {
                                anyElement = mapping;
                                break;
                            }
                        }
                    }
                    else if (mapping.IsParticle && !mapping.IsSequence)
                    {
                        StructMapping declaringMapping;
                        structMapping.FindDeclaringMapping(mapping, out declaringMapping, structMapping.TypeName);
                        throw new InvalidOperationException(SR.Format(SR.XmlSequenceHierarchy, structMapping.TypeDesc.FullName, mapping.Name, declaringMapping.TypeDesc.FullName, "Order"));
                    }

                    if (mapping.TypeDesc.IsArrayLike)
                    {
                        if (member.Source == null && mapping.TypeDesc.IsArrayLike && !(mapping.Elements.Length == 1 && mapping.Elements[0].Mapping is ArrayMapping))
                        {
                            member.Source = (item) =>
                            {
                                if (member.Collection == null)
                                {
                                    member.Collection = new CollectionMember();
                                }

                                member.Collection.Add(item);
                            };
                            member.ArraySource = member.Source;
                        }
                        else if (!mapping.TypeDesc.IsArray)
                        {

                        }
                    }

                    if (member.Source == null)
                    {
                        PropertyInfo pi = member.Mapping.MemberInfo as PropertyInfo;
                        if (pi != null && typeof(IList).IsAssignableFrom(pi.PropertyType)
                            && (pi.SetMethod == null || !pi.SetMethod.IsPublic))
                        {
                            member.Source = (value) =>
                            {
                                var getOnlyList = (IList)pi.GetValue(o);

                                var valueList = value as IList;
                                if (valueList != null)
                                {
                                    foreach (var v in valueList)
                                    {
                                        getOnlyList.Add(v);
                                    }
                                }
                                else
                                {
                                    getOnlyList.Add(value);
                                }
                            };
                        }
                        else
                        {
                            if (member.Mapping.Xmlns != null)
                            {
                                var xmlSerializerNamespaces = new XmlSerializerNamespaces();
                                SetMemberValue(o, xmlSerializerNamespaces, member.Mapping.Name);
                                member.XmlnsSource = (ns, name) =>
                                {
                                    xmlSerializerNamespaces.Add(ns, name);
                                };
                            }
                            else
                            {
                                member.Source = (value) => SetMemberValue(o, value, member.Mapping.Name);
                            }
                        }
                    }

                    if (member.Mapping.CheckSpecified == SpecifiedAccessor.ReadWrite)
                    {
                        member.CheckSpecifiedSource = (_) =>
                        {
                            string specifiedMemberName = member.Mapping.Name + "Specified";
                            MethodInfo specifiedMethodInfo = o.GetType().GetMethod("set_" + specifiedMemberName);
                            if (specifiedMethodInfo != null)
                            {
                                specifiedMethodInfo.Invoke(o, new object[] { true });
                            }
                        };
                    }

                    ChoiceIdentifierAccessor choice = mapping.ChoiceIdentifier;
                    if (choice != null && o != null)
                    {
                        member.ChoiceSource = (elementNameObject) =>
                        {
                            string elementName = elementNameObject as string;
                            foreach (var name in choice.MemberIds)
                            {
                                if (name == elementName)
                                {
                                    object choiceValue = Enum.Parse(choice.Mapping.TypeDesc.Type, name);
                                    SetOrAddValueToMember(o, choiceValue, choice.MemberInfo);

                                    break;
                                }
                            }
                        };
                    }

                    allMemberMappingList.Add(mapping);
                    allMembersList.Add(member);

                    if (mapping == anyElement)
                    {
                        anyElementMember = member;
                    }
                    else if (mapping == anyText)
                    {
                        anyTextMember = member;
                    }
                }

                var allMembers = allMembersList.ToArray();
                var allMemberMappings = allMemberMappingList.ToArray();

                Action<object> unknownNodeAction = (n) => UnknownNode(n);
                WriteAttributes(allMembers, anyAttribute, unknownNodeAction, ref o);

                Reader.MoveToElement();
                if (Reader.IsEmptyElement)
                {
                    Reader.Skip();
                    return o;
                }

                Reader.ReadStartElement();
                bool IsSequenceAllMembers = IsSequence(allMembers);
                if (IsSequenceAllMembers)
                {
                    // #10586: Currently the reflection based method treat this kind of type as normal types. 
                    // But potentially we can do some optimization for types that have ordered properties.
                }

                WriteMembers(ref o, allMembers, UnknownNode, UnknownNode, anyElementMember, anyTextMember);

                foreach (var member in allMembers)
                {
                    if (member.Collection != null)
                    {
                        MemberInfo[] memberInfos = o.GetType().GetMember(member.Mapping.Name);
                        var memberInfo = memberInfos[0];
                        object collection = null;
                        SetCollectionObjectWithCollectionMember(ref collection, member.Collection, member.Mapping.TypeDesc.Type);
                        SetMemberValue(o, collection, memberInfo);
                    }
                }

                ReadEndElement();
                return o;
            }
        }

        private bool WriteEnumAndArrayTypes(out object o, StructMapping mapping, XmlQualifiedName xsiType, string defaultNamespace)
        {
            foreach (var m in _mapping.Scope.TypeMappings)
            {
                var em = m as EnumMapping;
                if (em != null)
                {
                    if (QNameEqual(xsiType, em.TypeName, em.Namespace, defaultNamespace))
                    {
                        Reader.ReadStartElement();
                        o = WriteEnumMethod(em, () => (CollapseWhitespace(this.ReadString())));
                        ReadEndElement();
                        return true;
                    }

                    continue;
                }

                var am = m as ArrayMapping;
                if (am != null)
                {
                    if (QNameEqual(xsiType, am.TypeName, am.Namespace, defaultNamespace))
                    {
                        o = WriteArray(am, false, false, defaultNamespace);
                        return true;
                    }

                    continue;
                }
            }

            o = null;
            return false;
        }

        bool WriteDerivedTypes(out object o, StructMapping mapping, XmlQualifiedName xsiType, string defaultNamespace, bool checkType, bool isNullable)
        {
            for (StructMapping derived = mapping.DerivedMappings; derived != null; derived = derived.NextDerivedMapping)
            {
                if (QNameEqual(xsiType, derived.TypeName, derived.Namespace, defaultNamespace))
                {
                    o = WriteStructMethod(derived, isNullable, checkType, defaultNamespace);
                    return true;
                }

                if (WriteDerivedTypes(out o, derived, xsiType, defaultNamespace, checkType, isNullable))
                {
                    return true;
                }
            }

            o = null;
            return false;
        }

        private void WriteAttributes(Member[] members, Member anyAttribute, Action<object> elseCall, ref object o)
        {
            Member xmlnsMember = null;
            var attributes = new List<AttributeAccessor>();
            foreach (var member in members)
            {
                if (member.Mapping.Xmlns != null)
                {
                    xmlnsMember = member;
                    break;
                }
            }

            while (Reader.MoveToNextAttribute())
            {
                bool memberFound = false;
                foreach (var member in members)
                {
                    if (member.Mapping.Xmlns != null || member.Mapping.Ignore)
                    {
                        continue;
                    }

                    AttributeAccessor attribute = member.Mapping.Attribute;

                    if (attribute == null) continue;
                    if (attribute.Any) continue;

                    attributes.Add(attribute);

                    if (attribute.IsSpecialXmlNamespace)
                    {
                        memberFound = XmlNodeEqual(Reader, attribute.Name, XmlReservedNs.NsXml);
                    }
                    else
                    {
                        memberFound = XmlNodeEqual(Reader, attribute.Name, attribute.Form == XmlSchemaForm.Qualified ? attribute.Namespace : "");
                    }

                    if (memberFound)
                    {
                        WriteAttribute(member);
                        memberFound = true;
                        break;
                    }
                }

                if (memberFound)
                {
                    continue;
                }

                bool flag2 = false;
                if (xmlnsMember != null)
                {
                    if (IsXmlnsAttribute(Reader.Name))
                    {
                        Debug.Assert(xmlnsMember.XmlnsSource != null, "Xmlns member's source was not set.");
                        xmlnsMember.XmlnsSource(Reader.Name.Length == 5 ? "" : Reader.LocalName, Reader.Value);
                    }
                    else
                    {
                        flag2 = true;
                    }
                }
                else if (!IsXmlnsAttribute(Reader.Name))
                {
                    flag2 = true;
                }

                if (flag2)
                {
                    if (anyAttribute != null)
                    {
                        var attr = Document.ReadNode(Reader) as XmlAttribute;
                        ParseWsdlArrayType(attr);
                        WriteAttribute(anyAttribute, attr);
                    }
                    else
                    {

                        elseCall(o);
                    }
                }
            }
        }

        private void WriteAttribute(Member member, object attr = null)
        {
            AttributeAccessor attribute = member.Mapping.Attribute;
            object value = null;
            if (attribute.Mapping is SpecialMapping)
            {
                SpecialMapping special = (SpecialMapping)attribute.Mapping;

                if (special.TypeDesc.Kind == TypeKind.Attribute)
                {
                    value = attr;
                }
                else if (special.TypeDesc.CanBeAttributeValue)
                {
                    // #10590: To Support special.TypeDesc.CanBeAttributeValue == true
                    throw new NotImplementedException("special.TypeDesc.CanBeAttributeValue");
                }
                else
                    throw new InvalidOperationException(SR.Format(SR.XmlInternalError));
            }
            else
            {
                if (attribute.IsList)
                {

                    string listValues = Reader.Value;
                    string[] vals = listValues.Split(null);
                    Array arrayValue = Array.CreateInstance(member.Mapping.TypeDesc.Type.GetElementType(), vals.Length);
                    for (int i = 0; i < vals.Length; i++)
                    {
                        arrayValue.SetValue(WritePrimitive(attribute.Mapping, () => vals[i]), i);
                    }

                    value = arrayValue;
                }
                else
                {
                    value = WritePrimitive(attribute.Mapping, () => Reader.Value);
                }
            }

            member.Source(value);

            if (member.Mapping.CheckSpecified == SpecifiedAccessor.ReadWrite)
            {
                // #10591: we need to add tests for this block.
                member.CheckSpecifiedSource?.Invoke(null);
            }
        }

        private void SetOrAddValueToMember(object o, object value, MemberInfo memberInfo)
        {
            Type memberType = GetMemberType(memberInfo);

            if (memberType == value.GetType())
            {
                SetMemberValue(o, value, memberInfo);
            }
            else if (memberType.IsArray)
            {
                AddItemInArrayMember(o, memberInfo, memberType, value);
            }
            else
            {
                SetMemberValue(o, value, memberInfo);
            }
        }

        private void AddItemInArrayMember(object o, MemberInfo memberInfo, Type memberType, object item)
        {
            Debug.Assert(memberType.IsArray);

            Array currentArray = (Array)GetMemberValue(o, memberInfo);

            int length;
            if (currentArray == null)
            {
                length = 0;
            }
            else
            {
                length = currentArray.Length;
            }

            var newArray = Array.CreateInstance(memberType.GetElementType(), length + 1);
            if (currentArray != null)
            {
                Array.Copy(currentArray, newArray, length);
            }

            newArray.SetValue(item, length);
            SetMemberValue(o, newArray, memberInfo);
        }

        // WriteXmlNodeEqual
        private bool XmlNodeEqual(XmlReader source, string name, string ns)
        {
            return source.LocalName == name && string.Equals(source.NamespaceURI, ns);
        }

        private bool QNameEqual(XmlQualifiedName xsiType, string name, string ns, string defaultNamespace)
        {
            return xsiType.Name == name && string.Equals(xsiType.Namespace, defaultNamespace);
        }

        private void CreateUnknownNodeException(object o)
        {
            CreateUnknownNodeException();
        }

        internal class CollectionMember : List<object>
        {
        }

        internal class Member
        {
            public MemberMapping Mapping;
            public CollectionMember Collection;
            public int FixupIndex = -1;
            public bool MultiRef = false;
            public Action<object> Source;
            public Action<object> ArraySource;
            public Action<object> CheckSpecifiedSource;
            public Action<object> ChoiceSource;
            public Action<string, string> XmlnsSource;

            public Member(MemberMapping mapping)
            {
                Mapping = mapping;
            }

            public Member(MemberMapping mapping, CollectionMember collectionMember) : this(mapping)
            {
                Collection = collectionMember;
            }
        }

        internal class CheckTypeSource
        {
            public string Id { get; set; }
            public bool IsObject { get; set; }
            public Type Type { get; set; }
            public object RefObject { get; set; }
        }

        internal class ObjectHolder
        {
            public Object Object;
        }
    }
}
